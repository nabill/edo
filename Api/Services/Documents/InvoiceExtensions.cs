using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public static class InvoiceExtensions
    {
        public static DocumentRegistrationInfo GetRegistrationInfo(this Invoice invoice) => new DocumentRegistrationInfo(invoice.Id, invoice.Date);
    }
}