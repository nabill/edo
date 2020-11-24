using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContract
    {
        public RoomContract(BoardBasisTypes boardBasis, string mealPlan, int contractTypeCode, bool isAvailableImmediately,
            bool isDynamic, string contractDescription, List<KeyValuePair<string, string>> remarks, List<DailyRate> dailyRoomRates,
            Rate rate, int adultsNumber, List<int> childrenAges, RoomTypes type, bool isExtraBedNeeded, Deadline deadline,
            bool isAdvancePurchaseRate)
        {
            BoardBasis = boardBasis;
            MealPlan = mealPlan;
            ContractTypeCode = contractTypeCode;
            IsAvailableImmediately = isAvailableImmediately;
            IsDynamic = isDynamic;
            ContractDescription = contractDescription;
            Rate = rate;
            Remarks = remarks;
            AdultsNumber = adultsNumber;
            ChildrenAges = childrenAges;
            IsExtraBedNeeded = isExtraBedNeeded;
            Deadline = deadline;
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            DailyRoomRates = dailyRoomRates;
            Type = type;
        }


        public BoardBasisTypes BoardBasis { get; }

        public string MealPlan { get; }

        public int ContractTypeCode { get; }

        public bool IsAvailableImmediately { get; }

        public bool IsDynamic { get; }

        public string ContractDescription { get; }

        public Rate Rate { get; }

        public List<KeyValuePair<string, string>> Remarks { get; }

        [Required]
        public int AdultsNumber { get; }

        public List<int> ChildrenAges { get; }

        public bool IsExtraBedNeeded { get; }

        public Deadline Deadline { get; }

        public bool IsAdvancePurchaseRate { get; }

        public List<DailyRate> DailyRoomRates { get; }

        public RoomTypes Type { get; }
    }
}