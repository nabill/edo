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
                    var roomPrices = new List<RoomPrice>();
                    foreach (var roomPrice in roomPrices)
                    {
                        roomPrices.Add(new RoomPrice(roomPrice,
                            await aggregatedMarkupFunction(roomPrice.Gross, currency),
                            await aggregatedMarkupFunction(roomPrice.Nett, currency)));
                    }

                    var agreementPrice = new AgreementPrice(await aggregatedMarkupFunction(agreement.Price.Gross, currency),
                        await aggregatedMarkupFunction(agreement.Price.Original, currency),
                        await aggregatedMarkupFunction(agreement.Price.Total, currency));

                    agreements.Add(new RichAgreement(agreement, agreementPrice, roomPrices));
                }

                availabilityResults.Add(new SlimAvailabilityResult(availabilityResult, agreements));
            }

            return new AvailabilityResponse(supplierResponse, availabilityResults);
        }

        private readonly IMarkupService _markupService;
        private const MarkupPolicyTarget AvailabilityPolicyTarget = MarkupPolicyTarget.AccommodationAvailability;
    }
}