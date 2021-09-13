using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct BookingInvoiceData
    {
        [JsonConstructor]
        public BookingInvoiceData(in BuyerInfo buyerDetails, in SellerInfo sellerDetails, string referenceCode, string supplierReferenceCode,
            List<InvoiceItemInfo> invoiceItems, MoneyAmount totalPrice, in DateTime payDueDate, DateTime checkInDate, DateTime checkOutDate,
            DateTime? deadlineDate)
        {
            BuyerDetails = buyerDetails;
            PayDueDate = payDueDate;
            ReferenceCode = referenceCode;
            SupplierReferenceCode = supplierReferenceCode;
            InvoiceItems = invoiceItems;
            TotalPrice = totalPrice;
            SellerDetails = sellerDetails;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            DeadlineDate = deadlineDate;
        }


        public BuyerInfo BuyerDetails { get; }
        public DateTime PayDueDate { get; }
        public string ReferenceCode { get; }
        public string SupplierReferenceCode { get; }
        public List<InvoiceItemInfo> InvoiceItems { get; }
        public MoneyAmount TotalPrice { get; }
        public SellerInfo SellerDetails { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime? DeadlineDate { get; }


        public readonly struct BuyerInfo
        {
            [JsonConstructor]
            public BuyerInfo(string name, string address, string contactPhone, string email)
            {
                Address = address;
                ContactPhone = contactPhone;
                Email = email;
                Name = name;
            }


            public string Address { get; }
            public string ContactPhone { get; }
            public string Email { get; }
            public string Name { get; }
        }


        public readonly struct SellerInfo
        {
            [JsonConstructor]
            public SellerInfo(string companyName, string bankName, string bankAddress, string accountNumber, string iban, string routingCode, string swiftCode)
            {
                AccountNumber = accountNumber;
                BankAddress = bankAddress;
                BankName = bankName;
                CompanyName = companyName;
                Iban = iban;
                RoutingCode = routingCode;
                SwiftCode = swiftCode;
            }


            public string AccountNumber { get; }
            public string BankAddress { get; }
            public string BankName { get; }
            public string CompanyName { get; }
            public string Iban { get; }
            public string RoutingCode { get; }
            public string SwiftCode { get; }
        }
        
        
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
}