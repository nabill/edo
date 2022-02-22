using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        ///     ID for the room contract set
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        ///     Total price for the room contract set
        /// </summary>
        public Rate Rate { get; }
        
        /// <summary>
        ///     List of room contracts within a set
        /// </summary>
        public List<RoomContract> Rooms { get; }
        
        /// <summary>
        ///     Indicates if the rate must be sold as a package
        /// </summary>
        public bool IsPackageRate { get; }
    }
}