using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AgreementPrice
    {
        [JsonConstructor]
        public AgreementPrice(decimal total, decimal gross, decimal original)
        {
            Total = total;
            Gross = gross;
            Original = original;
        }


        public decimal Total { get; }
        public decimal Gross { get; }
        public decimal Original { get; }
    }
}
