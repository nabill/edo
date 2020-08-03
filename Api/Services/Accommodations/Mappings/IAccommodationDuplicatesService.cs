using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public interface IAccommodationDuplicatesService
    {
        Task<Result> Report(ReportAccommodationDuplicateRequest duplicateRequest, AgentContext agent);

        Task<HashSet<ProviderAccommodationId>> Get(AgentContext agent);
    }
}