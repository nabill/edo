using System.Collections.Generic;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct DataWithMarkup<TData>
    {
        public DataWithMarkup(TData data, List<AppliedMarkup> appliedMarkups, MoneyAmount convertedPrice, MoneyAmount supplierPrice)
        {
            Data = data;
            AppliedMarkups = appliedMarkups;
            ConvertedPrice = convertedPrice;
            SupplierPrice = supplierPrice;
        }
        
        public TData Data { get; }
        public List<AppliedMarkup> AppliedMarkups { get; }
        public MoneyAmount ConvertedPrice { get; }
        public MoneyAmount SupplierPrice { get; }
    }
    
    public static class DataWithMarkup
    {
        public static DataWithMarkup<TProviderData> Create<TProviderData>(TProviderData data, List<AppliedMarkup> policies, MoneyAmount convertedPrice, MoneyAmount supplierPrice)
            => new DataWithMarkup<TProviderData>(data, policies, convertedPrice, supplierPrice);
    }
}