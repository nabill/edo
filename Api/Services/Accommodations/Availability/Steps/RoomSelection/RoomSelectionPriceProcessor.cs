using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionPriceProcessor : IRoomSelectionPriceProcessor
    {
        public RoomSelectionPriceProcessor(IPriceProcessor priceProcessor,
            IOptions<ContractKindCommissionOptions> contractKindCommissionOptions)
        {
            _priceProcessor = priceProcessor;
            _contractKindCommissionOptions = contractKindCommissionOptions.Value;
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
                localityHtId: availabilityDetails.LocalityHtId,
                marketId: availabilityDetails.MarketId,
                countryCode: availabilityDetails.CountryCode,
                supplierCode: availabilityDetails.SupplierCode);
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
                localityHtId: response.LocalityHtId,
                marketId: response.MarketId,
                countryCode: response.CountryCode,
                supplierCode: response.SupplierCode);
        }


        public async Task<SingleAccommodationAvailability> AlignPrices(SingleAccommodationAvailability source, AgentContext agent)
        {
            var roomContractSets = RoomContractSetPriceProcessor.AlignPrices(source.RoomContractSets,
                agent.AgencyContractKind, _contractKindCommissionOptions);
            return new SingleAccommodationAvailability(availabilityId: source.AvailabilityId,
                checkInDate: source.CheckInDate,
                roomContractSets: roomContractSets,
                htId: source.HtId,
                countryHtId: source.CountryHtId,
                localityHtId: source.LocalityHtId,
                marketId: source.MarketId,
                countryCode: source.CountryCode,
                supplierCode: source.SupplierCode);
        }


        private static MarkupDestinationInfo GetMarkupDestinationInfo(SingleAccommodationAvailability availability)
            => new()
            {
                AccommodationHtId = availability.HtId,
                CountryHtId = availability.CountryHtId,
                LocalityHtId = availability.LocalityHtId,
                MarketId = availability.MarketId,
                CountryCode = availability.CountryCode,
                SupplierCode = availability.SupplierCode
            };


        private static Currencies? GetCurrency(RoomContractSet roomContractSet)
        {
            return roomContractSet.Rate.Currency == Currencies.NotSpecified
                ? null
                : roomContractSet.Rate.Currency;
        }


        private readonly IPriceProcessor _priceProcessor;
        private readonly ContractKindCommissionOptions _contractKindCommissionOptions;
    }
}