using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request with account
    /// </summary>
    public readonly struct AccountPaymentRequest
    {
        [JsonConstructor]
        public AccountPaymentRequest(string itineraryNumber, long availabilityId, Guid agreementId)
        {
            ItineraryNumber = itineraryNumber;
            AvailabilityId = availabilityId;
            AgreementId = agreementId;
        }


        /// <summary>
        ///    The itinerary number (code) to combine several orders in one pack.
        /// </summary>
        public string ItineraryNumber { get; }
        
        
        /// <summary>
        ///     Exact search availability id
        /// </summary>
        public long AvailabilityId { get; }
        
        
        /// <summary>
        ///     Identifier of chosen agreement.
        /// </summary>
        public Guid AgreementId { get; }
    }
}