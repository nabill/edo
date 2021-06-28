namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class CounterpartyIsActiveStatusChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string CounterpartyName { get; set; }
        public string Status { get; set; }
    }
}