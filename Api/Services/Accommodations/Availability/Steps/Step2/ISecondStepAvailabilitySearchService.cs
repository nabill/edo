using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step2
{
    public interface ISecondStepAvailabilitySearchService
    {
        Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
            string accommodationId, string availabilityId, AgentContext agent,
            string languageCode);
    }
}