using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public record RoomContract
    {
        [JsonConstructor]
        public RoomContract(BoardBasisTypes boardBasis, string mealPlan, string contractTypeCode, bool isAvailableImmediately,
            bool isDynamic, string contractDescription, List<KeyValuePair<string, string>> remarks, List<DailyRate> dailyRoomRates,
            Rate rate, int adultsNumber, int childrenNumber, RoomTypes type, Deadline deadline,
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
            ChildrenNumber = childrenNumber;
            Deadline = deadline;
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            DailyRoomRates = dailyRoomRates;
            Type = type;
        }


        /// <summary>
        ///     Meals included in the contract
        /// </summary>
        public BoardBasisTypes BoardBasis { get; }

        /// <summary>
        ///     Description of the board basis (included meals)
        /// </summary>
        public string MealPlan { get; }

        // TODO: check naming and meaning
        /// <summary>
        ///     Code for the contract type
        /// </summary>
        public string ContractTypeCode { get; }

        /// <summary>
        ///     Flag indicates if the contract can be booked immediately. 'FALSE' means what the contract is available on request.
        /// </summary>
        public bool IsAvailableImmediately { get; }

        /// <summary>
        ///     Indicates if the contract is a dynamic offer
        /// </summary>
        public bool IsDynamic { get; }

        /// <summary>
        ///     Description for the contract, such as "Pool View Suite", "Ocean Club Room", or "Pioneer Cabin"
        /// </summary>
        public string ContractDescription { get; }

        /// <summary>
        ///     Total price for the contract
        /// </summary>
        public Rate Rate { get; }

        /// <summary>
        ///     Extra notes on the contract
        /// </summary>
        public List<KeyValuePair<string, string>> Remarks { get; }

        /// <summary>
        ///     Number of adult passengers
        /// </summary>
        [Required]
        public int AdultsNumber { get; }

        /// <summary>
        ///     Number of children
        /// </summary>
        public int ChildrenNumber { get; }

        // TODO: we pass a database entity straight to the world
        /// <summary>
        ///     Deadline and cancellation information.
        ///     <b>A null value means an unknown deadline for the first search steps, and it means an empty deadline at the evaluation step.</b>
        /// </summary>
        public Deadline Deadline { get; }

        /// <summary>
        ///     Indicates if a contract is an advance purchase
        /// </summary>
        public bool IsAdvancePurchaseRate { get; }

        /// <summary>
        ///     List of room prices on a daily basis
        /// </summary>
        public List<DailyRate> DailyRoomRates { get; }

        /// <summary>
        ///     Desired room type
        /// </summary>
        public RoomTypes Type { get; }
    }
}