using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.SuppliersCatalog;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record RoomContractSet
    {
        [JsonConstructor]
        public RoomContractSet(Guid id, in Rate rate, Deadline deadline, List<RoomContract> rooms,
            bool isAdvancePurchaseRate, Suppliers? supplier, List<string> tags, bool isDirectContract)
        {
            Id = id;
            Rate = rate;
            Deadline = deadline;
            Rooms = rooms ?? new List<RoomContract>(0);
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            Supplier = supplier;
            Tags = tags;
            IsDirectContract = isDirectContract;
        }
        
        /// <summary>
        ///     The set ID.
        /// </summary>
        public Guid Id { get; init; }
        
        /// <summary>
        ///     The total set price.
        /// </summary>
        public Rate Rate { get; init; }
        
        /// <summary>
        /// Deadline information
        /// </summary>
        public Deadline Deadline { get; init; }
        
        /// <summary>
        /// Is advanced purchase rate (Non-refundable)
        /// </summary>
        public bool IsAdvancePurchaseRate { get; init; }
        
        /// <summary>
        ///     The list of room contracts within a set.
        /// </summary>
        public List<RoomContract> Rooms { get; init; }
        
        /// <summary>
        /// Supplier
        /// </summary>
        public Suppliers? Supplier { get; init; }
        
        /// <summary>
        /// System tags returned by connector, e.g. "DirectConnectivity"
        /// </summary>
        public List<string> Tags { get; init; }
        
        /// <summary>
        /// Direct contract flag
        /// </summary>
        public bool IsDirectContract { get; init; }
    }
}