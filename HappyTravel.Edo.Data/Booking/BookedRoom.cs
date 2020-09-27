using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Data.Booking
{
    public class BookedRoom
    {
        // EF constructor
        private BookedRoom()
        {
        }
        
        public BookedRoom(RoomTypes type, bool isExtraBedNeeded, MoneyAmount price, BoardBasisTypes boardBasis, string mealPlan, 
            DateTime? deadlineDate, string contractDescription, List<KeyValuePair<string, string>> remarks, Deadline deadlineDetails, List<Pax> passengers)
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


        public BookedRoom(BookedRoom room, List<KeyValuePair<string, string>> remarks) : this(room.Type, room.IsExtraBedNeeded, room.Price,
            room.BoardBasis, room.MealPlan, room.DeadlineDate, room.ContractDescription, remarks, room.DeadlineDetails, room.Passengers)
        { }
        

        public BoardBasisTypes BoardBasis { get; set; }
        public string MealPlan { get; set;}
        public DateTime? DeadlineDate { get; set;}
        public string ContractDescription { get; set;}
        public List<KeyValuePair<string, string>> Remarks { get; set;}
        public Deadline DeadlineDetails { get; set;}
        public RoomTypes Type { get; set;}
        public bool IsExtraBedNeeded { get; set;}
        public MoneyAmount Price { get; set;}
        public List<Pax> Passengers { get; set;}
    }
}