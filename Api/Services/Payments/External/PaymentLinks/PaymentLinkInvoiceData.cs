using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public readonly struct PaymentLinkInvoiceData
    {
        [JsonConstructor]
        public PaymentLinkInvoiceData(MoneyAmount amount, ServiceTypes serviceType, string comment)
        {
            Amount = amount;
            ServiceType = serviceType;
            Comment = comment;
        }
        
        public MoneyAmount Amount { get; }
        public ServiceTypes ServiceType { get; }
        public string Comment { get; }
    }
}