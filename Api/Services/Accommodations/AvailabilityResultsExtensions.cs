using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public static class AvailabilityResultsExtensions
    {
        public static async ValueTask<CombinedAvailabilityDetails> ProcessPrices(this CombinedAvailabilityDetails source,
            PriceProcessFunction processFunction)
        {
            var resultsWithMarkup = new List<ProviderData<AvailabilityResult>>(source.Results.Count);
            foreach (var supplierResponse in source.Results)
            {
                var supplierAgreements = supplierResponse.Data.Agreements;
                var agreementsWithMarkup = await ProcessAgreementsPrices(supplierAgreements, processFunction);
                var responseWithMarkup = ProviderData.Create(supplierResponse.Source,
                    new AvailabilityResult(supplierResponse.Data, agreementsWithMarkup));

                resultsWithMarkup.Add(responseWithMarkup);
            }

            return new CombinedAvailabilityDetails(source, resultsWithMarkup);
        }


        public static async ValueTask<SingleAccommodationAvailabilityDetails> ProcessPrices(this SingleAccommodationAvailabilityDetails source,
            PriceProcessFunction processFunction)
        {
            var agreements = await ProcessAgreementsPrices(source.Agreements, processFunction);
            return new SingleAccommodationAvailabilityDetails(source.AvailabilityId,
                source.CheckInDate,
                source.CheckOutDate,
                source.NumberOfNights,
                source.AccommodationDetails,
                agreements);
        }


        public static async ValueTask<SingleAccommodationAvailabilityDetailsWithDeadline> ProcessPrices(
            this SingleAccommodationAvailabilityDetailsWithDeadline source,
            PriceProcessFunction processFunction)
        {
            var agreement = await ProcessAgreementPrice(source.Agreement, processFunction);
            return new SingleAccommodationAvailabilityDetailsWithDeadline(source.AvailabilityId,
                source.CheckInDate,
                source.CheckOutDate,
                source.NumberOfNights,
                source.AccommodationDetails,
                agreement,
                source.DeadlineDetails);
        }


        private static async Task<List<Agreement>> ProcessAgreementsPrices(List<Agreement> sourceAgreements, PriceProcessFunction priceProcessFunction)
        {
            var agreements = new List<Agreement>(sourceAgreements.Count);
            foreach (var agreement in sourceAgreements)
            {
                var agreementWithMarkup = await ProcessAgreementPrice(agreement, priceProcessFunction);
                agreements.Add(agreementWithMarkup);
            }

            return agreements;
        }


        private static async Task<Agreement> ProcessAgreementPrice(Agreement sourceAgreement, PriceProcessFunction priceProcessFunction)
        {
            var currency = sourceAgreement.Price.Currency;

            var rooms = new List<RoomDetails>(sourceAgreement.Rooms.Count);
            foreach (var room in sourceAgreement.Rooms)
            {
                var roomPrices = new List<DailyPrice>(room.RoomPrices.Count);
                foreach (var roomPrice in room.RoomPrices)
                {
                    var (roomGross, roomCurrency) = await priceProcessFunction(roomPrice.Gross, currency);
                    var (roomNetTotal, _) = await priceProcessFunction(roomPrice.NetTotal, currency);

                    roomPrices.Add(BuildDailyPrice(roomPrice, roomNetTotal, roomGross, roomCurrency));
                }

                rooms.Add(BuildRoomDetails(room, roomPrices));
            }

            var (agreementGross, agreementCurrency) = await priceProcessFunction(sourceAgreement.Price.Gross, currency);
            var (agreementNetTotal, _) = await priceProcessFunction(sourceAgreement.Price.NetTotal, currency);
            var agreementPrice = new Price(agreementCurrency, agreementNetTotal, agreementGross, sourceAgreement.Price.Type,
                sourceAgreement.Price.Description);

            return BuildAgreement(sourceAgreement, agreementPrice, rooms);


            static DailyPrice BuildDailyPrice(in DailyPrice roomPrice, decimal roomNetTotal, decimal roomGross, Currencies roomCurrency)
                => new DailyPrice(roomPrice.FromDate, roomPrice.ToDate, roomCurrency, roomNetTotal, roomGross, roomPrice.Type, roomPrice.Description);


            static RoomDetails BuildRoomDetails(in RoomDetails room, List<DailyPrice> roomPrices)
                => new RoomDetails(roomPrices, room.AdultsNumber, room.ChildrenNumber, room.ChildrenAges, room.Type, room.IsExtraBedNeeded);


            static Agreement BuildAgreement(in Agreement agreement, in Price agreementPrice, List<RoomDetails> rooms)
                => new Agreement(agreement.Id, agreement.TariffCode, agreement.BoardBasisCode, agreement.BoardBasis, agreement.MealPlanCode, agreement.MealPlan,
                    agreement.DeadlineDate,
                    agreement.ContractTypeId, agreement.IsAvailableImmediately, agreement.IsDynamic, agreement.IsSpecial, agreementPrice, rooms,
                    agreement.ContractType, agreement.Remarks);
        }


        public static Currencies GetCurrency(this CombinedAvailabilityDetails availabilityDetails)
        {
            return availabilityDetails.Results
                .SelectMany(r => r.Data.Agreements)
                .Select(a => a.Price.Currency)
                .FirstOrDefault();
        }
        
        public static Currencies GetCurrency(this SingleAccommodationAvailabilityDetails availabilityDetails)
        {
            return availabilityDetails.Agreements
                .Select(a => a.Price.Currency)
                .FirstOrDefault();
        }
        
        public static Currencies GetCurrency(this SingleAccommodationAvailabilityDetailsWithDeadline availabilityDetails)
        {
            return availabilityDetails.Agreement.Price.Currency;
        }
    }
}