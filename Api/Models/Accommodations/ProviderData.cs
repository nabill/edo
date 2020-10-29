using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct ProviderData<TData>
    {
        [JsonConstructor]
        public ProviderData(Suppliers source, TData data)
        {
            Source = source;
            Data = data;
        }
        
        /// <summary>
        /// Results source
        /// </summary>
        public Suppliers Source { get; }
        
        /// <summary>
        /// Nested data
        /// </summary>
        public TData Data { get; }
        
        public bool Equals(ProviderData<TData> other) => Source == other.Source && EqualityComparer<TData>.Default.Equals(Data, other.Data);

        public override bool Equals(object obj) => obj is ProviderData<TData> other && Equals(other);
        
        public override int GetHashCode() => HashCode.Combine((int) Source, Data);
    }
    
    public static class ProviderData
    {
        public static ProviderData<TProviderData> Create<TProviderData>(Suppliers source, TProviderData data) => new ProviderData<TProviderData>(source, data);
    }
}