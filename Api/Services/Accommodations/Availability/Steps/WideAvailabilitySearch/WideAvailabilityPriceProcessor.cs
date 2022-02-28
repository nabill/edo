using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilityPriceProcessor : IWideAvailabilityPriceProcessor
    {
        public WideAvailabilityPriceProcessor(IPriceProcessor priceProcessor)
        {
            _priceProcessor = priceProcessor;
        }
        
        
        public async Task<List<AccommodationAvailabilityResult>> ApplyMarkups(List<AccommodationAvailabilityResult> results, AgentContext agent)
        {
            var convertedResults = new List<AccommodationAvailabilityResult>(results.Count);
            var subjectInfo = agent.ToMarkupSubjectInfo();
            foreach (var slimAccommodationAvailability in results)
            {
                var convertedRoomContractSets = new List<RoomContractSet>(slimAccommodationAvailability.RoomContractSets.Count);
                foreach (var roomContractSet in slimAccommodationAvailability.RoomContractSets)
                {
                    var convertedRoomContractSet = await _priceProcessor.ApplyMarkups(subjectInfo,
                        roomContractSet,
                        async (rcs, function) => await RoomContractSetPriceProcessor.ProcessPrices(rcs, function),
                        _ => GetMarkupDestinationInfo(slimAccommodationAvailability));
                    
                    convertedRoomContractSets.Add(convertedRoomContractSet);
                }
                
                convertedResults.Add(new AccommodationAvailabilityResult(searchId: slimAccommodationAvailability.SearchId,
                    supplierCode: slimAccommodationAvailability.SupplierCode,
                    created: slimAccommodationAvailability.Created,
                    availabilityId: slimAccommodationAvailability.AvailabilityId,
                    roomContractSets: convertedRoomContractSets,
                    minPrice: slimAccommodationAvailability.MinPrice,
                    maxPrice: slimAccommodationAvailability.MaxPrice,
                    checkInDate: slimAccommodationAvailability.CheckInDate,
                    checkOutDate: slimAccommodationAvailability.CheckOutDate,
                    htId: slimAccommodationAvailability.HtId,
                    supplierAccommodationCode: slimAccommodationAvailability.SupplierAccommodationCode,
                    countryHtId: slimAccommodationAvailability.CountryHtId,
                    localityHtId: slimAccommodationAvailability.LocalityHtId));
            }

            return convertedResults;
        }


        public async Task<Result<List<AccommodationAvailabilityResult>, ProblemDetails>> ConvertCurrencies(List<AccommodationAvailabilityResult> results)
        {
            var convertedResults = new List<AccommodationAvailabilityResult>(results.Count);
            foreach (var slimAccommodationAvailability in results)
            {
                var convertedRoomContractSets = new List<RoomContractSet>(slimAccommodationAvailability.RoomContractSets.Count);
                foreach (var roomContractSet in slimAccommodationAvailability.RoomContractSets)
                {
                    var (_, isFailure, convertedRoomContractSet, error) = await _priceProcessor.ConvertCurrencies(roomContractSet,
                        async (rcs, function) => await RoomContractSetPriceProcessor.ProcessPrices(rcs, function),
                        GetCurrency);
                    
                    if (isFailure)
                        return Result.Failure<List<AccommodationAvailabilityResult>, ProblemDetails>(error);
                    
                    convertedRoomContractSets.Add(convertedRoomContractSet);
                }

                convertedResults.Add(new AccommodationAvailabilityResult(searchId: slimAccommodationAvailability.SearchId,
                    supplierCode: slimAccommodationAvailability.SupplierCode,
                    created: slimAccommodationAvailability.Created,
                    availabilityId: slimAccommodationAvailability.AvailabilityId,
                    roomContractSets: convertedRoomContractSets,
                    minPrice: slimAccommodationAvailability.MinPrice,
                    maxPrice: slimAccommodationAvailability.MaxPrice,
                    checkInDate: slimAccommodationAvailability.CheckInDate,
                    checkOutDate: slimAccommodationAvailability.CheckOutDate,
                    htId: slimAccommodationAvailability.HtId,
                    supplierAccommodationCode: slimAccommodationAvailability.SupplierAccommodationCode,
                    countryHtId: slimAccommodationAvailability.CountryHtId,
                    localityHtId: slimAccommodationAvailability.LocalityHtId));
            }

            return convertedResults;
        }
        
        
        public List<AccommodationAvailabilityResult> AlignPrices(List<AccommodationAvailabilityResult> results)
        {
            var convertedResults = new List<AccommodationAvailabilityResult>(results.Count);
            foreach (var accommodationAvailability in results)
            {
                // Currency can differ in different results
                var roomContractSets = RoomContractSetPriceProcessor.AlignPrices(accommodationAvailability.RoomContractSets);
                convertedResults.Add(new AccommodationAvailabilityResult(searchId: accommodationAvailability.SearchId,
                    supplierCode: accommodationAvailability.SupplierCode,
                    created: accommodationAvailability.Created,
                    availabilityId: accommodationAvailability.AvailabilityId,
                    roomContractSets: roomContractSets,
                    minPrice: accommodationAvailability.MinPrice,
                    maxPrice: accommodationAvailability.MaxPrice,
                    checkInDate: accommodationAvailability.CheckInDate,
                    checkOutDate: accommodationAvailability.CheckOutDate,
                    htId: accommodationAvailability.HtId,
                    supplierAccommodationCode: accommodationAvailability.SupplierAccommodationCode,
                    countryHtId: accommodationAvailability.CountryHtId,
                    localityHtId: accommodationAvailability.LocalityHtId));
            }

            return convertedResults;
        }
        
        
        private static Currencies? GetCurrency(RoomContractSet roomContractSet)
        {
            return roomContractSet.Rate.Currency == Currencies.NotSpecified
                ? null
                : roomContractSet.Rate.Currency;
        }


        private static MarkupDestinationInfo GetMarkupDestinationInfo(AccommodationAvailabilityResult availability)
            => new ()
            {
                CountryHtId = availability.CountryHtId,
                LocalityHtId = availability.LocalityHtId,
                AccommodationHtId = availability.HtId
            };


        private readonly IPriceProcessor _priceProcessor;
        private readonly IAgencyService _agencyService;
    }
}