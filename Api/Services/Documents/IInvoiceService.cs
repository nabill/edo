using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IInvoiceService
    {
        Task<DocumentRegistrationInfo> Register<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode, TInvoiceData data);

        Task Update(List<Invoice> invoices);


        Task<List<(DocumentRegistrationInfo Metadata, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode);

        Task<List<Invoice>> GetInvoices(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode);
    }
}