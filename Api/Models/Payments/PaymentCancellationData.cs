using System.ComponentModel.DataAnnotations;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentCancellationData
    {
        [JsonConstructor]
        public PaymentCancellationData(decimal amount, Currencies currency)
        {
            Amount = amount;
            Currency = currency;
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
    }
}