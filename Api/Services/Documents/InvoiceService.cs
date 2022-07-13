using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService(IPaymentDocumentsStorage documentsStorage, IJsonSerializer serializer)
        {
            _documentsStorage = documentsStorage;
            _serializer = serializer;
        }


        public Task<DocumentRegistrationInfo> Register<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode,
            TInvoiceData data)
        {
            var invoice = new Invoice
            {
                ServiceSource = serviceSource,
                ServiceType = serviceType,
                ParentReferenceCode = referenceCode,
                Data = _serializer.SerializeObject(data)
            };
            
            return _documentsStorage
                .Register(invoice, (id, regDate) => $"INV-{id:000}/{regDate.Year}");
        }


        public Task Update(List<Invoice> invoices)
            => _documentsStorage.Update(invoices);


        public async Task<List<(DocumentRegistrationInfo Metadata, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode)
        {
            var invoices = await _documentsStorage.Get<Invoice>(serviceType, serviceSource, referenceCode);
            return invoices
                .Select(r => (r.GetRegistrationInfo(), _serializer.DeserializeObject<TInvoiceData>(r.Data)))
                .ToList();
        }


        public Task<List<Invoice>> GetInvoices(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode)
            => _documentsStorage.Get<Invoice>(serviceType, serviceSource, referenceCode);


        private readonly IPaymentDocumentsStorage _documentsStorage;
        private readonly IJsonSerializer _serializer;
    }
}