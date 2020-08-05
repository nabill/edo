using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IAccommodationDuplicateReportsManagementService
    {
        Task<List<SlimAccommodationDuplicateReportInfo>> Get();
        
        Task<Result<AccommodationDuplicateReportInfo>> Get(int reportId, string languageCode);

        Task<Result> Approve(int reportId);

        Task<Result> Disapprove(int reportId);
    }
}