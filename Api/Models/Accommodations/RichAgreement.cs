using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RichAgreement
    {
        [JsonConstructor]
        public RichAgreement(Guid id, string tariffCode, string mealPlanCode, string boardBasisCode, string currencyCode, string mealPlan, string roomPlan,
            DateTime deadlineDate, int contractTypeId, bool isAvailableImmediately, bool isDynamic, bool isSpecial, AgreementPrice price,
            List<RoomPrice> roomPrices, List<RoomDetails> rooms, string contractType, Dictionary<string, string> remarks)
        {
            Id = id;
            TariffCode = tariffCode;
            MealPlan = mealPlan;
            MealPlanCode = mealPlanCode;
            BoardBasis = roomPlan;
            BoardBasisCode = boardBasisCode;
            CurrencyCode = currencyCode;
            DeadlineDate = deadlineDate;
            ContractType = contractType;
            ContractTypeId = contractTypeId;
            IsAvailableImmediately = isAvailableImmediately;
            IsDynamic = isDynamic;
            IsSpecial = isSpecial;
            Price = price;
            Remarks = remarks;
            RoomPrices = roomPrices;
            Rooms = rooms;
        }


        public Guid Id { get; }
        public string BoardBasis { get; }
        public string BoardBasisCode { get; }
        public string ContractType { get; }
        public int ContractTypeId { get; }
        public string MealPlan { get; }
        public string MealPlanCode { get; }
        public string CurrencyCode { get; }
        public DateTime DeadlineDate { get; }
        public bool IsAvailableImmediately { get; }
        public bool IsDynamic { get; }
        public bool IsSpecial { get; }
        public AgreementPrice Price { get; }
        public List<RoomPrice> RoomPrices { get; }
        public Dictionary<string, string> Remarks { get; }
        public List<RoomDetails> Rooms { get; }
        public string TariffCode { get; }
    }
}
