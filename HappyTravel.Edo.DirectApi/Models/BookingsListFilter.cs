﻿using System;
using HappyTravel.Edo.DirectApi.Enum;

namespace HappyTravel.Edo.DirectApi.Models
{
    public class BookingsListFilter
    {
        public DateTime? CreatedFrom { get; set; } 
        public DateTime? CreatedTo { get; set; }
        public DateTime? CheckinFrom { get; set; }
        public DateTime? CheckinTo { get; set; }
        public BookingListOrderTypes OrderBy { get; set; }
    }
}