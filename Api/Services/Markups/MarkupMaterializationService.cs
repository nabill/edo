using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupMaterializationService : IMarkupMaterializationService
    {
        public MarkupMaterializationService(EdoContext context)
        {
            _context = context;
        }


        public async Task Materialize()
        {
            var recordsForMaterialization = await GetData();
            if (!recordsForMaterialization.Any())
                return;

            foreach (var materializationData in recordsForMaterialization)
            {
                await Result.Success(materializationData)
                    .BindWithTransaction(_context, m => Result.Success(m)
                        .Map(ApplyBonus)
                        .Map(MarkAsPaid)
                        .Tap(WriteLog)
                    );
            }
        }


        private Task<List<MaterializationData>> GetData()
        {
            var query =
                from appliedMarkup in _context.AppliedBookingMarkups
                join booking in _context.Bookings on appliedMarkup.ReferenceCode equals booking.ReferenceCode
                join policy in _context.MarkupPolicies on appliedMarkup.PolicyId equals policy.Id
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join agencyAccount in _context.AgencyAccounts on agency.Id equals agencyAccount.AgencyId
                where booking.CheckOutDate >= DateTime.UtcNow && appliedMarkup.Paid == null && policy.ScopeType == MarkupPolicyScopeType.Agent
                select new MaterializationData
                {
                    PolicyId = appliedMarkup.PolicyId,
                    ReferenceCode = appliedMarkup.ReferenceCode,
                    AgencyAccountId = agencyAccount.Id,
                    Amount = appliedMarkup.Amount
                };

            return query.ToListAsync();
        }


        private async Task<MaterializationData> ApplyBonus(MaterializationData data)
        {
            var agencyAccount = await _context.AgencyAccounts
                .SingleOrDefaultAsync(a => a.Id == data.AgencyAccountId);

            agencyAccount.Balance += data.Amount;
            _context.AgencyAccounts.Update(agencyAccount);
            await _context.SaveChangesAsync();
            return data;
        }


        private async Task<MaterializationData> MarkAsPaid(MaterializationData data)
        {
            var appliedMarkup = await _context.AppliedBookingMarkups
                .SingleOrDefaultAsync(a => a.PolicyId == data.PolicyId && a.ReferenceCode == data.ReferenceCode);

            appliedMarkup.Paid = DateTime.UtcNow;
            _context.AppliedBookingMarkups.Update(appliedMarkup);
            await _context.SaveChangesAsync();
            return data;
        }


        private async Task WriteLog(MaterializationData data)
        {
            _context.MaterializationBonusLogs.Add(new MaterializationBonusLog
            {
                PolicyId = data.PolicyId,
                ReferenceCode = data.ReferenceCode,
                AgencyAccountId = data.AgencyAccountId,
                Amount = data.Amount,
                Paid = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }


        private readonly EdoContext _context;
    }
}