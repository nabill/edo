﻿using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct BookingInvoiceInfo
    {
        public BookingInvoiceInfo(BookingInvoiceData bookingInvoiceData, BookingPaymentStatuses paymentStatus)
        {
            BuyerDetails = bookingInvoiceData.BuyerDetails;
            PayDueDate = bookingInvoiceData.PayDueDate;
            ReferenceCode = bookingInvoiceData.ReferenceCode;
            SupplierReferenceCode = bookingInvoiceData.SupplierReferenceCode;
            InvoiceItems = bookingInvoiceData.InvoiceItems;
            TotalPrice = bookingInvoiceData.TotalPrice;
            SellerDetails = bookingInvoiceData.SellerDetails;
            CheckInDate = bookingInvoiceData.CheckInDate;
            CheckOutDate = bookingInvoiceData.CheckOutDate;
            DeadlineDate = bookingInvoiceData.DeadlineDate;
            PaymentStatus = paymentStatus;
        }


        public BookingInvoiceData.BuyerInfo BuyerDetails { get; }
        public DateTime PayDueDate { get; }
        public string ReferenceCode { get; }
        public string SupplierReferenceCode { get; }
        public List<BookingInvoiceData.InvoiceItemInfo> InvoiceItems { get; }
        public MoneyAmount TotalPrice { get; }
        public BookingInvoiceData.SellerInfo SellerDetails { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime? DeadlineDate { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
    }
}