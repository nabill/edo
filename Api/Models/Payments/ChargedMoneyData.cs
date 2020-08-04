using System.ComponentModel.DataAnnotations;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct ChargedMoneyData
    {
        [JsonConstructor]
        public ChargedMoneyData(decimal amount, Currencies currency, string referenceCode, string reason)
        {
            Amount = amount;
            Currency = currency;
            ReferenceCode = referenceCode;
            Reason = reason;
        }


        /// <summary>
        ///     Money amount.
        /// </summary>
        [Required]
        public decimal Amount { get; }

        /// <summary>
        ///     Payment currency.
        /// </summary>
        [Required]
        public Currencies Currency { get; }

        /// <summary>
        ///     Reference code.
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     Reason for payment.
        /// </summary>
        [Required]
        public string Reason { get; }
    }
}