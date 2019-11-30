using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingVoucherMailData
    {
        public string BookingId { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public List<BookingRoomDetails> RoomDetails { get; set; }
        public string AccomodationName { get; set; }
        public string LocationName { get; set; }
        public string CountryName { get; set; }
        public string BoardBasis { get; set; }
        public string BoardBasisCode { get; set; }
        public string ContractType { get; set; }
        public string MealPlan { get; set; }
        public string MealPlanCode { get; set; }
        public string TariffCode { get; set; }
        public string ReferenceCode { get; set; }
    }
}