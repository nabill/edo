using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public static class InvoiceExtensions
    {
        public static InvoiceRegistrationInfo GetMetadata(this Invoice invoice) => new InvoiceRegistrationInfo(invoice.Id, invoice.Date);
    }
}