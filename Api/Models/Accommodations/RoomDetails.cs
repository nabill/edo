using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomDetails
    {
        [JsonConstructor]
        public RoomDetails(List<RoomPrice> roomPrices, int adultsNumber, int childrenNumber = 0, List<int> childrenAges = null, RoomTypes type = RoomTypes.NotSpecified, 
            bool isExtraBedNeeded = false, bool isCotNeeded = false)
        {
            AdultsNumber = adultsNumber;
            ChildrenAges = childrenAges ?? new List<int>();
            ChildrenNumber = childrenNumber;
            IsCotNeeded = isCotNeeded;
            IsExtraBedNeeded = isExtraBedNeeded;
            RoomPrices = roomPrices;
            Type = type;
        }


        public RoomDetails(in RoomDetails details, List<RoomPrice> roomPrices) : this(roomPrices, details.AdultsNumber, details.ChildrenNumber,
            details.ChildrenAges, details.Type, details.IsExtraBedNeeded, details.IsCotNeeded)
        { }


        /// <summary>
        /// Required. Number of adult passengers.
        /// </summary>
        [Required]
        public int AdultsNumber { get; }
        
        /// <summary>
        /// Ages of each child.
        /// </summary>
        public List<int> ChildrenAges { get; }

        /// <summary>
        /// Number of children.
        /// </summary>
        public int ChildrenNumber { get; }

        /// <summary>
        /// Indicates if extra cot needed.
        /// </summary>
        public bool IsCotNeeded { get; }

        /// <summary>
        /// Indicates if extra child bed needed.
        /// </summary>
        public bool IsExtraBedNeeded { get; }

        /// <summary>
        /// Room prices.
        /// </summary>
        public List<RoomPrice> RoomPrices { get; }

        /// <summary>
        /// Desirable room type.
        /// </summary>
        public RoomTypes Type { get; }
    }
}
