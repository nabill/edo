using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class ChildAgencyService : IChildAgencyService
    {
        public ChildAgencyService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<ChildAgencyInfo>> GetChildAgency(int agencyId, AgentContext agent)
        {
            var agency = await _context.Agencies
                .SingleOrDefaultAsync(a => a.Id == agencyId && a.ParentId == agent.AgencyId);

            return agency is null
                ? Result.Failure<ChildAgencyInfo>("Could not get child agency")
                : new ChildAgencyInfo(agency.Id, agency.Name, agency.IsActive, agency.Created);
        }


        public Task<List<ChildAgencyInfo>> GetChildAgencies(AgentContext agent)
            => _context.Agencies.Where(a => a.ParentId == agent.AgencyId)
                .Select(a => new ChildAgencyInfo(a.Id, a.Name, a.IsActive, a.Created))
                .ToListAsync();


        private readonly EdoContext _context;
    }
}
