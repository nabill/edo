﻿using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingFinalizedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public Details BookingDetails { get; set; }
        public string CounterpartyName { get; set; }
        public string PaymentStatus { get; set; }
        public string Price { get; set; }


        public class Details
        {
            public string AccommodationName { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public DateTime? DeadlineDate { get; set; }
            public AccommodationLocation Location { get; set; }
            public int NumberOfNights { get; set; }
            public int NumberOfPassengers { get; set; }
            public string ReferenceCode { get; set; }
            public List<BookedRoomDetails> RoomDetails { get; set; }
            public string Status { get; set; }
            public string SupplierReferenceCode { get; set; }
        }


        public class BookedRoomDetails
        {
            public string MealPlan { get; set;}
            public string ContractDescription { get; set;}
            public string Type { get; set;}
            public string Price { get; set;}
            public List<Pax> Passengers { get; set;}
        }
    }
}
