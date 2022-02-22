using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct BookedRoom
    {
        [JsonConstructor]
        public BookedRoom(RoomTypes type, MoneyAmount price, BoardBasisTypes boardBasis, string mealPlan,
            string contractDescription, List<KeyValuePair<string, string>> remarks, Deadline deadline, List<Passenger> passengers)
        {
            Type = type;
            Passengers = passengers;
            Price = price;
            BoardBasis = boardBasis;
            MealPlan = mealPlan;
            ContractDescription = contractDescription;
            Remarks = remarks;
            Deadline = deadline;
            Passengers = passengers;
        }
        

        /// <summary>
        ///     Meals included in the contract
        /// </summary>
        public BoardBasisTypes BoardBasis { get; }

        /// <summary>
        ///     Description of the board basis (included meals)
        /// </summary>
        public string MealPlan { get;}

        /// <summary>
        ///     Description for the contract, such as "Pool View Suite", "Ocean Club Room", or "Pioneer Cabin"
        /// </summary>
        public string ContractDescription { get;}

        /// <summary>
        ///     Extra notes on the contract
        /// </summary>
        public List<KeyValuePair<string, string>> Remarks { get;}

        /// <summary>
        ///     Deadline and cancellation information
        /// </summary>
        public Deadline Deadline { get;}

        /// <summary>
        ///     Booked room type
        /// </summary>
        public RoomTypes Type { get;}

        /// <summary>
        ///     Room price
        /// </summary>
        public MoneyAmount Price { get;}

        /// <summary>
        ///     List of passengers in the room
        /// </summary>
        public List<Passenger> Passengers { get;}
    }
}