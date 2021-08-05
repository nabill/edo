using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.DataFormatters;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public class BookingReportsService : IBookingReportsService
    {
        public BookingReportsService(IDateTimeProvider dateTimeProvider,
            INotificationService notificationService,
            IAgentSettingsManager agentSettingsManager,
            IAccountPaymentService accountPaymentService,
            IOptions<BookingMailingOptions> options,
            EdoContext context)
        {
            _dateTimeProvider = dateTimeProvider;
            _notificationService = notificationService;
            _agentSettingsManager = agentSettingsManager;
            _accountPaymentService = accountPaymentService;
            _context = context;
            _options = options.Value;
        }


        public async Task<Result<string>> SendBookingReports(int agencyId)
        {
            var reportBeginTime = _dateTimeProvider.UtcNow();
            var reportEndTime = reportBeginTime.AddDays(1);

            return await GetEmailsAndSettings()
                .Map(GetBookings)
                .Bind(CreateMailData)
                .Bind(SendMails);


            async Task<Result<List<EmailAndSetting>>> GetEmailsAndSettings()
            {
                var rolesWithPermission = await _context.AgentRoles
                    .Where(x => x.Permissions.HasFlag(InAgencyPermissions.ReceiveBookingSummary))
                    .Select(x => x.Id)
                    .ToListAsync();
                
                var emailsAndSettings = await
                    (from relation in _context.AgentAgencyRelations
                        join agent in _context.Agents
                            on relation.AgentId equals agent.Id
                        where relation.AgencyId == agencyId
                            && relation.IsActive
                            && relation.AgentRoleIds.Any(rolesWithPermission.Contains)
                        select new EmailAndSetting
                        {
                            AgentId = relation.AgentId,
                            AgencyId = relation.AgencyId,
                            Email = agent.Email,
                            ReportDaysSetting = _agentSettingsManager.GetUserSettings(agent).BookingReportDays
                        }).ToListAsync();

                return emailsAndSettings.Any()
                    ? Result.Success(emailsAndSettings)
                    : Result.Failure<List<EmailAndSetting>>($"Couldn't find any agents in agency with id {agencyId} to send summary to");
            }


            async Task<(List<EmailAndSetting>, List<Booking>)> GetBookings(List<EmailAndSetting> emailsAndSettings)
            {
                var bookings = await _context.Bookings
                    .Where(b => b.AgencyId == agencyId
                        && b.PaymentType == PaymentTypes.VirtualAccount
                        && b.PaymentStatus != BookingPaymentStatuses.Captured
                        && BookingStatusesForSummary.Contains(b.Status)
                        && ((b.DeadlineDate != null) ? b.DeadlineDate : b.CheckInDate) > reportBeginTime 
                        && ((b.DeadlineDate != null) ? b.DeadlineDate : b.CheckInDate) <= reportEndTime)
                    .ToListAsync();

                return (emailsAndSettings, bookings);
            }


            async Task<Result<List<(BookingSummaryNotificationData, EmailAndSetting)>>> CreateMailData(
                (List<EmailAndSetting> emailsAndSettings, List<Booking> bookings) values)
            {
                var (_, isFailure, balanceInfo, error) = await _accountPaymentService.GetAccountBalance(Currencies.USD, agencyId);
                if (isFailure)
                    return Result.Failure<List<(BookingSummaryNotificationData, EmailAndSetting)>>(
                        $"Couldn't retrieve account balance for agency with id {agencyId}. Error: {error}");

                var agencyBalance = balanceInfo.Balance;

                return values.emailsAndSettings.Select(emailAndSetting =>
                {
                    var resultingBalance = agencyBalance - values.bookings.Sum(b => b.TotalPrice);

                    return (new BookingSummaryNotificationData
                        {
                            Bookings = values.bookings.OrderBy(b => b.DeadlineDate).Select(CreateBookingData).ToList(),
                            CurrentBalance = MoneyFormatter.ToCurrencyString(agencyBalance, Currencies.USD),
                            ResultingBalance = MoneyFormatter.ToCurrencyString(resultingBalance, Currencies.USD),
                            ShowAlert = resultingBalance < 0m,
                            ReportDate = DateTimeFormatters.ToDateString(reportBeginTime)
                        },
                        emailAndSetting);
                }).Where(t => t.Item1.Bookings.Any()).ToList();


                static BookingSummaryNotificationData.BookingData CreateBookingData(Booking booking)
                    => new BookingSummaryNotificationData.BookingData
                    {
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = booking.GetLeadingPassengerFormattedName(),
                        Amount = MoneyFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = DateTimeFormatters.ToDateString(booking.DeadlineDate),
                        CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                        Status = EnumFormatters.FromDescription(booking.Status)
                    };
            }


            async Task<Result<string>> SendMails(List<(BookingSummaryNotificationData Data, EmailAndSetting emailAndSetting)> dataAndEmailTuples)
            {
                var builder = new StringBuilder();
                var hasErrors = false;

                foreach (var (data, emailAndSetting) in dataAndEmailTuples)
                {
                    var (_, isFailure, error) = await _notificationService.Send(agent: new SlimAgentContext(emailAndSetting.AgentId, emailAndSetting.AgencyId), 
                        messageData: data,
                        notificationType: NotificationTypes.BookingSummaryReportForAgent,
                        email: emailAndSetting.Email,
                        templateId: _options.BookingSummaryTemplateId);

                    if (isFailure)
                    {
                        hasErrors = true;
                        builder.AppendLine($"Failed to send a booking summary report for agency with id {agencyId} to '{emailAndSetting.Email}'. Error: {error}");
                    }
                }

                return hasErrors
                    ? Result.Failure<string>(builder.ToString())
                    : Result.Success(string.Empty);
            }
        }


        public Task<Result> SendBookingsAdministratorSummary()
        {
            return GetNotificationData()
                .Bind(Send);


            async Task<Result<BookingAdministratorSummaryNotificationData>> GetNotificationData()
            {
                var startTime = _dateTimeProvider.UtcNow();
                var endTime = startTime.AddDays(DayBeforeAdministratorsNotification);

                var bookingRowsQuery = from booking in _context.Bookings
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join agentAgencyRelation in _context.AgentAgencyRelations on agent.Id equals agentAgencyRelation.AgentId
                    join agency in _context.Agencies on agentAgencyRelation.AgencyId equals agency.Id
                    where (booking.DeadlineDate.HasValue ? booking.DeadlineDate : booking.CheckInDate) > startTime
                        && (booking.DeadlineDate.HasValue ? booking.DeadlineDate : booking.CheckInDate) <= endTime
                        && BookingStatusesForSummary.Contains(booking.Status)
                    orderby booking.DeadlineDate ?? booking.CheckInDate
                    select new BookingAdministratorSummaryNotificationData.BookingRowData()
                    {
                        Agency = agency.Name,
                        Agent = $"{agent.FirstName} {agent.LastName}",
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = booking.GetLeadingPassengerFormattedName(),
                        Amount = MoneyFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = DateTimeFormatters.ToDateString(booking.DeadlineDate),
                        CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                        Status = EnumFormatters.FromDescription(booking.Status),
                        PaymentStatus = EnumFormatters.FromDescription(booking.PaymentStatus)
                    };

                return new BookingAdministratorSummaryNotificationData
                {
                    ReportDate = DateTimeFormatters.ToDateString(_dateTimeProvider.UtcToday()),
                    Bookings = await bookingRowsQuery.ToListAsync()
                };
            }


            Task<Result> Send(BookingAdministratorSummaryNotificationData notificationData)
                => _notificationService.Send(messageData: notificationData,
                    notificationType: NotificationTypes.BookingsAdministratorSummaryNotification,
                    emails: _options.CcNotificationAddresses,
                    templateId: _options.BookingAdministratorSummaryTemplateId);
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
                    where booking.PaymentType == PaymentTypes.VirtualAccount
                    orderby booking.DeadlineDate ?? booking.CheckInDate
                    select new BookingAdministratorSummaryNotificationData.BookingRowData
                    {
                        Agency = agency.Name,
                        Agent = $"{agent.FirstName} {agent.LastName}",
                        ReferenceCode = booking.ReferenceCode,
                        Accommodation = booking.AccommodationName,
                        Location = $"{booking.Location.Country}, {booking.Location.Locality}",
                        LeadingPassenger = booking.GetLeadingPassengerFormattedName(),
                        Amount = MoneyFormatter.ToCurrencyString(booking.TotalPrice, booking.Currency),
                        DeadlineDate = DateTimeFormatters.ToDateString(booking.DeadlineDate),
                        CheckInDate = DateTimeFormatters.ToDateString(booking.CheckInDate),
                        CheckOutDate = DateTimeFormatters.ToDateString(booking.CheckOutDate),
                        Status = EnumFormatters.FromDescription(booking.Status),
                        PaymentStatus = EnumFormatters.FromDescription(booking.PaymentStatus)
                    };

                return new BookingAdministratorSummaryNotificationData
                {
                    ReportDate = DateTimeFormatters.ToDateString(_dateTimeProvider.UtcToday()),
                    Bookings = await bookingRowsQuery.ToListAsync()
                };
            }


            Task<Result> Send(BookingAdministratorSummaryNotificationData notificationData)
                => _notificationService.Send(messageData: notificationData,
                    notificationType: NotificationTypes.BookingAdministratorPaymentsSummary,
                    emails: _options.CcNotificationAddresses,
                    templateId: _options.BookingAdministratorPaymentsSummaryTemplateId);
        }


        private static readonly HashSet<BookingStatuses> BookingStatusesForSummary = new()
        {
            BookingStatuses.WaitingForResponse,
            BookingStatuses.Pending,
            BookingStatuses.Confirmed,
            BookingStatuses.ManualCorrectionNeeded,
            BookingStatuses.PendingCancellation
        };

        private const int DayBeforeAdministratorsNotification = 5;
        private const int MonthlyReportScheduleDay = 1;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INotificationService _notificationService;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly EdoContext _context;
        private readonly BookingMailingOptions _options;


        private class EmailAndSetting
        {
            public int AgentId { get; set; }
            public int AgencyId { get; set; }
            public string Email { get; set; }
            public int ReportDaysSetting { get; set; }
        }
    }
}