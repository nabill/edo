using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public class AvailabilityMarkupService : IAvailabilityMarkupService
    {
        public AvailabilityMarkupService(IMarkupService markupService)
        {
            _markupService = markupService;
        }


        public async Task<AvailabilityDetailsWithMarkup> Apply(CustomerInfo customerInfo, AvailabilityDetails supplierResponse)
        {
            var markup = await _markupService.Get(customerInfo, AvailabilityPolicyTarget);
            var resultResponse = await ApplyMarkup(supplierResponse, markup.Function);
            return new AvailabilityDetailsWithMarkup(supplierResponse, markup.Policies, resultResponse);
        }


        private static async ValueTask<AvailabilityDetails> ApplyMarkup(AvailabilityDetails supplierResponse, AggregatedMarkupFunction aggregatedMarkupFunction)
        {
            var availabilityResults = new List<SlimAvailabilityResult>(supplierResponse.Results.Count);
            foreach (var availabilityResult in supplierResponse.Results)
            {
                var agreements = new List<Agreement>(availabilityResult.Agreements.Count);
                var currency = GetCurrency(agreements.FirstOrDefault().Price.CurrencyCode);
                foreach (var agreement in availabilityResult.Agreements)
                {
                    var rooms = new List<RoomDetails>(agreement.Rooms.Count);
                    foreach (var room in agreement.Rooms)
                    {
                        var roomPrices = new List<DailyPrice>(room.RoomPrices.Count);
                        foreach (var roomPrice in room.RoomPrices)
                        {
                            var roomGross = await aggregatedMarkupFunction(roomPrice.Gross, currency);
                            var roomNetTotal = await aggregatedMarkupFunction(roomPrice.NetTotal, currency);

                            roomPrices.Add(BuildDailyPrice(roomPrice, roomNetTotal, roomGross));
                        }

                        rooms.Add(BuildRoomDetails(room, roomPrices));
                    }

                    var agreementGross = await aggregatedMarkupFunction(agreement.Price.Gross, currency);
                    var agreementNetTotal = await aggregatedMarkupFunction(agreement.Price.NetTotal, currency);
                    var agreementPrice = new Price(agreement.Price.CurrencyCode, agreementNetTotal, agreementGross, agreement.Price.Type, agreement.Price.Description);

                    agreements.Add(BuildAgreement(agreement, agreementPrice, rooms));
                }

                availabilityResults.Add(new SlimAvailabilityResult(availabilityResult.AccommodationDetails, agreements));
            }

            return new AvailabilityDetails(supplierResponse.AvailabilityId, supplierResponse.NumberOfNights, supplierResponse.CheckInDate,
                supplierResponse.CheckOutDate, availabilityResults);


            Agreement BuildAgreement(in Agreement agreement, in Price agreementPrice, List<RoomDetails> rooms)
                => new Agreement(agreement.Id, agreement.TariffCode, agreement.BoardBasis, agreement.MealPlan, agreement.DeadlineDate,
                    agreement.ContractTypeId, agreement.IsAvailableImmediately, agreement.IsDynamic, agreement.IsSpecial, agreementPrice, rooms,
                    agreement.ContractType, agreement.Remarks);


            DailyPrice BuildDailyPrice(in DailyPrice roomPrice, decimal roomNetTotal, decimal roomGross) 
                => new DailyPrice(roomPrice.FromDate, roomPrice.ToDate, roomPrice.CurrencyCode, roomNetTotal, roomGross, roomPrice.Type, roomPrice.Description);


            RoomDetails BuildRoomDetails(in RoomDetails room, List<DailyPrice> roomPrices) 
                => new RoomDetails(roomPrices, room.AdultsNumber, room.ChildrenNumber, room.ChildrenAges, room.Type, room.IsExtraBedNeeded);
        }


        private static Currencies GetCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return Currencies.NotSpecified;

            return Enum.TryParse<Currencies>(currencyCode, out var currency) 
                ? currency 
                : Currencies.NotSpecified;
        }


        private readonly IMarkupService _markupService;
        private const MarkupPolicyTarget AvailabilityPolicyTarget = MarkupPolicyTarget.AccommodationAvailability;
    }
}