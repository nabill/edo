using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Helpers;
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
            var sourceTotalPrice = sourceRoomContractSet.Rate.FinalPrice;
            var processedTotalPrice = await priceProcessFunction(sourceRoomContractSet.Rate.FinalPrice);
            
            var roomContractSetGross = ChangeProportionally(sourceRoomContractSet.Rate.Gross);
            var roomContractSetRate = new Rate(processedTotalPrice, roomContractSetGross, sourceRoomContractSet.Rate.Discounts,
                sourceRoomContractSet.Rate.Type, sourceRoomContractSet.Rate.Description);
            
            foreach (var room in sourceRoomContractSet.RoomContracts)
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
                if (price.Amount == 0)
                    throw new NotSupportedException($"Cannot get ratio for {price.Amount}");
                
                var totalPricePercent = price.Amount / sourceTotalPrice.Amount;
                return new MoneyAmount(processedTotalPrice.Amount * totalPricePercent, processedTotalPrice.Currency);
            }
        }


        public static async ValueTask<RoomContractSet> AlignPrices(RoomContractSet roomContractSet)
        {
            var ceiledRoomContractSet = await ProcessPrices(roomContractSet, price 
                => new ValueTask<MoneyAmount>(MoneyRounder.Ceil(price)));
            
            var currency = roomContractSet.Rate.Currency;
            var priceChangeStep = 1 / currency.GetDecimalDigitsCount();

            var finalRate = ceiledRoomContractSet.Rate.FinalPrice.Amount;
            var roomFinalRates = ceiledRoomContractSet.RoomContracts.Select(r => r.Rate.FinalPrice.Amount).ToList();
            var (alignedFinalPrice, alignedRoomFinalPrices) = AlignAggregateValues(finalRate, roomFinalRates, priceChangeStep);

            var grossRate = ceiledRoomContractSet.Rate.Gross.Amount;
            var grossRateValues = ceiledRoomContractSet.RoomContracts.Select(r => r.Rate.Gross.Amount).ToList();
            var (alignedGrossPrice, alignedRoomGrossRates) = AlignAggregateValues(grossRate, grossRateValues, priceChangeStep);

            var roomContracts = new List<RoomContract>(roomContractSet.RoomContracts.Count);
            for (var i = 0; i < ceiledRoomContractSet.RoomContracts.Count; i++)
            {
                var room = ceiledRoomContractSet.RoomContracts[i];
                var totalPriceNet = new MoneyAmount(alignedRoomFinalPrices[i], room.Rate.Currency);
                var totalPriceGross = new MoneyAmount(alignedRoomGrossRates[i], room.Rate.Currency);
                var totalRate = new Rate(totalPriceNet, totalPriceGross);
                
                roomContracts.Add(BuildRoomContracts(room, room.DailyRoomRates, totalRate));
            }

            var roomContractSetRate = new Rate(new MoneyAmount(alignedFinalPrice, currency),
                new MoneyAmount(alignedGrossPrice, currency));

            return BuildRoomContractSet(roomContractSet, roomContractSetRate, roomContracts);


            static (decimal Aggregated, List<decimal> Parts) AlignAggregateValues(decimal aggregated, List<decimal> parts, decimal changeStep)
            {
                var partsSum = parts.Sum();
                return aggregated switch
                {
                    _ when aggregated == partsSum => (aggregated, parts),
                    _ when aggregated < partsSum => (partsSum, parts),
                    _ when aggregated > partsSum => Align(aggregated, parts, changeStep),
                    _ => throw new ArgumentOutOfRangeException(nameof(aggregated), aggregated, null)
                };


                static (decimal Aggregated, List<decimal> Parts) Align(decimal aggregated, List<decimal> parts, decimal changeStep)
                {
                    while (parts.Sum() < aggregated)
                        parts = parts.Select(p => p + changeStep).ToList();

                    return (parts.Sum(), parts);
                }
            }
        }
        
        
        private static DailyRate BuildDailyPrice(in DailyRate dailyRate, MoneyAmount roomNetTotal, MoneyAmount roomGross)
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
                isDirectContract: roomContractSet.IsDirectContract,
                isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate);
    }
}