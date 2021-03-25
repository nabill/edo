using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class Counterparty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PaymentMethods PreferredPaymentMethod { get; set; }
        public CounterpartyStates State { get; set; }
        public DateTime Created { get; set; }
        public string VerificationReason { get; set; }
        public DateTime? Verified { get; set; }
        public DateTime Updated { get; set; }
        public string VatNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsContractUploaded { get; set; }
        public CounterpartyContractKind? ContractKind { get; set; }
    }
}