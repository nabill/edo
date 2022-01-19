using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public record RoomContractSet
    {
        [JsonConstructor]
        public RoomContractSet(Guid id, in Rate rate, List<RoomContract> rooms, bool isPackageRate)
        {
            Id = id;
            Rate = rate;
            Rooms = rooms;
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
        ///     The list of room contracts within a set.
        /// </summary>
        public List<RoomContract> Rooms { get; }
        
        /// <summary>
        /// Indicates that rates must be sold as a package
        /// </summary>
        public bool IsPackageRate { get; }
    }
}