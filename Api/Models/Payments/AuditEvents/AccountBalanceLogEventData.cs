using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.AuditEvents
{
    public readonly struct AccountBalanceLogEventData
    {
        [JsonConstructor]
        public AccountBalanceLogEventData(string reason, decimal balance, int? transferPayerAccountId = null, int? transferRecipientAccountId = null)
        {
            Reason = reason;
            Balance = balance;
            TransferPayerAccountId = transferPayerAccountId;
            TransferRecipientAccountId = transferRecipientAccountId;
        }


        public string Reason { get; }
        public decimal Balance { get; }
        public int? TransferPayerAccountId { get; }
        public int? TransferRecipientAccountId { get; }
    }
}