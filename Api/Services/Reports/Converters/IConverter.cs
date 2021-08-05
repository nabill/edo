using System;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public interface IConverter<in TIn, out TOut>
    {
        public TOut Convert(TIn data, Func<decimal, decimal> vatAmountFunc, Func<decimal, decimal> amountExcludedVatFunc);
    }
}