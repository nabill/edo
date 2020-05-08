using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct BookedRoom
    {
        public BookedRoom(RoomTypes type, bool isExtraBedNeeded, MoneyAmount price, BoardBasisTypes boardBasis, string mealPlan, 
            DateTime? deadlineDate, string contractDescription, List<KeyValuePair<string, string>> remarks, DeadlineDetails deadlineDetails, List<Pax> passengers)
        {
            Type = type;
            Passengers = passengers;
            IsExtraBedNeeded = isExtraBedNeeded;
            Price = price;
            BoardBasis = boardBasis;
            MealPlan = mealPlan;
            DeadlineDate = deadlineDate;
            ContractDescription = contractDescription;
            Remarks = remarks ?? new List<KeyValuePair<string, string>>(0);
            DeadlineDetails = deadlineDetails;
            Passengers = passengers ?? new List<Pax>(0);
        }
        
        public BoardBasisTypes BoardBasis { get; }
        public string MealPlan { get; }
        public DateTime? DeadlineDate { get; }
        public string ContractDescription { get; }
        public List<KeyValuePair<string, string>> Remarks { get; }
        public DeadlineDetails DeadlineDetails { get; }

        public RoomTypes Type { get; }
        public bool IsExtraBedNeeded { get; }
        public MoneyAmount Price { get; }
        public List<Pax> Passengers { get; }
    }
}