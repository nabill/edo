using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.AuditEvents
{
    public readonly struct AccountBalanceLogEventData
    {
        [JsonConstructor]
        public AccountBalanceLogEventData(string reason, decimal balance)
        {
            Reason = reason;
            Balance = balance;
        }


        public string Reason { get; }
        public decimal Balance { get; }
    }
}