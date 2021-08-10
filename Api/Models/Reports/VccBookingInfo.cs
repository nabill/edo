using System;
using System.Text.Json.Serialization;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct VccBookingInfo
    {
        [JsonConstructor]
        public VccBookingInfo(string transactionId, string referenceCode, decimal amount, Currencies currency, DateTime activationDate, DateTime dueDate, string clientId, string cardNumber)
        {
            TransactionId = transactionId;
            ReferenceCode = referenceCode;
            Amount = amount;
            Currency = currency;
            ActivationDate = activationDate;
            DueDate = dueDate;
            ClientId = clientId;
            CardNumber = cardNumber;
        }
        
        
        public string TransactionId { get; }
        public string ReferenceCode { get; }
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public DateTime ActivationDate { get; }
        public DateTime DueDate { get; }
        public string ClientId { get; }
        public string CardNumber { get; }
    }
}