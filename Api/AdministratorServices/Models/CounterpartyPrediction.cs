namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct CounterpartyPrediction
    {
        public CounterpartyPrediction(int counterpartyId, string counterpartyName, string masterAgentName ,string billingEmail)
        {
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            BillingEmail = billingEmail;
            MasterAgentName = masterAgentName;
        }
        
        public int CounterpartyId { get; }
        public string CounterpartyName { get; }
        public string MasterAgentName { get; }
        public string BillingEmail { get; }
        
     
        
    }
}