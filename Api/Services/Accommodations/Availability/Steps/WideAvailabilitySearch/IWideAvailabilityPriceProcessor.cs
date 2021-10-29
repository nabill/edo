using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilityPriceProcessor
    {
        Task<List<AccommodationAvailabilityResult>> ApplyMarkups(List<AccommodationAvailabilityResult> results, AgentContext agent);

        Task<Result<List<AccommodationAvailabilityResult>, ProblemDetails>> ConvertCurrencies(List<AccommodationAvailabilityResult> results);

        List<AccommodationAvailabilityResult> AlignPrices(List<AccommodationAvailabilityResult> results);
    }
}