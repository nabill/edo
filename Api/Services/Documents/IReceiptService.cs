using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IReceiptService
    {
        Task<Result<DocumentRegistrationInfo>> Register<TReceiptData>(string invoiceNumber,
            TReceiptData data);


        Task<List<(DocumentRegistrationInfo RegistrationInfo, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode);
    }
}