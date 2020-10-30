using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    public class SupplierAccommodationId
    {
        // EF constructor
        private SupplierAccommodationId()
        {
        }
            
            
        [JsonConstructor]
        public SupplierAccommodationId(Common.Enums.Suppliers dataProvider, string id)
        {
            DataProvider = dataProvider;
            Id = id;
        }


        public override string ToString() => $"{DataProvider}{StringDelimiter}{Id}";


        public bool Equals(SupplierAccommodationId other) => Id == other.Id && DataProvider == other.DataProvider;


        public override bool Equals(object obj) => obj is SupplierAccommodationId other && Equals(other);


        public override int GetHashCode() => HashCode.Combine(Id, (int) DataProvider);


        public static SupplierAccommodationId FromString(string accommodationId)
        {
            var idParts = accommodationId.Split(StringDelimiter);
            return new SupplierAccommodationId(Enum.Parse<Common.Enums.Suppliers>(idParts[0]), idParts[1]);
        }


        /// <summary>
        /// Id in data provider
        /// </summary>
        [Required]
        public string Id { get; set; }
        
        
        /// <summary>
        /// Provider code
        /// </summary>
        [Required]
        public Common.Enums.Suppliers DataProvider { get; set; }


        private const string StringDelimiter = "::";
    }
}