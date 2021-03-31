using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
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
            var agencyInfo = await GetChildAgencyInfos(agent.AgencyId)
                .SingleOrDefaultAsync(a => a.Id == agencyId);

            return agencyInfo.Equals(default) 
                ? Result.Failure<ChildAgencyInfo>("Could not get child agency")
                : agencyInfo;
        }


        public Task<List<ChildAgencyInfo>> Get(AgentContext agent)
            => GetChildAgencyInfos(agent.AgencyId)
                .ToListAsync();


        private IQueryable<ChildAgencyInfo> GetChildAgencyInfos(int parentAgencyId)
        {
            return from agency in _context.Agencies
                join agencyAccount in _context.AgencyAccounts on agency.Id equals agencyAccount.AgencyId
                where agency.ParentId == parentAgencyId
                select new ChildAgencyInfo
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    AccountBalance = new MoneyAmount
                    {
                        Amount = agencyAccount.Balance,
                        Currency = agencyAccount.Currency
                    },
                    Created = agency.Created,
                    IsActive = agency.IsActive
                };
        }


        private readonly EdoContext _context;
    }
}
