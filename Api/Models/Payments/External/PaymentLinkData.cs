using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External
{
    public readonly struct PaymentLinkData
    {
        [JsonConstructor]
        public PaymentLinkData(decimal price, ServiceTypes serviceType, Currencies currency, string comment)
        {
            Price = price;
            ServiceType = serviceType;
            Currency = currency;
            Comment = comment;
        }


        /// <summary>
        ///     Payment price.
        /// </summary>
        [Required]
        public decimal Price { get; }

        /// <summary>
        /// Service type to pay for.
        /// </summary>
        public ServiceTypes ServiceType { get; }


        /// <summary>
        ///     Payment currency.
        /// </summary>
        [Required]
        public Currencies Currency { get; }

        /// <summary>
        ///     Optional payment comment.
        /// </summary>
        public string Comment { get; }
    }
}