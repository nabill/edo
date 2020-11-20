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
        public SupplierAccommodationId(Common.Enums.Suppliers supplier, string id)
        {
            Supplier = supplier;
            Id = id;
        }


        public override string ToString() => $"{Supplier}{StringDelimiter}{Id}";


        public bool Equals(SupplierAccommodationId other) => Id == other.Id && Supplier == other.Supplier;


        public override bool Equals(object obj) => obj is SupplierAccommodationId other && Equals(other);


        public override int GetHashCode() => HashCode.Combine(Id, (int) Supplier);


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
        /// Supplier code
        /// </summary>
        [Required]
        public Common.Enums.Suppliers Supplier { get; set; }


        private const string StringDelimiter = "::";
    }
}