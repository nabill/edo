using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class ReceiptService : IReceiptService
    {
        public ReceiptService(IPaymentDocumentsStorage documentsStorage)
        {
            _documentsStorage = documentsStorage;
        }


        public Task<DocumentRegistrationInfo> Register<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode,
            TInvoiceData data)
            => _documentsStorage
                .Register<TInvoiceData, Receipt>(serviceType, serviceSource, referenceCode, data);


        public Task<List<(DocumentRegistrationInfo Metadata, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode)
            => _documentsStorage.Get<TInvoiceData, Receipt>(serviceType, serviceSource, referenceCode);


        private readonly IPaymentDocumentsStorage _documentsStorage;
    }
}