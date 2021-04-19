using System;

namespace HappyTravel.Edo.Api.Models.Reports.Converters
{
    public interface IConverter<in TIn, out TOut>
    {
        public TOut Convert(TIn projection, Func<decimal, decimal> vatAmountFunc, Func<decimal, decimal> amountExcludedVatFunc);
    }
}