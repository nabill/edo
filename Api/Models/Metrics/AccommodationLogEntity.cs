namespace HappyTravel.Edo.Api.Models.Metrics
{
    public readonly struct AccommodationLogEntity
    {
        public AccommodationLogEntity(string id, string name, string counterpartyName)
        {
            Id = id;
            Name = name;
            CounterpartyName = counterpartyName;
        }


        public string Id { get; }
        public string CounterpartyName { get; }
        public string Name { get; }
    }
}
