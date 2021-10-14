using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
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
            foreach (var slimAccommodationAvailability in results)
            {
                // Currency can differ in different results
                var convertedAccommodationAvailability = await _priceProcessor.ApplyMarkups(agent,
                    slimAccommodationAvailability,
                    ProcessPrices);

                convertedResults.Add(convertedAccommodationAvailability);
            }

            return convertedResults;
        }


        public async Task<Result<List<AccommodationAvailabilityResult>, ProblemDetails>> ConvertCurrencies(List<AccommodationAvailabilityResult> results, AgentContext agent)
        {
            var convertedResults = new List<AccommodationAvailabilityResult>(results.Count);
            foreach (var slimAccommodationAvailability in results)
            {
                // Currency can differ in different results
                var (_, isFailure, convertedAccommodationAvailability, error) = await _priceProcessor.ConvertCurrencies(agent,
                    slimAccommodationAvailability,
                    ProcessPrices,
                    GetCurrency);

                if (isFailure)
                    return Result.Failure<List<AccommodationAvailabilityResult>, ProblemDetails>(error);
                    
                convertedResults.Add(convertedAccommodationAvailability);
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
                    supplier: accommodationAvailability.Supplier,
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
        
        
        private static Currencies? GetCurrency(AccommodationAvailabilityResult accommodationAvailability)
        {
            if (!accommodationAvailability.RoomContractSets.Any())
                return null;

            return accommodationAvailability.RoomContractSets.First().Rate.Currency;
        }
        

        private static async ValueTask<AccommodationAvailabilityResult> ProcessPrices(AccommodationAvailabilityResult accommodationAvailability, PriceProcessFunction function)
        {
            var supplierRoomContractSets = accommodationAvailability.RoomContractSets;
            var roomContractSetsWithMarkup = await RoomContractSetPriceProcessor.ProcessPrices(supplierRoomContractSets, function);
            var convertedAccommodationAvailability = new AccommodationAvailabilityResult(searchId: accommodationAvailability.SearchId,
                supplier: accommodationAvailability.Supplier,
                created: accommodationAvailability.Created,
                availabilityId: accommodationAvailability.AvailabilityId,
                roomContractSets: roomContractSetsWithMarkup,
                minPrice: roomContractSetsWithMarkup.Min(rcs => rcs.Rate.FinalPrice.Amount),
                maxPrice: roomContractSetsWithMarkup.Max(rcs => rcs.Rate.FinalPrice.Amount),
                checkInDate: accommodationAvailability.CheckInDate,
                checkOutDate: accommodationAvailability.CheckOutDate,
                htId: accommodationAvailability.HtId,
                supplierAccommodationCode: accommodationAvailability.SupplierAccommodationCode,
                countryHtId: accommodationAvailability.CountryHtId,
                localityHtId: accommodationAvailability.LocalityHtId);
            
            return convertedAccommodationAvailability;
        }
        

        private readonly IPriceProcessor _priceProcessor;
    }
}