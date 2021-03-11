﻿using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingStatusHistoryEntry
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int AgentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public BookingStatuses Status { get; set; }
        public BookingChangeReasons ChangeReason { get; set; }
    }
}
