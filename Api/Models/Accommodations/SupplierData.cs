using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SupplierData<TData>
    {
        [JsonConstructor]
        public SupplierData(Suppliers source, TData data)
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
        
        public bool Equals(SupplierData<TData> other) => Source == other.Source && EqualityComparer<TData>.Default.Equals(Data, other.Data);

        public override bool Equals(object obj) => obj is SupplierData<TData> other && Equals(other);
        
        public override int GetHashCode() => HashCode.Combine((int) Source, Data);
    }
    
    public static class SupplierData
    {
        public static SupplierData<TProviderData> Create<TProviderData>(Suppliers source, TProviderData data) => new SupplierData<TProviderData>(source, data);
    }
}