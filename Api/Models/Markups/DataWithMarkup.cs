using System.Collections.Generic;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct DataWithMarkup<TData>
    {
        public DataWithMarkup(TData data, List<AppliedMarkup> appliedMarkups, MoneyAmount convertedSupplierPrice, MoneyAmount originalSupplierPrice)
        {
            Data = data;
            AppliedMarkups = appliedMarkups;
            ConvertedSupplierPrice = convertedSupplierPrice;
            OriginalSupplierPrice = originalSupplierPrice;
        }
        
        public TData Data { get; }
        public List<AppliedMarkup> AppliedMarkups { get; }
        public MoneyAmount ConvertedSupplierPrice { get; }
        public MoneyAmount OriginalSupplierPrice { get; }
    }
    
    public static class DataWithMarkup
    {
        public static DataWithMarkup<TProviderData> Create<TProviderData>(TProviderData data, List<AppliedMarkup> policies, MoneyAmount convertedSupplierPrice, MoneyAmount originalSupplierPrice)
            => new DataWithMarkup<TProviderData>(data, policies, convertedSupplierPrice, originalSupplierPrice);
    }
}