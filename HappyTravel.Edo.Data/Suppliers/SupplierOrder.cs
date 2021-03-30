using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Suppliers
{
    public class SupplierOrder
    {
        public int Id { get; set; }
        public Common.Enums.Suppliers Supplier { get; set; }
        public decimal PriceInUsd { get; set; }
        public decimal SupplierPrice { get; set; }
        public Currencies SupplierCurrency { get; set; }
        public SupplierOrderState State { get; set; }
        public ServiceTypes Type { get; set; }
        public string ReferenceCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}