using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingSummaryNotificationData : DataWithCompanyInfo
    {
        public string CurrentBalance;
        public string ResultingBalance;
        public string ReportDate;
        public bool ShowAlert;
        public List<BookingData> Bookings;


        public class BookingData
        {
            public string ReferenceCode;
            public string Accommodation;
            public string Location;
            public string LeadingPassenger;
            public string CheckInDate;
            public string CheckOutDate;
            public string Amount;
            public string DeadlineDate;
            public string Status;
        }
    }
}
