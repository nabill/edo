namespace HappyTravel.Edo.Api.Services.Payments.AuditEvents
{
    public readonly struct AccountBalanceLogEventData
    {
        public AccountBalanceLogEventData(string reason, decimal balance, decimal creditLimit, decimal frozen)
        {
            Reason = reason;
            Balance = balance;
            CreditLimit = creditLimit;
            Frozen = frozen;
        }


        public string Reason { get; }
        public decimal Balance { get; }
        public decimal CreditLimit { get; }
        public decimal Frozen { get; }
    }
}