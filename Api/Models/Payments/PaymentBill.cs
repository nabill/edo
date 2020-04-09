using System;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentBill
    {
        public PaymentBill(string agentEmail, decimal amount, Currencies currency,
            DateTime date, PaymentMethods method, string referenceCode, string clientName = default)
        {
            AgentEmail = agentEmail;
            Amount = amount;
            Currency = currency;
            Date = date;
            Method = method;
            ReferenceCode = referenceCode;
            AgentName = clientName ?? string.Empty;
        }


        public string AgentEmail { get; }
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public DateTime Date { get; }
        public PaymentMethods Method { get; }
        public string ReferenceCode { get; }
        public string AgentName { get; }
    }
}