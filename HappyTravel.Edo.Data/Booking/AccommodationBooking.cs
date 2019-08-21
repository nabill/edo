using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Booking
{
    public class AccommodationBooking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string AgentReference { get; set; }
        public string ReferenceCode { get; set; }
        public BookingStatusCodes Status { get; set; }
        public DateTime BookingDate { get; set; }
        public Currencies PriceCurrency { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string CityCode { get; set; }
        public string AccommodationId { get; set; }
        public string TariffCode { get; set; }
        public int ContractTypeId { get; set; }
        public DateTime Deadline { get; set; }
        public string Nationality { get; set; }
        public string Residency { get; set; }
        public string Service { get; set; }
        public string RateBasis { get; set; }
        public string CountryCode { get; set; }
        public Dictionary<string, string> Features { get; set; }
    }
}