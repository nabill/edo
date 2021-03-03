using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupBonusMaterializationService  : IMarkupBonusMaterializationService
    {
        public MarkupBonusMaterializationService(EdoContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }


        public Task<List<int>> GetForMaterialize(DateTime dateTime)
        {
            var query =
                from appliedMarkup in _context.AppliedBookingMarkups
                join booking in _context.Bookings on appliedMarkup.ReferenceCode equals booking.ReferenceCode
                join policy in _context.MarkupPolicies on appliedMarkup.PolicyId equals policy.Id
                where 
                    booking.Status == BookingStatuses.Confirmed &&
                    booking.PaymentStatus == BookingPaymentStatuses.Captured &&
                    booking.CheckOutDate.Date >= dateTime && 
                    appliedMarkup.Paid == null &&
                    SupportedPolicyScopeTypes.Contains(policy.ScopeType)
                select appliedMarkup.Id;

            return query.ToListAsync();
        }


        public async Task<Result<BatchOperationResult>> Materialize(List<int> markupsForMaterialization)
        {
            foreach (var materializationData in await GetData(markupsForMaterialization))
                await ApplyBonus(materializationData);

            return new BatchOperationResult($"{markupsForMaterialization.Count} markups materialized", false);
        }


        private Task<List<MaterializationData>> GetData(ICollection<int> markupsForMaterialization)
        {
            var query =
                from appliedMarkup in _context.AppliedBookingMarkups
                join booking in _context.Bookings on appliedMarkup.ReferenceCode equals booking.ReferenceCode
                join policy in _context.MarkupPolicies on appliedMarkup.PolicyId equals policy.Id
                where 
                    markupsForMaterialization.Contains(appliedMarkup.Id) &&
                    appliedMarkup.Paid == null &&
                    policy.AgencyId != null
                select new MaterializationData
                {
                    PolicyId = appliedMarkup.PolicyId,
                    ReferenceCode = appliedMarkup.ReferenceCode,
                    AgencyId = policy.AgencyId.Value,
                    Amount = appliedMarkup.Amount,
                    ScopeType = policy.ScopeType
                };

            return query.ToListAsync();
        }


        private async Task ApplyBonus(MaterializationData data)
        {
            var applyBonusTask = data.ScopeType switch
            {
                MarkupPolicyScopeType.Agency => ApplyAgencyBonus(),
                MarkupPolicyScopeType.Agent => ApplyAgentBonus(),
                _ => Task.CompletedTask
            };

            await applyBonusTask;


            Task ApplyAgencyBonus() 
                => ApplyBonus(data.PolicyId, data.ReferenceCode, data.AgencyId, data.Amount);


            async Task ApplyAgentBonus()
            {
                var parentAgencyQuery = from agency in _context.Agencies
                    join parentAgency in _context.Agencies on agency.ParentId equals parentAgency.Id
                    where agency.Id == data.AgencyId
                    select parentAgency.Id;

                var parentAgencyId = await parentAgencyQuery.SingleOrDefaultAsync();
                await ApplyBonus(data.PolicyId, data.ReferenceCode, parentAgencyId, data.Amount);
            }
        }


        private async Task ApplyBonus(int policyId, string referenceCode, int agencyId, decimal amount)
        {
            var agencyAccount = await _context.AgencyAccounts
                .SingleOrDefaultAsync(a => a.AgencyId == agencyId);
                
            if (agencyAccount is null)
                return;

            var paidDate = _dateTimeProvider.UtcNow();

            await Result.Success()
                .BindWithTransaction(_context, () => Result.Success()
                    .Tap(UpdateBalance)
                    .Tap(MarkAsPaid)
                    .Tap(WriteLog)
                );


            async Task UpdateBalance()
            {
                agencyAccount.Balance += amount;
                _context.AgencyAccounts.Update(agencyAccount);
                await _context.SaveChangesAsync();
                _context.Detach(agencyAccount);
            }


            async Task MarkAsPaid()
            {
                var appliedMarkup = await _context.AppliedBookingMarkups
                    .SingleOrDefaultAsync(a => a.PolicyId == policyId && a.ReferenceCode == referenceCode);

                appliedMarkup.Paid = paidDate;
                _context.AppliedBookingMarkups.Update(appliedMarkup);
                await _context.SaveChangesAsync();
                _context.Detach(appliedMarkup);
            }


            async Task WriteLog()
            {
                await _context.MaterializationBonusLogs.AddAsync(new MaterializationBonusLog
                {
                    PolicyId = policyId,
                    ReferenceCode = referenceCode,
                    AgencyAccountId = agencyAccount.Id,
                    Amount = amount,
                    Created = paidDate
                });
                await _context.SaveChangesAsync();
            }
        }


        private static readonly HashSet<MarkupPolicyScopeType> SupportedPolicyScopeTypes 
            = new() {MarkupPolicyScopeType.Agent, MarkupPolicyScopeType.Agency};


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}