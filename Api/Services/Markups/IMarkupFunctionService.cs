using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupFunctionService
    {
        Task<List<(MarkupPolicy Policy, PriceProcessFunction Function)>> GetFunctions(AgentContext agentContext, MarkupPolicyTarget policyTarget);
    }
}