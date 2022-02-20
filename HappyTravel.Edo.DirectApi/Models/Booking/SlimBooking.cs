using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct SlimBooking
    {
        public SlimBooking(string clientReferenceCode, string referenceCode, DateTime checkInDate, DateTime checkOutDate, 
            string accommodationId, MoneyAmount totalPrice, bool isAdvancePurchaseRate, BookingStatuses status, string mainPassengerName)
        {
            ClientReferenceCode = clientReferenceCode;
            ReferenceCode = referenceCode;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            AccommodationId = accommodationId;
            TotalPrice = totalPrice;
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            Status = status;
            MainPassengerName = mainPassengerName;
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
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     Check-out date
        /// </summary>
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }

        /// <summary>
        ///     Total net price of a service (This is the <b>actual</b> value for the price)
        /// </summary>
        public MoneyAmount TotalPrice { get; }

        // TODO: SlimBooking has this field, but Booking hasn't
        /// <summary>
        ///     Indicates if a contract is an advance purchase
        /// </summary>
        public bool IsAdvancePurchaseRate { get; }
        /// <summary>
        ///     Current status of the booking
        /// </summary>
        public BookingStatuses Status { get; }

        // TODO: we use MainPassenger and IsLeader. THat's inconsistent
        /// <summary>
        ///     Name of a group leader for the booking
        /// </summary>
        public string MainPassengerName { get; }
    }
}