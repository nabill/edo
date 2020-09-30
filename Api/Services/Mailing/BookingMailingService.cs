using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.MailSender.Formatters;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingMailingService : IBookingMailingService
    {
        public BookingMailingService(MailSenderWithCompanyInfo mailSender,
            IBookingDocumentsService bookingDocumentsService,
            IBookingRecordsManager bookingRecordsManager,
            IOptions<BookingMailingOptions> options,
            IJsonSerializer serializer,
            IDateTimeProvider dateTimeProvider,
            EdoContext context)
        {
            _bookingDocumentsService = bookingDocumentsService;
            _bookingRecordsManager = bookingRecordsManager;
            _mailSender = mailSender;
            _options = options.Value;
            _serializer = serializer;
            _dateTimeProvider = dateTimeProvider;
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
                        DeadlineDate = FormatDate(voucher.DeadlineDate),
                        NightCount = voucher.NightCount,
                        ReferenceCode = voucher.ReferenceCode,
                        RoomDetails = voucher.RoomDetails,
                        CheckInDate = FormatDate(voucher.CheckInDate),
                        CheckOutDate = FormatDate(voucher.CheckOutDate),
                        MainPassengerName = voucher.MainPassengerName
                    };

                    return SendEmail(email, _options.VoucherTemplateId, voucherData);
                });
        }


        public Task<Result> SendInvoice(int bookingId, string email, AgentContext agent, string languageCode)
        {
            return _bookingDocumentsService.GetActualInvoice(bookingId, agent)
                .Bind(invoice =>
                {
                    var (registrationInfo, data) = invoice;
                    var invoiceData = new InvoiceData
                    {
                        Number = registrationInfo.Number,
                        BuyerDetails = data.BuyerDetails,
                        InvoiceDate = FormatDate(registrationInfo.Date),
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
                        CurrencyCode = data.TotalPrice.Currency.ToString(),
                        ReferenceCode = data.ReferenceCode,
                        SellerDetails = data.SellerDetails,
                        PayDueDate = FormatDate(data.PayDueDate)
                    };

                    return SendEmail(email, _options.InvoiceTemplateId, invoiceData);
                });
        }


        public Task<Result> NotifyBookingCancelled(string referenceCode, string email, string agentName)
        {
            // TODO: hardcoded to be removed with UEDA-20
            var addresses = new List<string> {email};
            addresses.AddRange(_options.CcNotificationAddresses);

            return _mailSender.Send(_options.BookingCancelledTemplateId, addresses, new BookingCancelledData
            {
                AgentName = agentName,
                ReferenceCode = referenceCode
            });
        }


        // TODO: hardcoded to be removed with UEDA-20
        public Task NotifyBookingFinalized(in AccommodationBookingInfo bookingInfo, in AgentContext agentContext)
        {
            var details = bookingInfo.BookingDetails;

            return _mailSender.Send(_options.ReservationsBookingFinalizedTemplateId, _options.CcNotificationAddresses, new BookingFinalizedData
            {
                AgentName = agentContext.AgentName,
                BookingDetails = new BookingFinalizedData.Details
                {
                    AccommodationName = details.AccommodationName,
                    CheckInDate = details.CheckInDate, 
                    CheckOutDate = details.CheckOutDate, 
                    DeadlineDate = details.DeadlineDate,
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

                        return new BookingFinalizedData.BookedRoomDetails
                        {
                            ContractDescription = d.ContractDescription,
                            MealPlan = d.MealPlan,
                            Passengers = maskedPassengers,
                            Price = PaymentAmountFormatter.ToCurrencyString(d.Price.Amount, d.Price.Currency),
                            Type = d.Type.ToString()
                        };
                    }).ToList(),
                    Status = details.Status.ToString(),
                    SupplierReferenceCode = details.AgentReference
                },
                CounterpartyName = agentContext.CounterpartyName,
                PaymentStatus = bookingInfo.PaymentStatus.ToString(),
                Price = PaymentAmountFormatter.ToCurrencyString(bookingInfo.TotalPrice.Amount, bookingInfo.TotalPrice.Currency)
            });
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
                        CheckInDate = FormatDate(booking.CheckInDate),
                        CheckOutDate = FormatDate(booking.CheckOutDate),
                        Deadline = FormatDate(booking.DeadlineDate)
                    };
                    
                    return SendEmail(email, _options.DeadlineNotificationTemplateId, deadlineData);
                });
        }


        public async Task SendBookingReports(int agencyId)
        {
            DateTime reportBeginDate = _dateTimeProvider.UtcNow().Date;

            await GetEmailsAndSettings()
                .Map(GetBookings)
                .Map(CreateMailData)
                .Tap(SendMails);


            async Task<Result<List<EmailAndSetting>>> GetEmailsAndSettings()
            {
                var emailSettingsObjects = await
                    (from relation in _context.AgentAgencyRelations
                        join agent in _context.Agents
                            on relation.AgentId equals agent.Id
                        where relation.AgencyId == agencyId
                            && relation.InAgencyPermissions.HasFlag(InAgencyPermissions.ReceiveBookingSummary)
                        select new {agent.Email, agent.UserSettings}).ToListAsync();

                var emailsAndSettings = emailSettingsObjects.Select(a => new EmailAndSetting
                {
                    Email = a.Email,
                    ReportDaysSetting = GetReportDaysFromSettings(a.UserSettings)
                }).ToList();

                return Result.Success(emailsAndSettings);


                int GetReportDaysFromSettings(string userSettings)
                {
                    var daysCount = _serializer.DeserializeObject<AgentUserSettings>(userSettings).BookingReportDays;
                    return daysCount == default
                        ? ReportDaysDefault
                        : daysCount;
                }
            }


            async Task<(List<EmailAndSetting>, List<Booking>)> GetBookings(List<EmailAndSetting> emailsAndSettings)
            {
                var maxPeriod = emailsAndSettings.Max(t => t.ReportDaysSetting);
                var reportMaxEndDate = reportBeginDate.AddDays(maxPeriod);

                var bookings = await _context.Bookings.Where(b => b.AgencyId == agencyId
                    && b.PaymentMethod == PaymentMethods.BankTransfer
                    && b.PaymentStatus != BookingPaymentStatuses.Captured
                    && b.DeadlineDate < reportMaxEndDate).ToListAsync();

                return (emailsAndSettings, bookings);
            }


            async Task<List<(BookingSummaryNotificationData, string)>> CreateMailData((List<EmailAndSetting> emailsAndSettings, List<Booking> bookings) values)
            {
                var agencyBalance = (await _context.AgencyAccounts.Where(a => a.IsActive && a.AgencyId == agencyId && a.Currency == Currencies.USD)
                    .SingleOrDefaultAsync()).Balance;

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
                            ReportDate = FormatDate(reportEndDate)
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
                        DeadlineDate = FormatDate(booking.DeadlineDate),
                        CheckInDate = FormatDate(booking.CheckInDate),
                        CheckOutDate = FormatDate(booking.CheckOutDate),
                        Status = booking.Status.ToString()
                    };


                static string GetLeadingPassengerFormattedName(Booking booking)
                {
                    var leadingPassengersList = booking.Rooms.SelectMany(r => r.Passengers.Where(p => p.IsLeader)).ToList();
                    if (leadingPassengersList.Any())
                    {
                        var leadingPassenger = leadingPassengersList.First();
                        return EmailContentFormatter.FromPassengerName(leadingPassenger.FirstName, leadingPassenger.LastName,
                            leadingPassenger.Title.ToString());
                    }

                    return EmailContentFormatter.FromPassengerName("*", string.Empty);
                }
            }


            async Task SendMails(List<(BookingSummaryNotificationData Data, string Email)> dataAndEmailTuples) =>
                await Task.WhenAll(dataAndEmailTuples.Select(d => SendEmail(d.Email, "TEMPLATE", d.Data)));                  //TODO use template id
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


        private static string FormatDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("dd-MMM-yy")
                : string.Empty;
        }


        private static string FormatPrice(MoneyAmount moneyAmount) => PaymentAmountFormatter.ToCurrencyString(moneyAmount.Amount, moneyAmount.Currency);


        private const int ReportDaysDefault = 3;

        private readonly IBookingDocumentsService _bookingDocumentsService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly BookingMailingOptions _options;
        private readonly IJsonSerializer _serializer;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;


        private class EmailAndSetting
        {
            public string Email { get; set; }
            public int ReportDaysSetting { get; set; }
        }
    }
}