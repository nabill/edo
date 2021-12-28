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
            string contractDescription, List<KeyValuePair<string, string>> remarks, Deadline deadlineDetails, List<Passenger> passengers)
        {
            Type = type;
            Passengers = passengers;
            Price = price;
            BoardBasis = boardBasis;
            MealPlan = mealPlan;
            ContractDescription = contractDescription;
            Remarks = remarks;
            DeadlineDetails = deadlineDetails;
            Passengers = passengers;
        }
        

        public BoardBasisTypes BoardBasis { get; }
        public string MealPlan { get;}
        public string ContractDescription { get;}
        public List<KeyValuePair<string, string>> Remarks { get;}
        public Deadline DeadlineDetails { get;}
        public RoomTypes Type { get;}
        public MoneyAmount Price { get;}
        public List<Passenger> Passengers { get;}
    }
}