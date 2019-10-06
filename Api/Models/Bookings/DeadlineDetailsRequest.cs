using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct DeadlineDetailsRequest
    {
        [JsonConstructor]
        public DeadlineDetailsRequest(
            string availabilityId, 
            string accommodationId, 
            string tariffCode, 
            DataProvidersContractTypes contractType, 
            string languageCode)
        {
            AvailabilityId = availabilityId;
            AccommodationId = accommodationId;
            TariffCode = tariffCode;
            ContractType = contractType;
            LanguageCode = languageCode;
        }

        public string AccommodationId { get; }
        public string AvailabilityId { get; }
        public string TariffCode { get; }
        public DataProvidersContractTypes ContractType { get; }
        public string LanguageCode { get; }
    }
}