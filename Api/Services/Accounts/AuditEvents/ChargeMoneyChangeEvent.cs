namespace HappyTravel.Edo.Api.Services.Accounts.AuditEvents
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
