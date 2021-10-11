namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class CounterpartyVerificationStateChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string CounterpartyName { get; set; }
        public string State { get; set; }
    }
}