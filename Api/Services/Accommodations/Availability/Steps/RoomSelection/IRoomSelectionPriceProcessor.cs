using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionPriceProcessor
    {
        Task<Result<SingleAccommodationAvailability, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailability availabilityDetails, AgentContext agent);

        Task<SingleAccommodationAvailability> ApplyMarkups(SingleAccommodationAvailability response, AgentContext agent);
        
        ValueTask<SingleAccommodationAvailability> AlignPrices(SingleAccommodationAvailability availabilityDetails);
    }
}