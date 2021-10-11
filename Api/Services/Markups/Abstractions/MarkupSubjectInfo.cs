namespace HappyTravel.Edo.Api.Services.Markups.Abstractions
{
    public readonly struct MarkupSubjectInfo
    {
        public string CountryHtId { get; init; }
        public string LocalityHtId { get; init;}
        public int CounterpartyId { get; init;}
        public int AgencyId { get; init;}
        public int AgentId { get; init;}
    }
}