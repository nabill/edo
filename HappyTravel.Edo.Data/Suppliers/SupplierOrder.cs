using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Data.Suppliers
{
    public class SupplierOrder
    {
        public int Id { get; set; }
        public SuppliersCatalog.Suppliers Supplier { get; set; }
        public decimal ConvertedPrice { get; set; }
        public Currencies ConvertedCurrency { get; set; }
        public decimal Price { get; set; }
        public Currencies Currency { get; set; }
        public SupplierOrderState State { get; set; }
        public ServiceTypes Type { get; set; }
        public string ReferenceCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public Deadline Deadline { get; set; }
        public decimal RefundableAmount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}