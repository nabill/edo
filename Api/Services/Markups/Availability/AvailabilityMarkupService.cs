using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public class AvailabilityMarkupService : IAvailabilityMarkupService
    {
        public AvailabilityMarkupService(IMarkupService markupService)
        {
            _markupService = markupService;
        }

        public async Task<AvailabilityResponseWithMarkup> Apply(CustomerInfo customerInfo,
            AvailabilityResponse supplierResponse)
        {
            var markup = await _markupService.Get(customerInfo, AvailabilityPolicyTarget);
            var resultResponse = await ApplyMarkup(supplierResponse, markup.Function);
            return new AvailabilityResponseWithMarkup(supplierResponse, markup.Policies, resultResponse);
        }


        private Currencies GetCurrency(in SlimAvailabilityResult availabilityResult)
        {
            var currencyCode = availabilityResult.Agreements.FirstOrDefault()
                .CurrencyCode;

            if (currencyCode == default)
                return Currencies.NotSpecified;
            
            Enum.TryParse<Currencies>(currencyCode, out var currency);
            return currency;
        }


        private async ValueTask<AvailabilityResponse> ApplyMarkup(AvailabilityResponse supplierResponse, AggregatedMarkupFunction aggregatedMarkupFunction)
        {
            var availabilityResults = new List<SlimAvailabilityResult>(supplierResponse.Results.Count);
            foreach (var availabilityResult in supplierResponse.Results)
            {
                var currency = GetCurrency(availabilityResult);
                var agreements = new List<RichAgreement>(availabilityResult.Agreements.Count);
                foreach (var agreement in availabilityResult.Agreements)
                {
                    var rooms = new List<RoomDetails>(agreement.Rooms.Count);
                    foreach (var room in agreement.Rooms)
                    {
                        var prices = new List<RoomPrice>(room.RoomPrices.Count);
                        foreach (var price in room.RoomPrices)
                        {
                            prices.Add(new RoomPrice(price,
                                await aggregatedMarkupFunction(price.Gross, currency),
                                await aggregatedMarkupFunction(price.Nett, currency)));
                        }

                        rooms.Add(new RoomDetails(room, prices));
                    }

                    var agreementPrice = new AgreementPrice(await aggregatedMarkupFunction(agreement.Price.Gross, currency),
                        await aggregatedMarkupFunction(agreement.Price.Original, currency),
                        await aggregatedMarkupFunction(agreement.Price.Total, currency));

                    agreements.Add(new RichAgreement(agreement, agreementPrice, rooms));
                }

                availabilityResults.Add(new SlimAvailabilityResult(availabilityResult, agreements));
            }

            return new AvailabilityResponse(supplierResponse, availabilityResults);
        }

        private readonly IMarkupService _markupService;
        private const MarkupPolicyTarget AvailabilityPolicyTarget = MarkupPolicyTarget.AccommodationAvailability;
    }
}