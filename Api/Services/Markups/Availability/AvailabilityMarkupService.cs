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
            var markup = await _markupService.GetMarkup(customerInfo, AvailabilityPolicyTarget);
            var resultResponse = ApplyMarkup(supplierResponse, markup.Function);
            return new AvailabilityResponseWithMarkup(supplierResponse, markup.Policies, resultResponse);
        }


        private AvailabilityResponse ApplyMarkup(AvailabilityResponse supplierResponse, MarkupFunction markupFunction)
        {
            var availabilityResults = new List<SlimAvailabilityResult>();
            var currencyCode = supplierResponse.Results
                .SingleOrDefault()
                .Agreements.SingleOrDefault()
                .CurrencyCode;
            
            Enum.TryParse<Currencies>(currencyCode, out var currency);
            
            foreach (var availabilityResult in supplierResponse.Results)
            {
                var agreements = new List<RichAgreement>(availabilityResult.Agreements.Count);
                foreach (var agreement in availabilityResult.Agreements)
                {
                    
                    var roomPrices = new List<RoomPrice>();
                    foreach (var roomPrice in roomPrices)
                    {
                        roomPrices.Add(new RoomPrice(roomPrice,
                            markupFunction(roomPrice.Gross),
                            markupFunction(roomPrice.Nett)));
                    }

                    var agreementPrice = new AgreementPrice(markupFunction(agreement.Price.Gross),
                        markupFunction(agreement.Price.Original),
                        markupFunction(agreement.Price.Total));

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