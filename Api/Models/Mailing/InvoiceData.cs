using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class InvoiceData : DataWithCompanyInfo
    {
        public int Id { get; set; }
        public BookingInvoiceData.BuyerInfo BuyerDetails { get; set; }
        public string InvoiceDate { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public string TotalPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string ReferenceCode { get; set; }
        public BookingInvoiceData.SellerInfo SellerDetails { get; set; }
        public string PayDueDate { get; set; }
    }

    public class InvoiceItem
    {
        public int Number { get; set; }
        public string Price { get; set; }
        public string Total { get; set; }
        public string AccommodationName { get; set; }
        public string RoomDescription { get; set; }
    }
}