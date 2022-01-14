using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IAvailabilityRequestStorage
    {
        Task Set(Guid searchId, AvailabilityRequest request);
        Task<Result<AvailabilityRequest>> Get(Guid searchId);
    }
}