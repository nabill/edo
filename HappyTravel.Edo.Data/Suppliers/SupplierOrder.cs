using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Suppliers
{
    public class SupplierOrder
    {
        public int Id { get; set; }
        public Common.Enums.Suppliers Supplier { get; set; }
        public decimal ConvertedPrice { get; set; }
        public Currencies ConvertedCurrency { get; set; }
        public decimal Price { get; set; }
        public Currencies Currency { get; set; }
        public SupplierOrderState State { get; set; }
        public ServiceTypes Type { get; set; }
        public string ReferenceCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}