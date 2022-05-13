using System;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationPriceProcessor : IBookingEvaluationPriceProcessor
    {
        public BookingEvaluationPriceProcessor(IPriceProcessor priceProcessor,
            IAgentContextService agentContextService,
            IOptions<ContractKindCommissionOptions> contractKindCommissionOptions)
        {
            _priceProcessor = priceProcessor;
            _agentContextService = agentContextService;
            _contractKindCommissionOptions = contractKindCommissionOptions.Value;
        }


        public Task<RoomContractSetAvailability> ApplyMarkups(RoomContractSetAvailability response, AgentContext agent, Action<MarkupApplicationResult<RoomContractSetAvailability>> logAction)
            => _priceProcessor.ApplyMarkups(agent.ToMarkupSubjectInfo(), response, ProcessPrices, GetMarkupDestinationInfo, logAction);


        public Task<Result<RoomContractSetAvailability, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability availabilityDetails)
            => _priceProcessor.ConvertCurrencies(availabilityDetails, ProcessPrices, GetCurrency);


        public async Task<RoomContractSetAvailability> AlignPrices(RoomContractSetAvailability value)
        {
            var contractKind = await _agentContextService.GetContractKind();

            var roomContractSet = RoomContractSetPriceProcessor.AlignPrices(value.RoomContractSet,
                contractKind, _contractKindCommissionOptions);
            return new RoomContractSetAvailability(availabilityId: value.AvailabilityId,
                checkInDate: value.CheckInDate,
                checkOutDate: value.CheckOutDate,
                numberOfNights: value.NumberOfNights,
                roomContractSet: roomContractSet,
                accommodation: value.Accommodation,
                availablePaymentMethods: value.AvailablePaymentMethods,
                countryHtId: value.CountryHtId,
                localityHtId: value.LocalityHtId,
                evaluationToken: value.EvaluationToken,
                marketId: value.MarketId,
                countryCode: value.CountryCode,
                supplierCode: value.SupplierCode);
        }


        private static async ValueTask<RoomContractSetAvailability> ProcessPrices(RoomContractSetAvailability value,
            PriceProcessFunction processFunction)
        {
            var roomContractSet = await RoomContractSetPriceProcessor.ProcessPrices(value.RoomContractSet, processFunction);
            return new RoomContractSetAvailability(availabilityId: value.AvailabilityId,
                checkInDate: value.CheckInDate,
                checkOutDate: value.CheckOutDate,
                numberOfNights: value.NumberOfNights,
                roomContractSet: roomContractSet,
                accommodation: value.Accommodation,
                availablePaymentMethods: value.AvailablePaymentMethods,
                countryHtId: value.CountryHtId,
                localityHtId: value.LocalityHtId,
                evaluationToken: value.EvaluationToken,
                marketId: value.MarketId,
                countryCode: value.CountryCode,
                supplierCode: value.SupplierCode);
        }


        private static MarkupDestinationInfo GetMarkupDestinationInfo(RoomContractSetAvailability availability)
            => new()
            {
                AccommodationHtId = availability.Accommodation.HtId,
                CountryHtId = availability.CountryHtId,
                LocalityHtId = availability.LocalityHtId,
                MarketId = availability.MarketId,
                CountryCode = availability.CountryCode,
                SupplierCode = availability.SupplierCode
            };


        private static Currencies? GetCurrency(RoomContractSetAvailability availabilityDetails)
            => availabilityDetails.RoomContractSet.Rate.Currency == Currencies.NotSpecified
                ? null
                : availabilityDetails.RoomContractSet.Rate.Currency;


        private readonly IPriceProcessor _priceProcessor;
        private readonly IAgentContextService _agentContextService;
        private readonly ContractKindCommissionOptions _contractKindCommissionOptions;
    }
}