using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Money.Models;
using HappyTravel.Edo.Api.Models.Management;
using FluentValidation;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencyVerificationService : IAgencyVerificationService
    {
        public AgencyVerificationService(EdoContext context, IAccountManagementService accountManagementService,
            IManagementAuditService managementAuditService, INotificationService notificationService,
            IDateTimeProvider dateTimeProvider, Services.Agents.IAgentService agentService)
        {
            _context = context;
            _accountManagementService = accountManagementService;
            _managementAuditService = managementAuditService;
            _notificationService = notificationService;
            _dateTimeProvider = dateTimeProvider;
            _agentService = agentService;
        }


        public async Task<Result> VerifyAsFullyAccessed(int agencyId, AgencyFullAccessVerificationRequest request)
        {
            return await ValidateVerify(request)
                .Bind(() => GetAgency(agencyId))
                .Ensure(a => a.ParentId is null, "Verification is only available for root agencies")
                .Ensure(a => a.VerificationState == AgencyVerificationStates.ReadOnly,
                    "Verification as fully accessed is only available for agencies that were verified as read-only earlier")
                .Tap(c => SetVerificationState(c, AgencyVerificationStates.FullAccess, request.Reason))
                .Tap(SetContractKind)
                .Tap(SetAprModeAndPassedDeadlineOffersMode)
                .Tap(() => WriteVerificationToAuditLog(agencyId, request.Reason, AgencyVerificationStates.FullAccess));


            Task<Result> ValidateVerify(AgencyFullAccessVerificationRequest request)
                => GenericValidator<AgencyFullAccessVerificationRequest>.ValidateAsync(v =>
                    {
                        v.RuleFor(r => r.CreditLimit)
                            .NotEmpty()
                            .When(r => r.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments);

                        v.RuleFor(r => r.CreditLimit)
                            .Empty()
                            .When(r => r.ContractKind != ContractKind.VirtualAccountOrCreditCardPayments);

                        v.RuleFor(r => r.ContractKind)
                            .NotEmpty();
                    }, request);


            async Task SetContractKind(Agency agency)
            {
                agency.ContractKind = request.ContractKind;
                agency.CreditLimit = (request.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments)
                    ? request.CreditLimit
                    : null;
                await _context.SaveChangesAsync();
            }


            async Task SetAprModeAndPassedDeadlineOffersMode(Agency agency)
            {
                if (request.ContractKind != ContractKind.VirtualAccountOrCreditCardPayments)
                    return;

                var settings = await _context.AgencySystemSettings
                    .SingleOrDefaultAsync(a => a.AgencyId == agencyId);

                if (Equals(settings, default))
                {
                    _context.Add(new AgencySystemSettings
                    {
                        AccommodationBookingSettings = new AgencyAccommodationBookingSettings
                        {
                            AprMode = AprMode.CardAndAccountPurchases,
                            PassedDeadlineOffersMode = PassedDeadlineOffersMode.CardAndAccountPurchases
                        },
                        AgencyId = agencyId
                    });
                }
                else
                {
                    settings.AccommodationBookingSettings ??= new AgencyAccommodationBookingSettings();
                    settings.AccommodationBookingSettings.AprMode = AprMode.CardAndAccountPurchases;
                    settings.AccommodationBookingSettings.PassedDeadlineOffersMode = PassedDeadlineOffersMode.CardAndAccountPurchases;

                    _context.Update(settings);
                }

                await _context.SaveChangesAsync();
            }
        }


        public Task<Result> VerifyAsReadOnly(int agencyId, string verificationReason)
        {
            return GetAgency(agencyId)
                .Ensure(a => a.ParentId is null, "Verification is only available for root agencies")
                .Ensure(a => a.VerificationState == AgencyVerificationStates.PendingVerification,
                    "Verification as read-only is only available for agencies that are in pending verification step")
                .BindWithTransaction(_context, agency => SetReadOnlyVerificationState(agency)
                    .Bind(CreateAccountsForAgencies))
                .Tap(() => WriteVerificationToAuditLog(agencyId, verificationReason, AgencyVerificationStates.ReadOnly));


            async Task<Result<Agency>> SetReadOnlyVerificationState(Agency agency)
            {
                await SetVerificationState(agency, AgencyVerificationStates.ReadOnly, verificationReason);
                return agency;
            }


            async Task<Result> CreateAccountsForAgencies(Agency agency)
            {
                var (_, isFailure) = await _accountManagementService.CreateForAgency(agency, agency.PreferredCurrency);
                if (isFailure)
                    return Result.Failure("Error while creating an account for agency");

                var childAgencies = await _context.Agencies.Where(a => a.Ancestors.Contains(agencyId)).ToListAsync();

                foreach (var childAgency in childAgencies)
                {
                    var (_, isChildAgencyFailure) = await _accountManagementService.CreateForAgency(childAgency, childAgency.PreferredCurrency);
                    if (isChildAgencyFailure)
                        return Result.Failure("Error while creating accounts for child agencies");
                }

                return Result.Success();
            }
        }


        public async Task<Result> DeclineVerification(int agencyId, string verificationReason)
        {
            return await GetAgency(agencyId)
                .Ensure(a => a.ParentId is null, "Verification is only available for root agencies")
                .Ensure(a => a.VerificationState == AgencyVerificationStates.PendingVerification,
                    "Verification failure is only available for agencies that are in a pending state")
                .Tap(a => SetVerificationState(a, AgencyVerificationStates.DeclinedVerification, verificationReason))
                .Tap(() => WriteVerificationToAuditLog(agencyId, verificationReason, AgencyVerificationStates.DeclinedVerification));
        }


        private async Task SetVerificationState(Agency agency, AgencyVerificationStates state, string verificationReason)
        {
            var now = _dateTimeProvider.UtcNow();
            string reason;
            if (string.IsNullOrEmpty(agency.VerificationReason))
                reason = verificationReason;
            else
                reason = agency.VerificationReason + Environment.NewLine + verificationReason;

            agency.VerificationState = state;
            agency.VerificationReason = reason;
            agency.Verified = now;
            agency.Modified = now;
            _context.Update(agency);
            await _context.SaveChangesAsync();

            await SendNotificationToMaster();


            async Task<Result> SendNotificationToMaster()
            {
                var (_, isFailure, master, error) = await _agentService.GetMasterAgent(agency.Id);
                if (isFailure)
                    return Result.Failure(error);

                var messageData = new AgencyVerificationStateChangedData
                {
                    AgentName = $"{master.FirstName} {master.LastName}",
                    AgencyName = agency.Name,
                    State = EnumFormatters.FromDescription(state),
                };

                return await _notificationService.Send(agent: new SlimAgentContext(master.Id, agency.Id),
                    messageData: messageData,
                    notificationType: NotificationTypes.AgencyVerificationChanged,
                    email: master.Email);
            }
        }


        private Task WriteVerificationToAuditLog(int agencyId, string verificationReason, AgencyVerificationStates verificationState)
            => _managementAuditService.Write(ManagementEventType.AgencyVerification,
                new AgencyVerifiedEventData(agencyId, verificationReason, verificationState));


        // This method is the same with AdminAgencyManagementService.GetAgency,
        // because administrator services in the future will be replaced to another application
        private async Task<Result<Agency>> GetAgency(int agencyId)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(c => c.Id == agencyId);

            if (agency == null)
                return Result.Failure<Agency>("Could not find agency with specified id");

            return Result.Success(agency);
        }


        private readonly EdoContext _context;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IManagementAuditService _managementAuditService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly Services.Agents.IAgentService _agentService;
    }
}