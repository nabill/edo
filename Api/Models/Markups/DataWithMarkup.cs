using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct DataWithMarkup<TData>
    {
        public DataWithMarkup(TData data, List<MarkupPolicy> policies)
        {
            Data = data;
            Policies = policies;
        }
        
        public TData Data { get; }
        public List<MarkupPolicy> Policies { get; }
    }
    
    public static class DataWithMarkup
    {
        public static DataWithMarkup<TProviderData> Create<TProviderData>(TProviderData data, List<MarkupPolicy> policies) => new DataWithMarkup<TProviderData>(data, policies);
    }
}