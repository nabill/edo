using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingAdministratorSummaryNotificationData : DataWithCompanyInfo
    {
        public string ReportDate { get; set; }
        public List<BookingRowData> Bookings { get; set; }
        
        public class BookingRowData
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
            public string Agency { get; set; }
            public string Agent { get; set; }
            public string PaymentStatus { get; set; }
            public string PaymentType { get; set; }
        }
    }
}