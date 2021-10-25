using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyManagementService : ICounterpartyManagementService
    {
        public CounterpartyManagementService(EdoContext context, IManagementAuditService managementAuditService, 
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            var counterpartyInfo = await 
                (from cp in _context.Counterparties
                join c in _context.Countries on cp.CountryCode equals c.Code
                where cp.Id == counterpartyId
                select cp.ToCounterpartyInfo(c.Names, languageCode, null))
                    .SingleOrDefaultAsync();

            if (counterpartyInfo.Id == default)
                return Result.Failure<CounterpartyInfo>("Could not find counterparty with specified id");

            return counterpartyInfo;
        }


        public async Task<List<SlimCounterpartyInfo>> Get()
        {
            var counterparties = await (from cp in _context.Counterparties
                join formula in _context.DisplayMarkupFormulas on new
                {
                    Id = (int?) cp.Id,
                    AgencyId = (int?) null,
                    AgentId = (int?) null
                } equals new
                {
                    Id = formula.CounterpartyId,
                    formula.AgencyId,
                    formula.AgentId
                } into formulas
                from markupFormula in formulas.DefaultIfEmpty()
                select new
                {
                    Counterparty = cp,
                    MarkupFormula = markupFormula == null ? null : markupFormula.DisplayFormula
                }).ToListAsync();

            return counterparties.Select(c => ToCounterpartySlimInfo(c.Counterparty, c.MarkupFormula)).ToList();
        }


        public Task<List<CounterpartyPrediction>> GetCounterpartiesPredictions(string query)
            => (from c in _context.Counterparties
                    join ag in _context.Agencies on c.Id equals ag.CounterpartyId
                    join ar in _context.AgentAgencyRelations on ag.Id equals ar.AgencyId
                    join a in _context.Agents on ar.AgentId equals a.Id
                    where ar.IsActive
                        && ar.Type == AgentAgencyRelationTypes.Master
                        && ag.VerificationState == CounterpartyStates.FullAccess
                        && !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().StartsWith(query.ToLower())
                            || !string.IsNullOrEmpty(a.FirstName) && a.FirstName.ToLower().StartsWith(query.ToLower())
                            || !string.IsNullOrEmpty(a.LastName) && a.LastName.ToLower().StartsWith(query.ToLower())
                            || !string.IsNullOrEmpty(a.Email) && a.Email.ToLower().StartsWith(query.ToLower())
                    select new CounterpartyPrediction(c.Id, c.Name, a.FirstName + " " + a.LastName, ag.BillingEmail ?? a.Email))
                .Distinct()
                .ToListAsync();


        public Task<List<AgencyInfo>> GetAllCounterpartyAgencies(int counterpartyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
        => (
                from a in _context.Agencies
                join c in _context.Countries on a.CountryCode equals c.Code
                join cp in _context.Counterparties on a.CounterpartyId equals cp.Id
                where a.CounterpartyId == counterpartyId
                join ra in _context.Agencies on a.Ancestors.Any() ? a.Ancestors[0] : a.Id equals ra.Id
                select a.ToAgencyInfo(a.ContractKind, ra.VerificationState, ra.Verified, c.Names, languageCode))
            .ToListAsync();


        public async Task<Result<MasterAgentContext>> GetRootAgencyMasterAgent(int counterpartyId)
        {
            var rootAgency = await _context.Agencies
                .SingleAsync(a => a.CounterpartyId == counterpartyId && a.ParentId == null);
            
            var master = await(from agent in _context.Agents
                join relation in _context.AgentAgencyRelations on agent.Id equals relation.AgentId
                where relation.AgencyId == rootAgency.Id && relation.Type == AgentAgencyRelationTypes.Master
                select new MasterAgentContext { AgentId = agent.Id, FirstName = agent.FirstName, LastName = agent.LastName, Email = agent.Email, 
                    AgencyId = relation.AgencyId})
                .FirstOrDefaultAsync();

            return (master is null)
                ? Result.Failure<MasterAgentContext>($"Master agent in root agency {rootAgency.Name} does not exist")
                : master;
        }


        public Task<Result<CounterpartyInfo>> Update(CounterpartyEditRequest changedCounterpartyInfo, int counterpartyId)
        {
            return GetCounterparty(counterpartyId)
                .BindWithTransaction(_context, c => UpdateCounterparty(c)
                    .Check(WriteAuditLog));


            async Task<Result<CounterpartyInfo>> UpdateCounterparty(Counterparty counterpartyToUpdate)
            {
                if (string.IsNullOrWhiteSpace(changedCounterpartyInfo.Name))
                    return Result.Failure<CounterpartyInfo>("Name must not be empty");
                
                counterpartyToUpdate.Name = changedCounterpartyInfo.Name;
                counterpartyToUpdate.PreferredPaymentMethod = changedCounterpartyInfo.PreferredPaymentMethod;
                counterpartyToUpdate.Updated = _dateTimeProvider.UtcNow();
                counterpartyToUpdate.Address = changedCounterpartyInfo.Address;
                counterpartyToUpdate.BillingEmail = changedCounterpartyInfo.BillingEmail;
                counterpartyToUpdate.City = changedCounterpartyInfo.City;
                counterpartyToUpdate.CountryCode = changedCounterpartyInfo.CountryCode;
                counterpartyToUpdate.Fax = changedCounterpartyInfo.Fax;
                counterpartyToUpdate.Phone = changedCounterpartyInfo.Phone;
                counterpartyToUpdate.PostalCode = changedCounterpartyInfo.PostalCode;
                counterpartyToUpdate.Website = changedCounterpartyInfo.Website;
                counterpartyToUpdate.VatNumber = changedCounterpartyInfo.VatNumber;
                
                _context.Counterparties.Update(counterpartyToUpdate);
                await _context.SaveChangesAsync();

                return await Get(counterpartyId);
            }


            Task<Result> WriteAuditLog(CounterpartyInfo _)
                => _managementAuditService.Write(ManagementEventType.CounterpartyEdit, new CounterpartyEditEventData(counterpartyId, changedCounterpartyInfo));
        }


        // This method is the same with CounterpartyService.GetCounterparty,
        // because administrator services in the future will be replaced to another application
        private async Task<Result<Counterparty>> GetCounterparty(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);

            if (counterparty == null)
                return Result.Failure<Counterparty>("Could not find counterparty with specified id");

            return counterparty;
        }


        private static SlimCounterpartyInfo ToCounterpartySlimInfo(Counterparty counterparty, string markupFormula = null)
            => new (counterparty.Id,
                counterparty.Name,
                counterparty.LegalAddress,
                counterparty.PreferredPaymentMethod,
                counterparty.IsContractUploaded,
                markupFormula);
        

        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}