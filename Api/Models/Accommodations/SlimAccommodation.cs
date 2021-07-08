using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimAccommodation
    {
        [JsonConstructor]
        public SlimAccommodation(in SlimLocationInfo location, string name, in ImageInfo photo, AccommodationRatings rating,
            PropertyTypes propertyType, string htId = null, string hotelChain = null)
        {
            Location = location;
            Name = name;
            Photo = photo;
            Rating = rating;
            PropertyType = propertyType;
            HtId = htId ?? string.Empty;
            HotelChain = hotelChain;
        }


        /// <summary>
        ///     The accommodation location description.
        /// </summary>
        public SlimLocationInfo Location { get; }

        /// <summary>
        ///     The accommodation name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The front photo of an accommodation.
        /// </summary>
        public ImageInfo Photo { get; }

        /// <summary>
        ///     The rating of an accommodation.
        /// </summary>
        public AccommodationRatings Rating { get; }

        /// <summary>
        ///     The type of an accommodation.
        /// </summary>
        public PropertyTypes PropertyType { get; }

        /// <summary>
        ///     The Happytravel.com correlation ID.
        /// </summary>
        public string HtId { get; }

        /// <summary>
        ///     The name of a hotel chain which an accommodation belongs.
        /// </summary>
        public string HotelChain { get; }


        public override bool Equals(object obj) => obj is SlimAccommodation other && Equals(other);


        public bool Equals(in SlimAccommodation other)
            => (Location, Name, Photo, Rating, PropertyType)
                .Equals((other.Location, other.Name, other.Photo, other.Rating, other.PropertyType));


        public override int GetHashCode()
            => (Location, Name, Photo, Rating)
                .GetHashCode();
    }
}