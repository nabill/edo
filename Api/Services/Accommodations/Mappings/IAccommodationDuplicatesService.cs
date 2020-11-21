using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.AccommodationMappings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public interface IAccommodationDuplicatesService
    {
        Task<Result> Report(ReportAccommodationDuplicateRequest duplicateRequest, AgentContext agent);

        Task<HashSet<SupplierAccommodationId>> Get(AgentContext agent);

        Task<Dictionary<SupplierAccommodationId, string>> GetDuplicateReports(List<SupplierAccommodationId> supplierAccommodationIds);
    }
}