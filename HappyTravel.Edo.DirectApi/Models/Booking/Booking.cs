using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct Booking
    {
        public Booking(string clientReferenceCode, string referenceCode, DateTime created, DateTime checkInDate, DateTime checkOutDate, MoneyAmount totalPrice,
            BookingStatuses status, List<BookedRoom> rooms, string accommodationId, List<CancellationPolicy> cancellationPolicies, DateTime? cancelled,
            bool isPackageRate)
        {
            ClientReferenceCode = clientReferenceCode;
            ReferenceCode = referenceCode;
            Created = created;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            TotalPrice = totalPrice;
            Status = status;
            Rooms = rooms;
            AccommodationId = accommodationId;
            CancellationPolicies = cancellationPolicies;
            Cancelled = cancelled;
            IsPackageRate = isPackageRate;
        }


        /// <summary>
        ///     Client booking reference code
        /// </summary>
        public string ClientReferenceCode { get; }

        /// <summary>
        ///     Happytravel.com 
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }

        /// <summary>
        ///     Date when an accommodation was booked
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        ///     Check-in date
        /// </summary>
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     Check-out date
        /// </summary>
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     Total net price of a service (This is the <b>actual</b> value for the price)
        /// </summary>
        public MoneyAmount TotalPrice { get; }

        /// <summary>
        ///     Current status of the booking
        /// </summary>
        public BookingStatuses Status { get; }

        /// <summary>
        ///     List of rooms in the booking contract
        /// </summary>
        public List<BookedRoom> Rooms { get; }

        /// <summary>
        ///     Cancellation information
        /// </summary>
        public List<CancellationPolicy> CancellationPolicies { get; }

        /// <summary>
        ///     Date when an accommodation booking was cancelled. 'NULL' means the booking is not cancelled.
        /// </summary>
        public DateTime? Cancelled { get; }

        /// <summary>
        ///     Indicates if the rate must be sold as a package
        /// </summary>
        public bool IsPackageRate { get; }
    }
}