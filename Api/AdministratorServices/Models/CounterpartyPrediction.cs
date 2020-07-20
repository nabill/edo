namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct CounterpartyPrediction
    {
        public CounterpartyPrediction(int counterpartyId, string counterpartyName, string masterAgentName, string billingEmail)
        {
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            BillingEmail = billingEmail;
            MasterAgentName = masterAgentName;
        }


        /// <summary>
        /// Counterparty Id.
        /// </summary>
        public int CounterpartyId { get; }

        /// <summary>
        /// Counterparty name.
        /// </summary>
        public string CounterpartyName { get; }

        /// <summary>
        /// Master agent name.
        /// </summary>
        public string MasterAgentName { get; }

        /// <summary>
        /// Billing email of counterparty or email of master agent.
        /// </summary>
        public string BillingEmail { get; }
    }
}