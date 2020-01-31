using System;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(PaymentTokenInfo token, string securityCode, string itineraryNumber, long availabilityId, Guid agreementId )
        {
            Token = token;
            SecurityCode = securityCode;
            ItineraryNumber = itineraryNumber;
            AvailabilityId = availabilityId;
            AgreementId = agreementId;
        }


        /// <summary>
        ///     Payment token
        /// </summary>
        public PaymentTokenInfo Token { get; }

        
        /// <summary>
        ///     Credit card security code
        /// </summary>
        public string SecurityCode { get; }
        
        
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