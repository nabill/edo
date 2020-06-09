using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IInvoiceService
    {
        Task<InvoiceRegistrationInfo> Register<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode, TInvoiceData data);

        Task<List<(InvoiceRegistrationInfo Metadata, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode);
    }
}