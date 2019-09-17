namespace HappyTravel.Edo.Api.Services.Management.AuditEvents
{
    public readonly struct AccountCreditLimitChangeEvent
    {
        public AccountCreditLimitChangeEvent(int accountId, decimal creditLimitBefore, decimal creditLimitAfter)
        {
            AccountId = accountId;
            CreditLimitBefore = creditLimitBefore;
            CreditLimitAfter = creditLimitAfter;
        }
        
        public int AccountId { get; }
        public decimal CreditLimitBefore { get; }
        public decimal CreditLimitAfter { get; }
    }
}