using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using DailyRate = HappyTravel.Edo.Api.Models.Accommodations.DailyRate;

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
                    var roomGross = MoneyRounder.Ceil(ChangeProportionally(dailyRate.Gross));
                    var roomTotalPrice = MoneyRounder.Ceil(ChangeProportionally(dailyRate.TotalPrice));

                    dailyRates.Add(BuildDailyPrice(dailyRate, roomTotalPrice, roomGross));
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


        public static List<RoomContractSet> AlignPrices(List<RoomContractSet> sourceRoomContractSets,
            ContractKind? contractKind, ContractKindCommissionOptions contractKindCommissionOptions)
        {
            var roomContractSets = new List<RoomContractSet>(sourceRoomContractSets.Count);
            foreach (var roomContractSet in sourceRoomContractSets)
            {
                var roomContractSetWithMarkup = AlignPrices(roomContractSet, contractKind, contractKindCommissionOptions);
                roomContractSets.Add(roomContractSetWithMarkup);
            }

            return roomContractSets;
        }


        public static RoomContractSet AlignPrices(RoomContractSet roomContractSet,
            ContractKind? contractKind, ContractKindCommissionOptions contractKindCommissionOptions)
        {
            var commission = 0m;

            switch (contractKind)
            {
                case ContractKind.CreditCardPayments:
                    commission = contractKindCommissionOptions.CreditCardPaymentsCommission;
                    break;
            }

            var totalFinalPrice = MoneyRounder.Ceil(roomContractSet.Rate.FinalPrice.ApplyCommission(commission));
            var roomFinalPrices = roomContractSet.Rooms
                .Select(r => MoneyRounder.Ceil(r.Rate.FinalPrice.ApplyCommission(commission)))
                .ToList();

            var (alignedFinalPrice, alignedRoomFinalPrices) = PriceAligner.AlignAggregatedValues(totalFinalPrice, roomFinalPrices);

            var totalGrossPrice = MoneyRounder.Ceil(roomContractSet.Rate.Gross);
            var roomGrossPrices = roomContractSet.Rooms
                .Select(r => MoneyRounder.Ceil(r.Rate.Gross))
                .ToList();

            var (alignedGrossPrice, alignedRoomGrossRates) = PriceAligner.AlignAggregatedValues(totalGrossPrice, roomGrossPrices);

            var totalCreditCardPrice = MoneyRounder.Ceil(roomContractSet.Rate.FinalPrice.ApplyCommission(contractKindCommissionOptions.CreditCardPaymentsCommission));
            var roomCreditCardPrices = roomContractSet.Rooms
                .Select(r => MoneyRounder.Ceil(r.Rate.FinalPrice.ApplyCommission(contractKindCommissionOptions.CreditCardPaymentsCommission)))
                .ToList();

            var (alignedCreditCardPrice, alignedCreditCardPrices) = PriceAligner.AlignAggregatedValues(totalCreditCardPrice, roomCreditCardPrices);

            var roomContracts = new List<RoomContract>(roomContractSet.Rooms.Count);
            for (var i = 0; i < roomContractSet.Rooms.Count; i++)
            {

                var room = roomContractSet.Rooms[i];
                var totalPriceNet = alignedRoomFinalPrices[i];
                var totalPriceGross = alignedRoomGrossRates[i];
                var totalCreditCardPriceNet = alignedCreditCardPrices[i];
                var totalRate = new Rate(totalPriceNet, totalPriceGross, commission: commission,
                    netPrice: room.Rate.FinalPrice, creditCardPrice: totalCreditCardPriceNet);

                roomContracts.Add(BuildRoomContracts(room, room.DailyRoomRates, totalRate));
            }

            var roomContractSetRate = new Rate(alignedFinalPrice, alignedGrossPrice,
                commission: commission,
                netPrice: roomContractSet.Rate.FinalPrice,
                creditCardPrice: alignedCreditCardPrice);

            return BuildRoomContractSet(roomContractSet, roomContractSetRate, roomContracts);
        }


        private static DailyRate BuildDailyPrice(in DailyRate dailyRate, MoneyAmount roomFinalPrice, MoneyAmount roomGross)
            => new(dailyRate.FromDate, dailyRate.ToDate, roomFinalPrice, roomGross, dailyRate.Type, dailyRate.Description);


        static RoomContract BuildRoomContracts(in RoomContract room, List<DailyRate> roomPrices, Rate totalPrice)
            => new(boardBasis: room.BoardBasis,
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
            => new(id: roomContractSet.Id,
                rate: roomContractSetRate,
                deadline: roomContractSet.Deadline,
                rooms: rooms,
                isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate,
                supplier: roomContractSet.Supplier,
                supplierCode: roomContractSet.SupplierCode,
                tags: roomContractSet.Tags, isDirectContract: roomContractSet.IsDirectContract, isPackageRate: roomContractSet.IsPackageRate);
    }
}