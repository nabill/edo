using System;
using HappyTravel.Edo.DirectApi.Enum;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public class BookingsListFilter
    {
        public DateTime? CreatedFrom { get; set; } 
        public DateTime? CreatedTo { get; set; }
        // TODO: fix naming
        public DateTime? CheckinFrom { get; set; }
        public DateTime? CheckinTo { get; set; }
        public BookingListOrderTypes OrderBy { get; set; }
    }
}