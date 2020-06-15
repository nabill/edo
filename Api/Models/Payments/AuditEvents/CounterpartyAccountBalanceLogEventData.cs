using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.AuditEvents
{
    public readonly struct CounterpartyAccountBalanceLogEventData
    {
        [JsonConstructor]
        public CounterpartyAccountBalanceLogEventData(string reason, decimal balance)
        {
            Reason = reason;
            Balance = balance;
        }


        public string Reason { get; }
        public decimal Balance { get; }
    }
}