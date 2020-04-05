using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct SlimRoomContract
    {
        [JsonConstructor]
        public SlimRoomContract(RoomContract roomContract)
        {
            MealPlan = roomContract.MealPlan;
            MealPlanCode = roomContract.MealPlanCode;
            ContractType = roomContract.ContractType;
            BoardBasisCode = roomContract.BoardBasisCode;
            BoardBasis = roomContract.BoardBasis;
        }


        public string MealPlan { get; }
        public string MealPlanCode { get; }
        public string ContractType { get; }
        public string BoardBasisCode { get; }
        public string BoardBasis { get; }
    }
}