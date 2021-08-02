using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyAccountInfo
    {
        public int Id { get; init; }
        public Currencies Currency { get; init; }
        public MoneyAmount Balance { get; init; }
        public bool IsActive { get; init; }
    }
}