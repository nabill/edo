using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationPriceProcessor : IBookingEvaluationPriceProcessor
    {
        public BookingEvaluationPriceProcessor(IPriceProcessor priceProcessor)
        {
            _priceProcessor = priceProcessor;
        }
        
        
        public Task<RoomContractSetAvailability> ApplyMarkups(RoomContractSetAvailability response, AgentContext agent, Action<MarkupApplicationResult<RoomContractSetAvailability>> logAction) 
            => _priceProcessor.ApplyMarkups(agent, response, ProcessPrices, null, logAction);

        
        public Task<Result<RoomContractSetAvailability, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability availabilityDetails, AgentContext agent) 
            => _priceProcessor.ConvertCurrencies(agent, availabilityDetails, ProcessPrices, GetCurrency);


        public RoomContractSetAvailability AlignPrices(RoomContractSetAvailability value)
        {
            var roomContractSet = RoomContractSetPriceProcessor.AlignPrices(value.RoomContractSet);
            return new RoomContractSetAvailability(availabilityId: value.AvailabilityId,
                checkInDate: value.CheckInDate,
                checkOutDate: value.CheckOutDate,
                numberOfNights: value.NumberOfNights,
                roomContractSet: roomContractSet,
                accommodation: value.Accommodation,
                availablePaymentMethods: value.AvailablePaymentMethods,
                countryHtId: value.CountryHtId,
                localityHtId: value.LocalityHtId);
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
                localityHtId: value.LocalityHtId);
        }


        private static Currencies? GetCurrency(RoomContractSetAvailability availabilityDetails)
            => availabilityDetails.RoomContractSet.Rate.Currency == Currencies.NotSpecified
                ? null
                : availabilityDetails.RoomContractSet.Rate.Currency;

        
        private readonly IPriceProcessor _priceProcessor;
    }
}