using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionPriceProcessor : IRoomSelectionPriceProcessor
    {
        public RoomSelectionPriceProcessor(IPriceProcessor priceProcessor)
        {
            _priceProcessor = priceProcessor;
        }
        
        
        public Task<Result<SingleAccommodationAvailability, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailability availabilityDetails, AgentContext agent)
            => _priceProcessor.ConvertCurrencies(agent, availabilityDetails, ProcessPrices, GetCurrency);


        public Task<SingleAccommodationAvailability> ApplyMarkups(SingleAccommodationAvailability response, AgentContext agent)
            => _priceProcessor.ApplyMarkups(agent, response, ProcessPrices, GetMarkupObjectInfo);


        public SingleAccommodationAvailability AlignPrices(SingleAccommodationAvailability source)
        {
            var roomContractSets = RoomContractSetPriceProcessor.AlignPrices(source.RoomContractSets);
            return new SingleAccommodationAvailability(availabilityId: source.AvailabilityId,
                checkInDate: source.CheckInDate,
                roomContractSets: roomContractSets,
                htId: source.HtId,
                countryHtId: source.CountryHtId,
                localityHtId: source.LocalityHtId);
        }


        private static async ValueTask<SingleAccommodationAvailability> ProcessPrices(SingleAccommodationAvailability source,
            PriceProcessFunction processFunction)
        {
            var roomContractSets = await RoomContractSetPriceProcessor.ProcessPrices(source.RoomContractSets, processFunction);
            return new SingleAccommodationAvailability(availabilityId: source.AvailabilityId,
                checkInDate: source.CheckInDate,
                roomContractSets: roomContractSets,
                htId: source.HtId,
                countryHtId: source.CountryHtId,
                localityHtId: source.LocalityHtId);
        }


        private static MarkupObjectInfo GetMarkupObjectInfo(SingleAccommodationAvailability availability)
            => new()
            {
                AccommodationHtId = availability.HtId,
                CountryHtId = availability.CountryHtId,
                LocalityHtId = availability.LocalityHtId
            };


        private static Currencies? GetCurrency(SingleAccommodationAvailability availabilityDetails)
        {
            if (!availabilityDetails.RoomContractSets.Any())
                return null;
            
            return availabilityDetails.RoomContractSets
                .Select(a => a.Rate.Currency)
                .First();
        }
        
        
        private readonly IPriceProcessor _priceProcessor;
    }
}