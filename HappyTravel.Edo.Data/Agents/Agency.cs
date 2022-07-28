using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Data.Agents
{
    public class Agency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Fax { get; set; }
        public string? PostalCode { get; set; }
        public Currencies PreferredCurrency { get; set; }
        public string? VatNumber { get; set; }
        public string? BillingEmail { get; set; }
        public string? Website { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public DateTimeOffset? Verified { get; set; }
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
        public List<int> Ancestors { get; set; } = new();
        public string? CountryHtId { get; set; }
        public string? LocalityHtId { get; set; }
        public ContractKind? ContractKind { get; set; }
        public string? VerificationReason { get; set; }
        public AgencyVerificationStates VerificationState { get; set; }
        public string LegalAddress { get; set; } = string.Empty;
        public PaymentTypes PreferredPaymentMethod { get; set; }
        public bool IsContractUploaded { get; set; }
        public int? AccountManagerId { get; set; }
        public MoneyAmount? CreditLimit { get; set; }
    }
}