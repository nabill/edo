using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct ProviderAccommodationId
    {
        [JsonConstructor]
        public ProviderAccommodationId(DataProviders dataProvider, string id)
        {
            DataProvider = dataProvider;
            Id = id;
        }


        public override string ToString() => $"{DataProvider}{StringDelimiter}{Id}";


        public bool Equals(ProviderAccommodationId other) => Id == other.Id && DataProvider == other.DataProvider;


        public override bool Equals(object obj) => obj is ProviderAccommodationId other && Equals(other);


        public override int GetHashCode() => HashCode.Combine(Id, (int) DataProvider);


        public static ProviderAccommodationId FromString(string accommodationId)
        {
            var idParts = accommodationId.Split(StringDelimiter);
            return new ProviderAccommodationId(Enum.Parse<DataProviders>(idParts[0]), idParts[1]);
        }


        /// <summary>
        /// Id in data provider
        /// </summary>
        [Required]
        public string Id { get; }
        
        
        /// <summary>
        /// Provider code
        /// </summary>
        [Required]
        public DataProviders DataProvider { get; }


        private const string StringDelimiter = "::";
    }
}