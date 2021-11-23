using System;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingChangedStatusData : DataWithCompanyInfo
    {
        // TODO: remove when we have appropriate admin panel booking monitoring
        public int BookingId { get; set; }
        public string ReferenceCode { get; set; }
        public string Status { get; set; }
        public DateTime ChangeTime { get; set; }
        public string AccommodationName { get; set; }
        public ImageInfo AccommodationPhoto { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}