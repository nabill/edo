using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Deadline
{
    public interface IDeadlineService
    {
        Task<Result<DeadlineDetails, ProblemDetails>> GetDeadlineDetails(DeadlineDetailsRequest request);
    }
}
