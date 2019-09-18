namespace HappyTravel.Edo.Api.Services.Payments.AuditEvents
{
    public readonly struct ChargeMoneyChangeEvent
    {
        public ChargeMoneyChangeEvent(int accountId, decimal amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public int AccountId { get; }
        public decimal Amount { get; }
    }
}
