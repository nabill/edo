using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct BookedRoom
    {
        public BookedRoom(RoomTypes type, bool isExtraBedNeeded, MoneyAmount price, BoardBasisTypes boardBasis, string mealPlan,
            DateTime? deadlineDate, string contractDescription, List<KeyValuePair<string, string>> remarks, Deadline deadlineDetails,
            List<Passenger> passengers)
        {
            Type = type;
            Passengers = passengers;
            IsExtraBedNeeded = isExtraBedNeeded;
            Price = price;
            BoardBasis = boardBasis;
            MealPlan = mealPlan;
            DeadlineDate = deadlineDate;
            ContractDescription = contractDescription;
            Remarks = remarks;
            DeadlineDetails = deadlineDetails;
            Passengers = passengers;
        }
        

        public BoardBasisTypes BoardBasis { get; }
        public string MealPlan { get;}
        public DateTime? DeadlineDate { get;}
        public string ContractDescription { get;}
        public List<KeyValuePair<string, string>> Remarks { get;}
        public Deadline DeadlineDetails { get;}
        public RoomTypes Type { get;}
        public bool IsExtraBedNeeded { get;}
        public MoneyAmount Price { get;}
        public List<Passenger> Passengers { get;}
    }
}