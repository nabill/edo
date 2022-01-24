using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks
{
    public class PaymentLinkCreationRequest
    {
        [JsonConstructor]
        public PaymentLinkCreationRequest(decimal amount, string email, ServiceTypes serviceType, Currencies currency, string comment,
            PaymentProcessors? paymentProcessor = null, string invoiceNumber = null)
        {
            Amount = amount;
            Email = email;
            ServiceType = serviceType;
            Currency = currency;
            Comment = comment;
            PaymentProcessor = paymentProcessor;
            InvoiceNumber = invoiceNumber;
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
        ///     Mandatory payment comment.
        /// </summary>
        [Required]
        public string Comment { get; }

        /// <summary>
        /// Payment processor to use for payment link. Default will be used if not set
        /// </summary>
        public PaymentProcessors? PaymentProcessor { get; }

        
        /// <summary>
        /// Optional invoice number to attach to payment link.
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string InvoiceNumber { get; }
    }
}