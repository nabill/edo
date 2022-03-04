using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct SlimBooking
    {
        public SlimBooking(string clientReferenceCode, string referenceCode, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, 
            string accommodationId, MoneyAmount totalPrice, BookingStatuses status, string leadPassengerName)
        {
            ClientReferenceCode = clientReferenceCode;
            ReferenceCode = referenceCode;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            AccommodationId = accommodationId;
            TotalPrice = totalPrice;
            Status = status;
            LeadPassengerName = leadPassengerName;
        }


        /// <summary>
        ///     Client booking reference code
        /// </summary>
        public string ClientReferenceCode { get; }

        /// <summary>
        ///     Happytravel.com reference code
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     Check-in date
        /// </summary>
        public DateTimeOffset CheckInDate { get; }

        /// <summary>
        ///     Check-out date
        /// </summary>
        public DateTimeOffset CheckOutDate { get; }

        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }

        /// <summary>
        ///     Total net price of a service (This is the <b>actual</b> value for the price)
        /// </summary>
        public MoneyAmount TotalPrice { get; }

        /// <summary>
        ///     Current status of the booking
        /// </summary>
        public BookingStatuses Status { get; }

        /// <summary>
        ///     Name of a group leader for the booking
        /// </summary>
        public string LeadPassengerName { get; }
    }
}