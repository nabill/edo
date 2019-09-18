namespace HappyTravel.Edo.Api.Services.Payments.AuditEvents
{
    public readonly struct AddMoneyChangeEvent
    {
        public AddMoneyChangeEvent(int accountId, decimal amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public int AccountId { get; }
        public decimal Amount { get; }
    }
}
