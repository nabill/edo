using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Deadline
{
    public interface ICancelationPoliciesService
    {
        Task<Result<DeadlineDetails, ProblemDetails>> GetDeadlineDetails(string availabilityId,
            string accommodationId,
            string tariffCode,
            DataProviders contractType,
            string languageCode);
    }
}
