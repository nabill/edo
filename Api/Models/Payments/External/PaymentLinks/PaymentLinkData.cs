using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks
{
    public readonly struct PaymentLinkData
    {
        [JsonConstructor]
        public PaymentLinkData(decimal amount, string email, ServiceTypes serviceType, Currencies currency, string comment, string referenceCode,
            CreditCardPaymentStatuses creditCardPaymentStatus, string code, DateTime date, PaymentProcessors? paymentProcessor)
        {
            Amount = amount;
            Email = email;
            ServiceType = serviceType;
            Currency = currency;
            Comment = comment;
            ReferenceCode = referenceCode;
            CreditCardPaymentStatus = creditCardPaymentStatus;
            Code = code;
            Date = date;
            PaymentProcessor = paymentProcessor;
        }


        /// <summary>
        ///     Payment price.
        /// </summary>
        [Required]
        public decimal Amount { get; }

        /// <summary>
        ///     Customer e-mail.
        /// </summary>
        [Required]
        public string Email { get; }

        /// <summary>
        ///     Service type to pay for.
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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; }

        /// <summary>
        ///     Link reference code.
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     Payment status.
        /// </summary>
        public CreditCardPaymentStatuses CreditCardPaymentStatus { get; }

        /// <summary>
        ///     Link unique code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     Date when the link was created
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Payment processor to use for payment link. Default will be used if not set
        /// </summary>
        public PaymentProcessors? PaymentProcessor { get; }
    }
}