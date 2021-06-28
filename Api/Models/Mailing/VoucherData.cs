using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class VoucherData : DataWithCompanyInfo
    {
        public BookingVoucherData.AccommodationInfo Accommodation { get; set; }
        public string AgentName { get; set; }
        public int BookingId { get; set; }
        public string DeadlineDate { get; set; }
        public int NightCount { get; set; }
        public string ReferenceCode { get; set; }
        public string SupplierReferenceCode { get; set; }
        public List<BookingVoucherData.RoomInfo> RoomDetails { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string MainPassengerName { get; set; }
        public string BannerUrl { get; set; }
        public string LogoUrl { get; set; }
    }
}