using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkOptions
    {
        public string MailTemplateId { get; set; }
        public ClientSettings ClientSettings { get; set; }
        public List<Version> SupportedVersions { get; set; }
        public Uri PaymentUrlPrefix { get; set; }
    }
}