using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External
{
    public readonly struct PaymentLinkData
    {
        [JsonConstructor]
        public PaymentLinkData(decimal price, string facility, string currency, string comment)
        {
            Price = price;
            Facility = facility;
            Currency = currency;
            Comment = comment;
        }


        /// <summary>
        ///     Payment price.
        /// </summary>
        [Required]
        public decimal Price { get; }

        /// <summary>
        ///     Facility to pay for.
        /// </summary>
        [Required]
        public string Facility { get; }

        /// <summary>
        ///     Payment currency.
        /// </summary>
        [Required]
        public string Currency { get; }

        /// <summary>
        ///     Optional payment comment.
        /// </summary>
        public string Comment { get; }
    }
}