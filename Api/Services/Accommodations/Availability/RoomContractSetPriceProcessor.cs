using System;
using System.Collections.Generic;
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
            var roomContractSetNetTotal = await priceProcessFunction(sourceRoomContractSet.Rate.FinalPrice);
            var ratio = GetRatio(sourceRoomContractSet.Rate.FinalPrice.Amount, roomContractSetNetTotal.Amount);
            var roomContractSetGross = ApplyRatio(sourceRoomContractSet.Rate.Gross, ratio);
            var roomContractSetRate = new Rate(roomContractSetNetTotal, roomContractSetGross, sourceRoomContractSet.Rate.Discounts,
                sourceRoomContractSet.Rate.Type, sourceRoomContractSet.Rate.Description);
            
            
            foreach (var room in sourceRoomContractSet.RoomContracts)
            {
                var dailyRates = new List<DailyRate>(room.DailyRoomRates.Count);
                foreach (var dailyRate in room.DailyRoomRates)
                {
                    var roomGross = ApplyRatio(dailyRate.Gross, ratio);
                    var roomFinalPrice = ApplyRatio(dailyRate.FinalPrice, ratio);

                    dailyRates.Add(BuildDailyPrice(dailyRate, roomFinalPrice, roomGross));
                }
                
                var totalPriceNet = ApplyRatio(room.Rate.FinalPrice, ratio);
                var totalPriceGross = ApplyRatio(room.Rate.Gross, ratio);
                var totalRate = new Rate(totalPriceNet, totalPriceGross);

                roomContracts.Add(BuildRoomContracts(room, dailyRates, totalRate));
            }

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
                    isDirectContract: roomContractSet.IsDirectContract,
                    isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate);


            static decimal GetRatio(decimal x, decimal y)
            {
                if (x == 0 || y == 0)
                    throw new NotSupportedException($"Cannot get ratio between {x} and {y}");
                
                return x / y;
            }


            static MoneyAmount ApplyRatio(MoneyAmount amount, decimal ratio)
                => MoneyRounder.Ceil((amount.Amount / ratio).ToMoneyAmount(amount.Currency));
        }
    }
}