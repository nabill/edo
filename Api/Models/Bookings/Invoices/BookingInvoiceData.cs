using HappyTravel.Money.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Bookings.Invoices
{
    public readonly struct BookingInvoiceData
    {
        [JsonConstructor]
        public BookingInvoiceData(in BuyerInfo buyerDetails, in SellerInfo sellerDetails, string referenceCode, string? clientReferenceCode, string supplierReferenceCode,
            List<InvoiceItemInfo> invoiceItems, MoneyAmount totalPrice, in DateTime payDueDate, DateTime checkInDate, DateTime checkOutDate,
            DateTime? deadlineDate)
        {
            BuyerDetails = buyerDetails;
            PayDueDate = payDueDate;
            ReferenceCode = referenceCode;
            ClientReferenceCode = clientReferenceCode;
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
        public string? ClientReferenceCode { get; }
        public string SupplierReferenceCode { get; }
        public List<InvoiceItemInfo> InvoiceItems { get; }
        public MoneyAmount TotalPrice { get; }
        public SellerInfo SellerDetails { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime? DeadlineDate { get; }
    }
}