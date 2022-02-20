using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Extensions;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct RoomOccupationRequest
    {
        [JsonConstructor]
        public RoomOccupationRequest(int adultsNumber, List<int> childrenAges, RoomTypes type = RoomTypes.NotSpecified)
        {
            AdultsNumber = adultsNumber;
            ChildrenAges = childrenAges;
            Type = type;
        }


        /// <summary>
        ///     Number of adult passengers
        /// </summary>
        [Required]
        public int AdultsNumber { get; }

        /// <summary>
        ///     Ages of each child
        /// </summary>
        public List<int> ChildrenAges { get; }

        /// <summary>
        ///     Desirable room type
        /// </summary>
        public RoomTypes Type { get; }

        public override bool Equals(object? obj) => obj is RoomOccupationRequest other && Equals(other);


        public bool Equals(in RoomOccupationRequest other)
            => (AdultsNumber, Type)
                .Equals((other.AdultsNumber, other.Type)) &&
                ChildrenAges.SafeSequenceEqual(other.ChildrenAges);


        public override int GetHashCode() => (AdultsNumber, ChildrenAges, Type).GetHashCode();
    }
}