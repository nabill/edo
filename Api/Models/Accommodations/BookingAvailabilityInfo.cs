using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Geography;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        [JsonConstructor]
        public BookingAvailabilityInfo(string accommodationId, string accommodationName, AccommodationInfo accommodationInfo,
            RoomContractSet roomContractSet, string zoneName, string localityName, string countryName,
            string countryCode, string address, GeoPoint coordinates, DateTime checkInDate,
            DateTime checkOutDate, int numberOfNights, string supplierCode, List<AppliedMarkup> appliedMarkups,
            MoneyAmount convertedSupplierPrice, MoneyAmount originalSupplierPrice, string availabilityId,
            string htId, List<PaymentTypes> availablePaymentTypes, bool isDirectContract, Deadline agentDeadline, Deadline supplierDeadline,
            CreditCardRequirement? cardRequirement, AvailabilityRequest availabilityRequest)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            AccommodationInfo = accommodationInfo;
            RoomContractSet = roomContractSet;
            ZoneName = zoneName;
            LocalityName = localityName;
            CountryName = countryName;
            CountryCode = countryCode;
            Address = address;
            Coordinates = coordinates;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            SupplierCode = supplierCode;
            AppliedMarkups = appliedMarkups;
            ConvertedSupplierPrice = convertedSupplierPrice;
            OriginalSupplierPrice = originalSupplierPrice;
            AvailabilityId = availabilityId;
            HtId = htId;
            AvailablePaymentTypes = availablePaymentTypes;
            IsDirectContract = isDirectContract;
            AgentDeadline = agentDeadline;
            SupplierDeadline = supplierDeadline;
            CardRequirement = cardRequirement;
            AvailabilityRequest = availabilityRequest;
        }


        public string AccommodationId { get; }
        public string AccommodationName { get; }
        public AccommodationInfo AccommodationInfo { get; }
        public RoomContractSet RoomContractSet { get; }
        public string ZoneName { get; }
        public string LocalityName { get; }
        public string CountryName { get; }
        public string CountryCode { get; }
        public string Address { get; }
        public GeoPoint Coordinates { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public int NumberOfNights { get; }
        public string SupplierCode { get; }
        public List<AppliedMarkup> AppliedMarkups { get; }
        public MoneyAmount ConvertedSupplierPrice { get; }
        public MoneyAmount OriginalSupplierPrice { get; }
        public string AvailabilityId { get; }
        public string HtId { get; }
        public List<PaymentTypes> AvailablePaymentTypes { get; }
        public bool IsDirectContract { get; }
        public Deadline AgentDeadline { get; }
        public Deadline SupplierDeadline { get; }
        public CreditCardRequirement? CardRequirement { get; }
        public AvailabilityRequest AvailabilityRequest { get; }


        public bool Equals(BookingAvailabilityInfo other)
            => (AccommodationId, AccommodationName, RoomContractSet: RoomContractSet, LocalityName, CountryName, CheckInDate, CheckOutDate, NumberOfNights, AvailabilityId, AvailabilityRequest)
                .Equals((other.AccommodationId, other.AccommodationName, other.RoomContractSet, other.LocalityName,
                    other.CountryName, other.CheckInDate, other.CheckOutDate, other.NumberOfNights, other.AvailabilityId, other.AvailabilityRequest));


        public override bool Equals(object obj) => obj is BookingAvailabilityInfo other && Equals(other);


        public override int GetHashCode()
            => (AccommodationId, AccommodationName, RoomContractSet: RoomContractSet, LocalityName, CountryName, CheckInDate, CheckOutDate, AvailabilityRequest)
                .GetHashCode();
    }
}