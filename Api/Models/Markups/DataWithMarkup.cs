using System.Collections.Generic;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct DataWithMarkup<TData>
    {
        public DataWithMarkup(TData data, List<AppliedMarkup> appliedMarkups, decimal supplierPrice)
        {
            Data = data;
            AppliedMarkups = appliedMarkups;
            SupplierPrice = supplierPrice;
        }
        
        public TData Data { get; }
        public List<AppliedMarkup> AppliedMarkups { get; }
        public decimal SupplierPrice { get; }
    }
    
    public static class DataWithMarkup
    {
        public static DataWithMarkup<TProviderData> Create<TProviderData>(TProviderData data, List<AppliedMarkup> policies, decimal supplierPrice)
            => new DataWithMarkup<TProviderData>(data, policies, supplierPrice);
    }
}