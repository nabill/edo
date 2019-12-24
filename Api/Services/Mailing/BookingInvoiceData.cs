using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingInvoiceData
    {
        public List<BookingRoomDetails> RoomDetails { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string AccomodationName { get; set; }
        public string LocationName { get; set; }
        public string CountryName { get; set; }
        public string TariffCode { get; set; }
        public string CurrencyCode { get; set; }
        public string PriceTotal { get; set; }
        public string PriceGross { get; set; }
        public string PriceOriginal { get; set; }
    }
}