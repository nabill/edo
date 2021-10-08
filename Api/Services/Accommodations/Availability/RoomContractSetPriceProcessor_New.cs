using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using DailyRate = HappyTravel.Edo.Api.Models.Accommodations.DailyRate;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    // Temporal class to replace existing RoomContractSetPriceProcessor in further PRs
    public static class RoomContractSetPriceProcessor_New
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
            var roomContracts = new List<RoomContract>(sourceRoomContractSet.Rooms.Count);
            var sourceTotalPrice = sourceRoomContractSet.Rate.FinalPrice;
            if (sourceTotalPrice.Amount == 0)
                throw new NotSupportedException("Room contract set price cannot be 0");
            
            var processedTotalPrice = await priceProcessFunction(sourceRoomContractSet.Rate.FinalPrice);
            
            var roomContractSetGross = ChangeProportionally(sourceRoomContractSet.Rate.Gross);
            var roomContractSetRate = new Rate(finalPrice: processedTotalPrice, 
                gross: roomContractSetGross,
                discounts: sourceRoomContractSet.Rate.Discounts,
                type: sourceRoomContractSet.Rate.Type,
                description: sourceRoomContractSet.Rate.Description);
            
            foreach (var room in sourceRoomContractSet.Rooms)
            {
                var dailyRates = new List<DailyRate>(room.DailyRoomRates.Count);
                foreach (var dailyRate in room.DailyRoomRates)
                {
                    var roomGross = ChangeProportionally(dailyRate.Gross);
                    var roomFinalPrice = ChangeProportionally(dailyRate.FinalPrice);

                    dailyRates.Add(BuildDailyPrice(dailyRate, roomFinalPrice, roomGross));
                }
                
                var totalPriceNet = ChangeProportionally(room.Rate.FinalPrice);
                var totalPriceGross = ChangeProportionally(room.Rate.Gross);
                var totalRate = new Rate(totalPriceNet, totalPriceGross);

                roomContracts.Add(BuildRoomContracts(room, dailyRates, totalRate));
            }

            return BuildRoomContractSet(sourceRoomContractSet, roomContractSetRate, roomContracts);


            MoneyAmount ChangeProportionally(MoneyAmount price)
            {
                var ratio = (price / sourceTotalPrice).Amount;
                return new MoneyAmount(processedTotalPrice.Amount * ratio, processedTotalPrice.Currency);
            }
        }
        
        
        public static async ValueTask<List<RoomContractSet>> AlignPrices(List<RoomContractSet> sourceRoomContractSets)
        {
            var roomContractSets = new List<RoomContractSet>(sourceRoomContractSets.Count);
            foreach (var roomContractSet in sourceRoomContractSets)
            {
                var roomContractSetWithMarkup = await AlignPrices(roomContractSet);
                roomContractSets.Add(roomContractSetWithMarkup);
            }

            return roomContractSets;
        }


        public static async ValueTask<RoomContractSet> AlignPrices(RoomContractSet roomContractSet)
        {
            var ceiledRoomContractSet = await ProcessPrices(roomContractSet, price 
                => new ValueTask<MoneyAmount>(MoneyRounder.Ceil(price)));
            
            var finalPrice = ceiledRoomContractSet.Rate.FinalPrice;
            var roomFinalRates = ceiledRoomContractSet.Rooms.Select(r => r.Rate.FinalPrice).ToList();
            var (alignedFinalPrice, alignedRoomFinalPrices) = PriceAligner.AlignAggregateValues(finalPrice, roomFinalRates);

            var gross = ceiledRoomContractSet.Rate.Gross;
            var roomGrossRateRates = ceiledRoomContractSet.Rooms.Select(r => r.Rate.Gross).ToList();
            var (alignedGrossPrice, alignedRoomGrossRates) = PriceAligner.AlignAggregateValues(gross, roomGrossRateRates);

            var roomContracts = new List<RoomContract>(roomContractSet.Rooms.Count);
            for (var i = 0; i < ceiledRoomContractSet.Rooms.Count; i++)
            {
                var room = ceiledRoomContractSet.Rooms[i];
                var totalPriceNet = alignedRoomFinalPrices[i];
                var totalPriceGross = alignedRoomGrossRates[i];
                var totalRate = new Rate(totalPriceNet, totalPriceGross);
                
                roomContracts.Add(BuildRoomContracts(room, room.DailyRoomRates, totalRate));
            }

            var roomContractSetRate = new Rate(alignedFinalPrice, alignedGrossPrice);

            return BuildRoomContractSet(roomContractSet, roomContractSetRate, roomContracts);
        }
        
        
        private static DailyRate BuildDailyPrice(in DailyRate dailyRate, MoneyAmount roomFinalPrice, MoneyAmount roomGross)
            => new (dailyRate.FromDate, dailyRate.ToDate, roomFinalPrice, roomGross, dailyRate.Type, dailyRate.Description);
        
        
        static RoomContract BuildRoomContracts(in RoomContract room, List<DailyRate> roomPrices, Rate totalPrice)
            => new (boardBasis: room.BoardBasis, 
                mealPlan: room.MealPlan, 
                contractTypeCode: room.ContractTypeCode,
                isAvailableImmediately: room.IsAvailableImmediately,
                isDynamic: room.IsDynamic,
                contractDescription: room.ContractDescription,
                remarks: room.Remarks,
                dailyRoomRates: roomPrices, 
                rate: totalPrice,
                adultsNumber: room.AdultsNumber, 
                childrenAges: room.ChildrenAges,
                type: room.Type,
                isExtraBedNeeded: room.IsExtraBedNeeded,
                deadline: room.Deadline,
                isAdvancePurchaseRate: room.IsAdvancePurchaseRate);

            
        static RoomContractSet BuildRoomContractSet(in RoomContractSet roomContractSet, in Rate roomContractSetRate, List<RoomContract> rooms)
            => new (id: roomContractSet.Id,
                isDirectContract: roomContractSet.IsDirectContract,
                rate: roomContractSetRate,
                rooms: rooms,
                isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate,
                deadline: roomContractSet.Deadline,
                supplier: roomContractSet.Supplier,
                tags: roomContractSet.Tags,
                isPackageRate: roomContractSet.IsPackageRate);
    }
}