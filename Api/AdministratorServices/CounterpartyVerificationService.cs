using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyVerificationService : ICounterpartyVerificationService
    {
        public CounterpartyVerificationService(IAccountManagementService accountManagementService,
            IDateTimeProvider dateTimeProvider,
            EdoContext context,
            IManagementAuditService managementAuditService)
        {
            _accountManagementService = accountManagementService;
            _dateTimeProvider = dateTimeProvider;
            _context = context;
            _managementAuditService = managementAuditService;
        }


        public async Task<Result> VerifyAsFullyAccessed(int counterpartyId, CounterpartyContractKind contractKind, string verificationReason)
        {
            return await GetCounterparty(counterpartyId)
                .Ensure(c => c.State == CounterpartyStates.ReadOnly,
                    "Verification as fully accessed is only available for counterparties that were verified as read-only earlier")
                .Ensure(IsContractTypeValid, "Invalid contract type")
                .Tap(c => SetVerificationState(c, CounterpartyStates.FullAccess, verificationReason))
                .Tap(SetContractType)
                .Tap(() => WriteVerificationToAuditLog(counterpartyId, verificationReason));
            
            
            bool IsContractTypeValid(Counterparty _) 
                => !contractKind.Equals(default);
            
            
            async Task SetContractType(Counterparty counterparty)
            {
                counterparty.ContractKind = contractKind;
                await _context.SaveChangesAsync();
            }
        }


        public Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason)
        {
            return GetCounterparty(counterpartyId)
                .Ensure(c => c.State == CounterpartyStates.PendingVerification,
                    "Verification as read-only is only available for counterparties that are in pending verification step")
                .BindWithTransaction(_context, c => SetReadOnlyVerificationState(c)
                    .Bind(CreateAccountForCounterparty)
                    .Bind(() => CreateAccountsForAgencies(c)))
                .Tap(() => WriteVerificationToAuditLog(counterpartyId, verificationReason));


            async Task<Result<Counterparty>> SetReadOnlyVerificationState(Counterparty counterparty)
            {
                await SetVerificationState(counterparty, CounterpartyStates.ReadOnly, verificationReason);
                return counterparty;
            }


            Task<Result> CreateAccountForCounterparty(Counterparty counterparty)
                => _accountManagementService
                    .CreateForCounterparty(counterparty, counterparty.PreferredCurrency);


            async Task<Result> CreateAccountsForAgencies(Counterparty counterparty)
            {
                var agencies = await _context.Agencies.Where(a => a.CounterpartyId == counterpartyId).ToListAsync();

                foreach (var agency in agencies)
                {
                    var (_, isFailure) = await _accountManagementService.CreateForAgency(agency, counterparty.PreferredCurrency);
                    if (isFailure)
                        return Result.Failure("Error while creating accounts for agencies");
                }

                return Result.Success();
            }
        }


        public async Task<Result> DeclineVerification(int counterpartyId, string verificationReason)
        {
            return await GetCounterparty(counterpartyId)
                .Ensure(c => c.State == CounterpartyStates.PendingVerification,
                    "Verification failure is only available for counterparties that are in a pending state")
                .Tap(c => SetVerificationState(c, CounterpartyStates.DeclinedVerification, verificationReason))
                .Tap(() => WriteVerificationToAuditLog(counterpartyId, verificationReason));
        }


        private Task SetVerificationState(Counterparty counterparty, CounterpartyStates state, string verificationReason)
        {
            var now = _dateTimeProvider.UtcNow();
            string reason;
            if (string.IsNullOrEmpty(counterparty.VerificationReason))
                reason = verificationReason;
            else
                reason = counterparty.VerificationReason + Environment.NewLine + verificationReason;

            counterparty.State = state;
            counterparty.VerificationReason = reason;
            counterparty.Verified = now;
            counterparty.Updated = now;
            _context.Update(counterparty);

            return _context.SaveChangesAsync();
        }


        private Task WriteVerificationToAuditLog(int counterpartyId, string verificationReason)
            => _managementAuditService.Write(ManagementEventType.CounterpartyVerification,
                new CounterpartyVerifiedAuditEventData(counterpartyId, verificationReason));


        // This method is the same with CounterpartyService.GetCounterparty,
        // because administrator services in the future will be replaced to another application
        private async Task<Result<Counterparty>> GetCounterparty(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);

            if (counterparty == null)
                return Result.Failure<Counterparty>("Could not find counterparty with specified id");

            return Result.Success(counterparty);
        }


        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IManagementAuditService _managementAuditService;
    }
}