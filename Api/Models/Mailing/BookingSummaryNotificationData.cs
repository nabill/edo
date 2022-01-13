using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingSummaryNotificationData : DataWithCompanyInfo
    {
        public string CurrentBalance { get; set; }
        public string ResultingBalance { get; set; }
        public string ReportDate { get; set; }
        public bool ShowAlert { get; set; }
        public List<BookingData> Bookings { get; set; }


        public class BookingData
        {
            public string ReferenceCode { get; set; }
            public string Accommodation { get; set; }
            public string Location { get; set; }
            public string LeadingPassenger { get; set; }
            public string CheckInDate { get; set; }
            public string CheckOutDate { get; set; }
            public string Amount { get; set; }
            public string DeadlineDate { get; set; }
            public string Status { get; set; }
            public string PaymentType { get; set; }
        }
    }
}
