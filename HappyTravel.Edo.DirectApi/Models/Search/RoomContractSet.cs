using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public record RoomContractSet
    {
        [JsonConstructor]
        public RoomContractSet(Guid id, in Rate rate, Deadline deadline, List<RoomContract> rooms,
            bool isAdvancePurchaseRate, bool isPackageRate)
        {
            Id = id;
            Rate = rate;
            Deadline = deadline;
            Rooms = rooms;
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            IsPackageRate = isPackageRate;
        }
        
        /// <summary>
        ///     The set ID.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        ///     The total set price.
        /// </summary>
        public Rate Rate { get; }
        
        /// <summary>
        /// Deadline information
        /// </summary>
        public Deadline Deadline { get; }
        
        /// <summary>
        /// Is advanced purchase rate (Non-refundable)
        /// </summary>
        public bool IsAdvancePurchaseRate { get; }
        
        /// <summary>
        ///     The list of room contracts within a set.
        /// </summary>
        public List<RoomContract> Rooms { get; }
        
        /// <summary>
        /// Indicates that rates must be sold as a package
        /// </summary>
        public bool IsPackageRate { get; }
    }
}