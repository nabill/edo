using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentService
    {
        Task<Result<List<SlimAgentInfo>>> GetAgents(int agencyId);
    }
}
