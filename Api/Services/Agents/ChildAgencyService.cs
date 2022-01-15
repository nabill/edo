using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class ChildAgencyService : IChildAgencyService
    {
        public ChildAgencyService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<ChildAgencyInfo>> Get(int agencyId, AgentContext agent)
        {
            var agency = await GetChildAgencies(agent.AgencyId)
                .SingleOrDefaultAsync(a => a.Id == agencyId);

            if (agency == default)
                return Result.Failure<ChildAgencyInfo>("Could not get child agency");

            var agencyAccounts = await _context.AgencyAccounts
                .Where(acc => acc.AgencyId == agency.Id)
                .ToListAsync();

            return new ChildAgencyInfo
            {
                Id = agency.Id,
                Name = agency.Name,
                Created = agency.Created.DateTime,
                IsActive = agency.IsActive,
                Accounts = agencyAccounts.Select(acc =>
                    new AgencyAccountInfo
                    {
                        Balance = new MoneyAmount
                        {
                            Amount = acc.Balance,
                            Currency = acc.Currency
                        },
                        Currency = acc.Currency,
                        Id = acc.Id
                    }).ToList()
            };
        }


        public Task<List<SlimChildAgencyInfo>> Get(AgentContext agent)
            => GetChildAgencies(agent.AgencyId)
                .Select(a => new SlimChildAgencyInfo
                {
                    Id = a.Id,
                    Name = a.Name,
                    Created = a.Created.DateTime,
                    IsActive = a.IsActive
                })
                .ToListAsync();


        private IQueryable<Agency> GetChildAgencies(int parentAgencyId) 
            => _context.Agencies.Where(a => a.ParentId == parentAgencyId);


        private readonly EdoContext _context;
    }
}
