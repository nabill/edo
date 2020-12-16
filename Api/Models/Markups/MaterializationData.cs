namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MaterializationData
    {
        public int PolicyId { get; init; }
        public string ReferenceCode { get; init; }
        public int AgencyAccountId { get; init; }
        public decimal Amount { get; init; }
    }
}