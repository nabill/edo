using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContractSetAvailability
    {
        [JsonConstructor]
        public RoomContractSetAvailability(string availabilityId, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, int numberOfNights,
            in SlimAccommodation accommodation, in RoomContractSet roomContractSet, List<PaymentTypes> availablePaymentMethods,
            string countryHtId, string localityHtId, string evaluationToken, int marketId, string countryCode, string supplierCode)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            Accommodation = accommodation;
            RoomContractSet = roomContractSet;
            AvailablePaymentMethods = availablePaymentMethods;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            EvaluationToken = evaluationToken;
            MarketId = marketId;
            CountryCode = countryCode;
            SupplierCode = supplierCode;
        }

        /// <summary>
        ///     The availability ID.
        /// </summary>
        public string AvailabilityId { get; }

        /// <summary>
        ///     The check-in date.
        /// </summary>
        public DateTimeOffset CheckInDate { get; }

        /// <summary>
        ///     The check-out date.
        /// </summary>
        public DateTimeOffset CheckOutDate { get; }

        /// <summary>
        ///     The number of nights to stay.
        /// </summary>
        public int NumberOfNights { get; }

        /// <summary>
        ///     Information about a selected accommodation.
        /// </summary>
        public SlimAccommodation Accommodation { get; }

        /// <summary>
        ///     Information about a selected room contract set.
        /// </summary>
        public RoomContractSet RoomContractSet { get; }

        /// <summary>
        /// List of available payment methods
        /// </summary>
        public List<PaymentTypes> AvailablePaymentMethods { get; }

        /// <summary>
        /// Country of accommodation
        /// </summary>
        public string CountryHtId { get; }

        /// <summary>
        /// Locality of accommodation
        /// </summary>
        public string LocalityHtId { get; }

        /// <summary>
        /// Token to use between 3-rd step and booking to avoid double bookings
        /// </summary>
        public string EvaluationToken { get; }

        /// <summary>
        /// Market of accommodation
        /// </summary>
        public int MarketId { get; }

        /// <summary>
        /// Country code of accommodation
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// Supplier code of accommodation
        /// </summary>
        public string SupplierCode { get; }
    }
}