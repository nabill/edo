using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContractSet
    {
        public RoomContractSet(Guid id, in Rate rate, Deadline deadline, List<RoomContract> rooms,
            bool isAdvancePurchaseRate, Suppliers? supplier)
        {
            Id = id;
            Rate = rate;
            Deadline = deadline;
            Rooms = rooms ?? new List<RoomContract>(0);
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            Supplier = supplier;
        }
        
        public Guid Id { get; }
        public Rate Rate { get; }
        public Deadline Deadline { get; }
        public List<RoomContract> Rooms { get; }
        public bool IsAdvancePurchaseRate { get; }
        public Suppliers? Supplier { get; }
    }
}