using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public record RoomContract
    {
        [JsonConstructor]
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


        /// <summary>
        ///     The board basis of a contract.
        /// </summary>
        public BoardBasisTypes BoardBasis { get; init; }

        /// <summary>
        ///     The textual description of a board basis.
        /// </summary>
        public string MealPlan { get; init; }

        /// <summary>
        ///     The numerical code of a contract type.
        /// </summary>
        public int ContractTypeCode { get; init; }

        public bool IsAvailableImmediately { get; init; }

        /// <summary>
        ///     Indicates if a contract a dynamic offer.
        /// </summary>
        public bool IsDynamic { get; init; }

        /// <summary>
        ///     The textual contract description i.e. "Pool View Suite", "Ocean Club Room", or "Pioneer Cabin".
        /// </summary>
        public string ContractDescription { get; init; }

        /// <summary>
        ///     The total contract price.
        /// </summary>
        public Rate Rate { get; init; }

        /// <summary>
        ///     Contract remarks.
        /// </summary>
        public List<KeyValuePair<string, string>> Remarks { get; init; }

        /// <summary>
        ///     Required. Number of adult passengers.
        /// </summary>
        [Required]
        public int AdultsNumber { get; init; }

        /// <summary>
        ///     Ages of each child.
        /// </summary>
        public List<int> ChildrenAges { get; init; }

        /// <summary>
        ///     Indicates if extra child bed needed.
        /// </summary>
        public bool IsExtraBedNeeded { get; init; }

        /// <summary>
        ///     Deadline and cancellation information.
        ///     <b>Null considers as an unknown deadline for first search steps, and as an empty deadline for the evaluation step.</b>
        /// </summary>
        public Deadline Deadline { get; init; }

        /// <summary>
        ///     Indicates if a contract is an advance purchase.
        /// </summary>
        public bool IsAdvancePurchaseRate { get; init; }

        /// <summary>
        ///     List of room prices on daily basis
        /// </summary>
        public List<DailyRate> DailyRoomRates { get; init; }

        /// <summary>
        ///     Desirable room type.
        /// </summary>
        public RoomTypes Type { get; init; }
    }
}