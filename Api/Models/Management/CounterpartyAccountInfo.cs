using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyAccountInfo
    {
        public int Id { get; init; }
        public Currencies Currency { get; init; }
        public decimal Balance { get; init; }
    }
}