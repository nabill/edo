using System.Collections.Generic;
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
        
        
        public async Task<Result<SingleAccommodationAvailability, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailability availabilityDetails)
        {
            var convertedRoomContractSets = new List<RoomContractSet>(availabilityDetails.RoomContractSets.Count);
            foreach (var roomContractSet in availabilityDetails.RoomContractSets)
            {
                var (_, isFailure, convertedRoomContractSet, error) = await _priceProcessor.ConvertCurrencies(roomContractSet,
                    async (rcs, function) => await RoomContractSetPriceProcessor.ProcessPrices(rcs, function),
                    GetCurrency);
                    
                if (isFailure)
                    return Result.Failure<SingleAccommodationAvailability, ProblemDetails>(error);
                    
                convertedRoomContractSets.Add(convertedRoomContractSet);
            }

            return new SingleAccommodationAvailability(availabilityId: availabilityDetails.AvailabilityId,
                checkInDate: availabilityDetails.CheckInDate,
                roomContractSets: convertedRoomContractSets,
                htId: availabilityDetails.HtId,
                countryHtId: availabilityDetails.CountryHtId,
                localityHtId: availabilityDetails.LocalityHtId);
        }


        public async Task<SingleAccommodationAvailability> ApplyMarkups(SingleAccommodationAvailability response, AgentContext agent)
        {
            var convertedRoomContractSets = new List<RoomContractSet>(response.RoomContractSets.Count);
            var subjectInfo = agent.ToMarkupSubjectInfo();
            foreach (var roomContractSet in response.RoomContractSets)
            {
                var convertedRoomContractSet = await _priceProcessor.ApplyMarkups(subjectInfo,
                    roomContractSet,
                    async (rcs, function) => await RoomContractSetPriceProcessor.ProcessPrices(rcs, function),
                    _ => GetMarkupDestinationInfo(response));

                convertedRoomContractSets.Add(convertedRoomContractSet);
            }

            return new SingleAccommodationAvailability(availabilityId: response.AvailabilityId,
                checkInDate: response.CheckInDate,
                roomContractSets: convertedRoomContractSets,
                htId: response.HtId,
                countryHtId: response.CountryHtId,
                localityHtId: response.LocalityHtId);
        }


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


        private static MarkupDestinationInfo GetMarkupDestinationInfo(SingleAccommodationAvailability availability)
            => new()
            {
                AccommodationHtId = availability.HtId,
                CountryHtId = availability.CountryHtId,
                LocalityHtId = availability.LocalityHtId
            };


        private static Currencies? GetCurrency(RoomContractSet roomContractSet)
        {
            return roomContractSet.Rate.Currency == Currencies.NotSpecified
                ? null
                : roomContractSet.Rate.Currency;
        }
        
        
        private readonly IPriceProcessor _priceProcessor;
    }
}