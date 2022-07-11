using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;
using System;

namespace HappyTravel.Edo.Api.Models.Bookings.Invoices
{
    public readonly struct InvoiceItemInfo
    {
        [JsonConstructor]
        public InvoiceItemInfo(int number, string accommodationName, string roomDescription, MoneyAmount price, MoneyAmount total, RoomTypes roomType,
            DateTime? deadlineDate, string mainPassengerFirstName, string mainPassengerLastName)
        {
            Number = number;
            AccommodationName = accommodationName;
            RoomDescription = roomDescription;
            Price = price;
            Total = total;
            RoomType = roomType;
            DeadlineDate = deadlineDate;
            MainPassengerFirstName = mainPassengerFirstName;
            MainPassengerLastName = mainPassengerLastName;
        }


        public InvoiceItemInfo(MoneyAmount total, InvoiceItemInfo invoiceItemInfo)
            : this(invoiceItemInfo.Number, invoiceItemInfo.AccommodationName, invoiceItemInfo.RoomDescription, invoiceItemInfo.Price,
                total, invoiceItemInfo.RoomType, invoiceItemInfo.DeadlineDate, invoiceItemInfo.MainPassengerFirstName, invoiceItemInfo.MainPassengerLastName)
            {}


        public int Number { get; }
        public string AccommodationName { get; }
        public string RoomDescription { get; }
        public MoneyAmount Price { get; }
        public MoneyAmount Total { get; }
        public RoomTypes RoomType { get; }
        public DateTime? DeadlineDate { get; }
        public string MainPassengerFirstName { get; }
        public string MainPassengerLastName { get; }
    }
}
