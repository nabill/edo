using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Documents
{
    public class Receipt : IPaymentDocumentEntity
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public ServiceTypes ServiceType { get; set; }
        public ServiceSource ServiceSource { get; set; }
        public string ParentReferenceCode { get; set; } = string.Empty;
        public string? Data { get; set; }
        public DateTimeOffset Date { get; set; }
        public int InvoiceId { get; set; }
        public DocumentRegistrationInfo GetRegistrationInfo() => new DocumentRegistrationInfo(Number, Date);
    }
}