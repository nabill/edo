using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Discounts;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.Accommodations;
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
        
        
        public Task<RoomContractSetAvailability?> ApplyMarkups(RoomContractSetAvailability? response, AgentContext agent, Action<MarkupApplicationResult<RoomContractSetAvailability?>> logAction) 
            => _priceProcessor.ApplyMarkups(agent, response, ProcessPrices, logAction);
        
        
        public Task<RoomContractSetAvailability?> ApplyDiscounts(RoomContractSetAvailability? response, AgentContext agent, Action<DiscountApplicationResult<RoomContractSetAvailability?>> logAction) 
            => _priceProcessor.ApplyDiscounts(agent, response, ProcessPrices, logAction);

        
        public Task<Result<RoomContractSetAvailability?, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability? availabilityDetails, AgentContext agent) 
            => _priceProcessor.ConvertCurrencies(agent, availabilityDetails, ProcessPrices, GetCurrency);
        
        
        private static async ValueTask<RoomContractSetAvailability?> ProcessPrices(RoomContractSetAvailability? source,
            PriceProcessFunction processFunction)
        {
            if (source == null)
                return null;

            var value = source.Value;
            var roomContractSet = await RoomContractSetPriceProcessor.ProcessPrices(value.RoomContractSet, processFunction);
            return new RoomContractSetAvailability(value.AvailabilityId,
                value.CheckInDate,
                value.CheckOutDate,
                value.NumberOfNights,
                value.Accommodation,
                roomContractSet);
        }


        private static Currencies? GetCurrency(RoomContractSetAvailability? availabilityDetails) 
            => availabilityDetails?.RoomContractSet.Rate.Currency;

        
        private readonly IPriceProcessor _priceProcessor;
    }
}