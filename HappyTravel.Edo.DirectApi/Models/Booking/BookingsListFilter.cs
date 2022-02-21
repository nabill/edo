using System;
using HappyTravel.Edo.DirectApi.Enum;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public class BookingsListFilter
    {
        public DateTime? CreatedFrom { get; set; } 
        public DateTime? CreatedTo { get; set; }
        public DateTime? CheckInFrom { get; set; }
        public DateTime? CheckInTo { get; set; }
        public BookingListOrderTypes OrderBy { get; set; }
    }
}