using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EmailContentFormatter = HappyTravel.Edo.Api.Infrastructure.EmailContentFormatter;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingMailingService : IBookingMailingService
    {
        public BookingMailingService(MailSenderWithCompanyInfo mailSender,
            IBookingDocumentsService bookingDocumentsService,
            IBookingRecordsManager bookingRecordsManager,
            IOptions<BookingMailingOptions> options,
            IDateTimeProvider dateTimeProvider,
            IAgentSettingsManager agentSettingsManager,
            IAccountPaymentService accountPaymentService,
            EdoContext context)
        {
            _bookingDocumentsService = bookingDocumentsService;
            _bookingRecordsManager = bookingRecordsManager;
            _mailSender = mailSender;
            _options = options.Value;
            _dateTimeProvider = dateTimeProvider;
            _agentSettingsManager = agentSettingsManager;
            _accountPaymentService = accountPaymentService;
            _context = context;
        }


        public Task<Result> SendVoucher(int bookingId, string email, AgentContext agent, string languageCode)
        {
            return _bookingDocumentsService.GenerateVoucher(bookingId, agent, languageCode)
                .Bind(voucher =>
                {
                    var voucherData = new VoucherData
                    {
                        Accommodation = voucher.Accommodation,
                        AgentName = voucher.AgentName,
                        BookingId = voucher.BookingId,
                        DeadlineDate = EmailContentFormatter.FromDate(voucher.DeadlineDate),
                        NightCount = voucher.NightCount,
                        ReferenceCode = voucher.ReferenceCode,
                        RoomDetails = voucher.RoomDetails,
                        CheckInDate = EmailContentFormatter.FromDate(voucher.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(voucher.CheckOutDate),
                        MainPassengerName = voucher.MainPassengerName
                    };

                    return SendEmail(email, _options.VoucherTemplateId, voucherData);
                });
        }


        public Task<Result> SendInvoice(int bookingId, string email, AgentContext agent, string languageCode)
        {
            // TODO: hardcoded to be removed with UEDA-20
            var addresses = new List<string> {email};
            addresses.AddRange(_options.CcNotificationAddresses);
            
            return _bookingDocumentsService.GetActualInvoice(bookingId, agent)
                .Bind(invoice =>
                {
                    var (registrationInfo, data) = invoice;
                    var invoiceData = new InvoiceData
                    {
                        Number = registrationInfo.Number,
                        BuyerDetails = data.BuyerDetails,
                        InvoiceDate = EmailContentFormatter.FromDate(registrationInfo.Date),
                        InvoiceItems = data.InvoiceItems
                            .Select(i => new InvoiceData.InvoiceItem
                            {
                                Number = i.Number,
                                Price = FormatPrice(i.Price),
                                Total = FormatPrice(i.Total),
                                AccommodationName = i.AccommodationName,
                                RoomDescription = i.RoomDescription
                            })
                            .ToList(),
                        TotalPrice = FormatPrice(data.TotalPrice),
                        CurrencyCode = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(data.TotalPrice.Currency),
                        ReferenceCode = data.ReferenceCode,
                        SellerDetails = data.SellerDetails,
                        PayDueDate = EmailContentFormatter.FromDate(data.PayDueDate)
                    };

                    return _mailSender.Send(_options.InvoiceTemplateId, addresses, invoiceData);
                });
        }


        public async Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo)
        {
            var agentNotificationTemplate = _options.BookingCancelledTemplateId;
            await SendDetailedBookingNotification(bookingInfo, agentNotificationTemplate);
            
            var adminNotificationTemplate = _options.ReservationsBookingCancelledTemplateId;
            await SendDetailedBookingNotification(bookingInfo, adminNotificationTemplate);
        }


        // TODO: hardcoded to be removed with UEDA-20
        public async Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo)
        {
            var agentNotificationTemplate = _options.BookingFinalizedTemplateId;
            await SendDetailedBookingNotification(bookingInfo, agentNotificationTemplate);
            
            var adminNotificationTemplate = _options.ReservationsBookingFinalizedTemplateId;
            await SendDetailedBookingNotification(bookingInfo, adminNotificationTemplate);
        }


        public Task<Result> NotifyDeadlineApproaching(int bookingId, string email)
        {
            return _bookingRecordsManager.Get(bookingId)
                .Bind(booking =>
                {
                    var roomDescriptions = booking.Rooms
                        .Select(r => r.ContractDescription);

                    var passengers = booking.Rooms
                        .SelectMany(r => r.Passengers)
                        .Select(p => $"{p.FirstName} {p.LastName}");
                    
                    var deadlineData = new BookingDeadlineData
                    {
                        BookingId = booking.Id,
                        RoomDescriptions = string.Join(", ", roomDescriptions),
                        Passengers = string.Join(", ", passengers),
                        ReferenceCode = booking.ReferenceCode,
                        CheckInDate = EmailContentFormatter.FromDate(booking.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(booking.CheckOutDate),
                        Deadline = EmailContentFormatter.FromDate(booking.DeadlineDate)
                    };
                    
                    return SendEmail(email, _options.DeadlineNotificationTemplateId, deadlineData);
                });
        }


        public async Task<Result<string>> SendBookingReports(int agencyId)
        {
            DateTime reportBeginDate = _dateTimeProvider.UtcNow().Date;

            return await GetEmailsAndSettings()
                .Map(GetBookings)
                .Bind(CreateMailData)
                .Bind(SendMails);
            

            async Task<Result<List<EmailAndSetting>>> GetEmailsAndSettings()
            {
                var emailsAndSettings = await
                    (from relation in _context.AgentAgencyRelations
                        join agent in _context.Agents
                            on relation.AgentId equals agent.Id
                        where relation.AgencyId == agencyId
                            && relation.InAgencyPermissions.HasFlag(InAgencyPermissions.ReceiveBookingSummary)
                        select new EmailAndSetting
                        {
                            Email = agent.Email, 
                            ReportDaysSetting = _agentSettingsManager.GetUserSettings(agent).BookingReportDays
                        }).ToListAsync();

                return emailsAndSettings.Any()
                    ? Result.Success(emailsAndSettings)
                    : Result.Failure<List<EmailAndSetting>>($"Couldn't find any agents in agency with id {agencyId} to send summary to");
            }


            async Task<(List<EmailAndSetting>, List<Booking>)> GetBookings(List<EmailAndSetting> emailsAndSettings)
            {
                var maxPeriod = emailsAndSettings.Max(t => t.ReportDaysSetting);
                var reportMaxEndDate = reportBeginDate.AddDays(maxPeriod);

                var bookings = await _context.Bookings.Where(b => b.AgencyId == agencyId
                    && b.PaymentMethod == PaymentMethods.BankTransfer
                    && b.PaymentStatus != BookingPaymentStatuses.Captured
                    && BookingStatusesForSummary.Contains(b.Status)
                    && b.DeadlineDate < reportMaxEndDate).ToListAsync();

                return (emailsAndSettings, bookings);
            }


            async Task<Result<List<(BookingSummaryNotificationData, string)>>> CreateMailData((List<EmailAndSetting> emailsAndSettings, List<Booking> bookings) values)
            {
                var (_, isFailure, balanceInfo, error) = await _accountPaymentService.GetAccountBalance(Currencies.USD, agencyId);
                if (isFailure)
                    return Result.Failure<List<(BookingSummaryNotificationData, string)>>(
                        $"Couldn't retrieve account balance for agency with id {agencyId}. Error: {error}");

                var agencyBalance = balanceInfo.Balance;

                return values.emailsAndSettings.Select(emailAndSetting =>
                {
                    var reportEndDate = reportBeginDate.AddDays(emailAndSetting.ReportDaysSetting);
                    var includedBookings = values.bookings.Where(b => b.DeadlineDate < reportEndDate).ToList();

                    var resultingBalance = agencyBalance - includedBookings.Sum(b => b.TotalPrice);

                    return (new BookingSummaryNotificationData
                        {
                            Bookings = includedBookings.OrderBy(b => b.DeadlineDate).Select(CreateBookingData).ToList(),
                            CurrentBalance = PaymentAmountFormatter.ToCurrencyString(agencyBalance, Currencies.USD),
                            ResultingBalance = PaymentAmountFormatter.ToCurrencyString(resultingBalance, Currencies.USD),
                            ShowAlert = resultingBalance < 0m,
                            ReportDate = EmailContentFormatter.FromDate(reportEndDate)
                        },
                        emailAndSetting.Email);
                }).Where(t => t.Item1.Bookings.Any()).ToList();


                static BookingSummaryNotificationData.BookingData CreateBookingData(Booking booking) =>
                    new BookingSummaryNotificationData.BookingData
                    {
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = GetLeadingPassengerFormattedName(booking),
                        Amount = PaymentAmountFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = EmailContentFormatter.FromDate(booking.DeadlineDate),
                        CheckInDate = EmailContentFormatter.FromDate(booking.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(booking.CheckOutDate),
                        Status = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(booking.Status)
                    };
            }


            async Task<Result<string>> SendMails(List<(BookingSummaryNotificationData Data, string Email)> dataAndEmailTuples)
            {
                var builder = new StringBuilder();
                var hasErrors = false;

                foreach (var (data, email) in dataAndEmailTuples)
                {
                    var (_, isFailure, error) = await SendEmail(email, _options.BookingSummaryTemplateId, data);
                    if (isFailure)
                        hasErrors = true;

                    var message = isFailure
                        ? $"Failed to send a booking summary report for agency with id {agencyId} to '{email}'. Error: {error}"
                        : $"Successfully sent a booking summary report for agency with id {agencyId} to '{email}'";

                    builder.AppendLine(message);
                }

                return hasErrors
                    ? Result.Failure<string>(builder.ToString())
                    : Result.Success(builder.ToString());
            }
        }


        public Task<Result> SendBookingsAdministratorSummary()
        {
            return GetNotificationData()
                .Bind(Send);
            
            
            async Task<Result<BookingAdministratorSummaryNotificationData>> GetNotificationData()
            {
                var startDate = _dateTimeProvider.UtcToday();
                var endDate = startDate.AddDays(DayBeforeAdministratorsNotification);
                
                var bookingRowsQuery = from booking in _context.Bookings
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join agentAgencyRelation in _context.AgentAgencyRelations on agent.Id equals agentAgencyRelation.AgentId
                    join agency in _context.Agencies on agentAgencyRelation.AgencyId equals agency.Id
                    where ((booking.CheckInDate <= endDate && booking.CheckInDate >= startDate) ||
                            booking.DeadlineDate.HasValue && booking.DeadlineDate >= startDate && booking.DeadlineDate <= endDate)
                    orderby booking.DeadlineDate ?? booking.CheckInDate    
                    select new BookingAdministratorSummaryNotificationData.BookingRowData()
                    {
                        Agency = agency.Name,
                        Agent = $"{agent.FirstName} {agent.LastName}",
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = GetLeadingPassengerFormattedName(booking),
                        Amount = PaymentAmountFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = EmailContentFormatter.FromDate(booking.DeadlineDate),
                        CheckInDate = EmailContentFormatter.FromDate(booking.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(booking.CheckOutDate),
                        Status = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(booking.Status),
                        PaymentStatus = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(booking.PaymentStatus)
                    };
            
                return new BookingAdministratorSummaryNotificationData
                {
                    ReportDate = EmailContentFormatter.FromDate(_dateTimeProvider.UtcToday()),
                    Bookings = await bookingRowsQuery.ToListAsync()
                };
            }
            
            
            Task<Result> Send(BookingAdministratorSummaryNotificationData notificationData) => _mailSender.Send(_options.BookingAdministratorSummaryTemplateId, _options.CcNotificationAddresses, notificationData);
        }


        public async Task<Result> SendBookingsPaymentsSummaryToAdministrator()
        {
            if (_dateTimeProvider.UtcToday().Day != MonthlyReportScheduleDay)
                return Result.Success();
            
            return await GetNotificationData()
                .Bind(Send);

            
            async Task<Result<BookingAdministratorSummaryNotificationData>> GetNotificationData()
            {
                var startDate = _dateTimeProvider.UtcToday().AddMonths(-1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                
                var bookingRowsQuery = from booking in _context.Bookings
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join agentAgencyRelation in _context.AgentAgencyRelations on agent.Id equals agentAgencyRelation.AgentId
                    join agency in _context.Agencies on agentAgencyRelation.AgencyId equals agency.Id
                    where ((booking.CheckInDate <= endDate && booking.CheckInDate >= startDate) ||
                            booking.DeadlineDate.HasValue && booking.DeadlineDate >= startDate && booking.DeadlineDate <= endDate)
                    where booking.PaymentMethod == PaymentMethods.BankTransfer
                    orderby booking.DeadlineDate ?? booking.CheckInDate    
                    select new BookingAdministratorSummaryNotificationData.BookingRowData
                    {
                        Agency = agency.Name,
                        Agent = $"{agent.FirstName} {agent.LastName}",
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = GetLeadingPassengerFormattedName(booking),
                        Amount = PaymentAmountFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = EmailContentFormatter.FromDate(booking.DeadlineDate),
                        CheckInDate = EmailContentFormatter.FromDate(booking.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(booking.CheckOutDate),
                        Status = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(booking.Status),
                        PaymentStatus = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(booking.PaymentStatus)
                    };
            
                return new BookingAdministratorSummaryNotificationData
                {
                    ReportDate = EmailContentFormatter.FromDate(_dateTimeProvider.UtcToday()),
                    Bookings = await bookingRowsQuery.ToListAsync()
                };
            }
            
            
            Task<Result> Send(BookingAdministratorSummaryNotificationData notificationData)
            {
                return _mailSender.Send(_options.BookingAdministratorPaymentsSummaryTemplateId, 
                    _options.CcNotificationAddresses, notificationData);
            }
        }


        private Task<Result> SendEmail(string email, string templateId, DataWithCompanyInfo data)
        {
            return Validate()
                .Bind(Send);


            Result Validate()
                => GenericValidator<string>
                    .Validate(setup => setup.RuleFor(e => e).EmailAddress(), email);


            Task<Result> Send() => _mailSender.Send(templateId, email, data);
        }


        private Task SendDetailedBookingNotification(AccommodationBookingInfo bookingInfo, string mailTemplate)
        {
            var details = bookingInfo.BookingDetails;
            var notificationData = CreateNotificationData(bookingInfo, details);
            return _mailSender.Send(mailTemplate, _options.CcNotificationAddresses, notificationData);


            static BookingNotificationData CreateNotificationData(AccommodationBookingInfo bookingInfo, AccommodationBookingDetails details)
            {
                return new BookingNotificationData
                {
                    AgentName = bookingInfo.AgentInformation.AgentName,
                    BookingDetails = new BookingNotificationData.Details
                    {
                        AccommodationName = details.AccommodationName,
                        CheckInDate = EmailContentFormatter.FromDate(details.CheckInDate),
                        CheckOutDate = EmailContentFormatter.FromDate(details.CheckOutDate),
                        DeadlineDate = EmailContentFormatter.FromDate(details.DeadlineDate),
                        Location = details.Location,
                        NumberOfNights = details.NumberOfNights,
                        NumberOfPassengers = details.NumberOfPassengers,
                        ReferenceCode = details.ReferenceCode,
                        RoomDetails = details.RoomDetails.Select(d =>
                        {
                            var maskedPassengers = d.Passengers.Where(p => p.IsLeader)
                                .Select(p =>
                                {
                                    var firstName = p.FirstName.Length == 1 ? "*" : p.FirstName.Substring(0, 1);
                                    return new Pax(p.Title, p.LastName, firstName);
                                })
                                .ToList();

                            return new BookingNotificationData.BookedRoomDetails
                            {
                                ContractDescription = d.ContractDescription,
                                MealPlan = d.MealPlan,
                                Passengers = maskedPassengers,
                                Price = PaymentAmountFormatter.ToCurrencyString(d.Price.Amount, d.Price.Currency),
                                Type = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(d.Type),
                                Remarks = d.Remarks
                            };
                        }).ToList(),
                        Status = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(details.Status),
                        SupplierReferenceCode = details.AgentReference,
                        ContactInfo = details.ContactInfo,
                    },
                    CounterpartyName = bookingInfo.AgentInformation.CounterpartyName,
                    AgencyName = bookingInfo.AgentInformation.AgencyName,
                    PaymentStatus = MailSender.Formatters.EmailContentFormatter.FromEnumDescription(bookingInfo.PaymentStatus),
                    Price = PaymentAmountFormatter.ToCurrencyString(bookingInfo.TotalPrice.Amount, bookingInfo.TotalPrice.Currency),
                    DataProvider = bookingInfo.DataProvider is null
                        ? string.Empty
                        : MailSender.Formatters.EmailContentFormatter.FromEnumDescription(bookingInfo.DataProvider.Value),
                };
            }
        }

        
        private static string GetLeadingPassengerFormattedName(Booking booking)
        {
            var leadingPassengersList = booking.Rooms
                .SelectMany(r =>
                {
                    if (r.Passengers == null)
                        return new List<Pax>(0);
                    
                    return r.Passengers.Where(p => p.IsLeader);
                })
                .ToList();
            
            if (leadingPassengersList.Any())
            {
                var leadingPassenger = leadingPassengersList.First();
                return MailSender.Formatters.EmailContentFormatter.FromPassengerName(leadingPassenger.FirstName, leadingPassenger.LastName,
                    MailSender.Formatters.EmailContentFormatter.FromEnumDescription(leadingPassenger.Title));
            }

            return MailSender.Formatters.EmailContentFormatter.FromPassengerName("*", string.Empty);
        }

        
        private static string FormatPrice(MoneyAmount moneyAmount) => PaymentAmountFormatter.ToCurrencyString(moneyAmount.Amount, moneyAmount.Currency);

        private static readonly HashSet<BookingStatuses> BookingStatusesForSummary = new HashSet<BookingStatuses>
        {
            BookingStatuses.Confirmed,
            BookingStatuses.InternalProcessing,
            BookingStatuses.Pending,
            BookingStatuses.WaitingForResponse
        };
        
        private const int DayBeforeAdministratorsNotification = 5;
        private const int MonthlyReportScheduleDay = 1;

        private readonly IBookingDocumentsService _bookingDocumentsService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly BookingMailingOptions _options;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly EdoContext _context;


        private class EmailAndSetting
        {
            public string Email { get; set; }
            public int ReportDaysSetting { get; set; }
        }
    }
}