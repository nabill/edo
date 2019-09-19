namespace HappyTravel.Edo.Api.Services.Payments.AuditEvents
{
    public readonly struct AccountBalanceLogEventData
    {
        public AccountBalanceLogEventData(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}
