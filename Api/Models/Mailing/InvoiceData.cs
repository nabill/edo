using HappyTravel.Edo.Api.Models.Bookings.Invoices;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class InvoiceData : DataWithCompanyInfo
    {
        public string Number { get; set; }
        public BuyerInfo BuyerDetails { get; set; }
        public string InvoiceDate { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public string TotalPrice { get; set; }
        public string NetPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string ReferenceCode { get; set; }
        public string? ClientReferenceCode { get; set; }
        public string SupplierReferenceCode { get; set; }
        public SellerInfo SellerDetails { get; set; }
        public string PayDueDate { get; set; }

        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string DeadlineDate { get; set; }
        public string InvoiceStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string MainPassengerName { get; set; }


        public class InvoiceItem
        {
            public int Number { get; set; }
            public string Price { get; set; }
            public string Total { get; set; }
            public string AccommodationName { get; set; }
            public string RoomDescription { get; set; }
            public string RoomType { get; set; }
            public string DeadlineDate { get; set; }
            public string MainPassengerName { get; set; }
        }
    }
}