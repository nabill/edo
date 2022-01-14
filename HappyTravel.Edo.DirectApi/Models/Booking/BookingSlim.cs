using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct BookingSlim
    {
        public BookingSlim(string clientReferenceCode, string referenceCode, DateTime checkInDate, DateTime checkOutDate, 
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


        public string ClientReferenceCode { get; }
        public string ReferenceCode { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public string AccommodationId { get; }
        public MoneyAmount TotalPrice { get; }
        public bool IsAdvancePurchaseRate { get; }
        public BookingStatuses Status { get; }
        public string MainPassengerName { get; }
    }
}