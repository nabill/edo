using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class RoomContractSetPriceProcessor
    {
        public static async Task<List<RoomContractSet>> ProcessPrices(List<RoomContractSet> sourceRoomContractSets, PriceProcessFunction priceProcessFunction)
        {
            var roomContractSets = new List<RoomContractSet>(sourceRoomContractSets.Count);
            foreach (var roomContractSet in sourceRoomContractSets)
            {
                var roomContractSetWithMarkup = await ProcessPrices(roomContractSet, priceProcessFunction);
                roomContractSets.Add(roomContractSetWithMarkup);
            }

            return roomContractSets;
        }


        public static async Task<RoomContractSet> ProcessPrices(RoomContractSet sourceRoomContractSet, PriceProcessFunction priceProcessFunction)
        {
            var roomContracts = new List<RoomContract>(sourceRoomContractSet.RoomContracts.Count);
            foreach (var room in sourceRoomContractSet.RoomContracts)
            {
                var dailyRates = new List<DailyRate>(room.DailyRoomRates.Count);
                foreach (var dailyRate in room.DailyRoomRates)
                {
                    var roomGross = await priceProcessFunction(dailyRate.Gross);
                    var roomFinalPrice = await priceProcessFunction(dailyRate.FinalPrice);

                    dailyRates.Add(BuildDailyPrice(dailyRate, roomFinalPrice, roomGross));
                }

                var totalPriceNet = await priceProcessFunction(room.Rate.FinalPrice);
                var totalPriceGross = await priceProcessFunction(room.Rate.FinalPrice);
                var totalRate = new Rate(totalPriceNet, totalPriceGross);

                roomContracts.Add(BuildRoomContracts(room, dailyRates, totalRate));
            }

            var roomContractSetGross = await priceProcessFunction(sourceRoomContractSet.Rate.Gross);
            var roomContractSetNetTotal = await priceProcessFunction(sourceRoomContractSet.Rate.FinalPrice);
            var roomContractSetRate = new Rate(roomContractSetNetTotal, roomContractSetGross, sourceRoomContractSet.Rate.Discounts,
                sourceRoomContractSet.Rate.Type, sourceRoomContractSet.Rate.Description);

            return BuildRoomContractSet(sourceRoomContractSet, roomContractSetRate, roomContracts);


            static DailyRate BuildDailyPrice(in DailyRate dailyRate, MoneyAmount roomNetTotal, MoneyAmount roomGross)
                => new DailyRate(dailyRate.FromDate, dailyRate.ToDate, roomNetTotal, roomGross, dailyRate.Type, dailyRate.Description);


            static RoomContract BuildRoomContracts(in RoomContract room, List<DailyRate> roomPrices, Rate totalPrice)
                => new RoomContract(room.BoardBasis, 
                    room.MealPlan, 
                    room.ContractTypeCode,
                    room.IsAvailableImmediately,
                    room.IsDynamic,
                    room.ContractDescription,
                    room.Remarks,
                    roomPrices, 
                    totalPrice,
                    room.AdultsNumber, 
                    room.ChildrenAges,
                    room.Type,
                    room.IsExtraBedNeeded,
                    room.Deadline,
                    room.IsAdvancePurchaseRate);

            static RoomContractSet BuildRoomContractSet(in RoomContractSet roomContractSet, in Rate roomContractSetRate, List<RoomContract> rooms)
                => new RoomContractSet(roomContractSet.Id, 
                    roomContractSetRate, 
                    roomContractSet.Deadline, 
                    rooms, 
                    roomContractSet.Tags,
                    roomContractSet.IsAdvancePurchaseRate);
        }
    }
}