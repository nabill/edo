using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct CreditCardRequirement
    {
        [JsonConstructor]
        public CreditCardRequirement(DateTime activationDate, DateTime dueDate)
        {
            ActivationDate = activationDate;
            DueDate = dueDate;
        }
        
        public DateTime ActivationDate { get; }
        public DateTime DueDate { get; }
    }
}