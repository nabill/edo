using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class CounterpartyService : ICounterpartyService
    {
        public CounterpartyService(EdoContext context,
            IAccountManagementService accountManagementService,
            IDateTimeProvider dateTimeProvider,
            IAgentContextService agentContextService)
        {
            _context = context;
            _accountManagementService = accountManagementService;
            _dateTimeProvider = dateTimeProvider;
            _agentContextService = agentContextService;
        }


        public async Task<Result<Counterparty>> Add(CounterpartyInfo counterparty)
        {
            var (_, isFailure, error) = CounterPartyValidator.Validate(counterparty);
            if (isFailure)
                return Result.Failure<Counterparty>(error);

            var now = _dateTimeProvider.UtcNow();
            var createdCounterparty = new Counterparty
            {
                Address = counterparty.Address,
                City = counterparty.City,
                CountryCode = counterparty.CountryCode,
                Fax = counterparty.Fax,
                Name = counterparty.Name,
                Phone = counterparty.Phone,
                Website = counterparty.Website,
                PostalCode = counterparty.PostalCode,
                PreferredCurrency = counterparty.PreferredCurrency,
                PreferredPaymentMethod = counterparty.PreferredPaymentMethod,
                State = CounterpartyStates.PendingVerification,
                Created = now,
                Updated = now
            };

            _context.Counterparties.Add(createdCounterparty);
            await _context.SaveChangesAsync();

            var defaultAgency = new Agency
            {
                Name = createdCounterparty.Name,
                CounterpartyId = createdCounterparty.Id,
                IsDefault = true,
                Created = now,
                Modified = now,
            };
            _context.Agencies.Add(defaultAgency);

            await _context.SaveChangesAsync();
            return Result.Ok(createdCounterparty);
        }


        public Task<Result<CounterpartyInfo>> Get(int counterpartyId)
        {
            return GetCounterpartyForAgent(counterpartyId)
                .Map(counterparty => new CounterpartyInfo(
                    counterparty.Name,
                    counterparty.Address,
                    counterparty.CountryCode,
                    counterparty.City,
                    counterparty.Phone,
                    counterparty.Fax,
                    counterparty.PostalCode,
                    counterparty.PreferredCurrency,
                    counterparty.PreferredPaymentMethod,
                    counterparty.Website,
                    counterparty.VatNumber));
        }


        public Task<Result<Agency>> AddAgency(int counterpartyId, AgencyInfo agency)
        {
            Counterparty counterparty = null;

            return CheckCounterpartyExists()
                .Ensure(HasPermissions, "Permission to create agencies denied")
                .Ensure(IsAgencyNameUnique, $"Agency with name {agency.Name} already exists")
                .Map(SaveAgency)
                .Bind(CreateAccountIfVerified);


            async Task<bool> HasPermissions()
            {
                var agent = await _agentContextService.GetAgent();
                return agent.IsMaster && agent.CounterpartyId == counterpartyId;
            }


            async Task<Result> CheckCounterpartyExists()
            {
                counterparty = await _context.Counterparties.Where(c => c.Id == counterpartyId).SingleOrDefaultAsync();
                return counterparty == null
                    ? Result.Failure("Could not find the counterparty with specified id")
                    : Result.Ok();
            }


            async Task<bool> IsAgencyNameUnique()
            {
                return !await _context.Agencies.Where(a => a.CounterpartyId == counterpartyId &&
                        EF.Functions.ILike(a.Name, agency.Name))
                    .AnyAsync();
            }


            async Task<Agency> SaveAgency()
            {
                var now = _dateTimeProvider.UtcNow();
                var createdAgency = new Agency
                {
                    Name = agency.Name,
                    CounterpartyId = counterpartyId,
                    IsDefault = false,
                    Created = now,
                    Modified = now,
                };
                _context.Agencies.Add(createdAgency);
                await _context.SaveChangesAsync();

                return createdAgency;
            }


            async Task<Result<Agency>> CreateAccountIfVerified(Agency createdAgency)
            {
                if (!new[] {CounterpartyStates.FullAccess, CounterpartyStates.ReadOnly}.Contains(counterparty.State))
                    return Result.Ok(createdAgency);

                var (_, isFailure, error) = await _accountManagementService.CreateForAgency(createdAgency, counterparty.PreferredCurrency);
                if (isFailure)
                    return Result.Failure<Agency>(error);

                return Result.Ok(createdAgency);
            }
        }


        public async Task<Result<AgencyInfo>> GetAgency(int agencyId)
        {
            var agent = await _agentContextService.GetAgent();

            if (!await _agentContextService.IsAgentAffiliatedWithAgency(agent.AgentId, agencyId))
                return Result.Failure<AgencyInfo>("The agent is not affiliated with the agency");

            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (agency == null)
                return Result.Failure<AgencyInfo>("Could not find agency with specified id");

            return Result.Ok(new AgencyInfo(agency.Name, agency.Id));
        }


        public Task<Agency> GetDefaultAgency(int counterpartyId)
            => _context.Agencies
                .SingleAsync(a => a.CounterpartyId == counterpartyId && a.IsDefault);


        private async Task<Result<Counterparty>> GetCounterpartyForAgent(int counterpartyId)
        {
            var agent = await _agentContextService.GetAgent();

            return await GetCounterparty(counterpartyId)
                .Ensure(x => _agentContextService.IsAgentAffiliatedWithCounterparty(agent.AgentId, counterpartyId),
                    "The agent isn't affiliated with the counterparty");
        }


        private async Task<Result<Counterparty>> GetCounterparty(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);

            if (counterparty == null)
                return Result.Failure<Counterparty>("Could not find counterparty with specified id");

            return Result.Ok(counterparty);
        }


        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly IAgentContextService _agentContextService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}