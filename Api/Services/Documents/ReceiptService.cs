using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class ReceiptService : IReceiptService
    {
        public ReceiptService(IPaymentDocumentsStorage documentsStorage, IJsonSerializer serializer)
        {
            _documentsStorage = documentsStorage;
            _serializer = serializer;
        }


        public async Task<Result<DocumentRegistrationInfo>> Register<TReceiptData>(string invoiceNumber,
            TReceiptData data)
        {
            var (_, isFailure, invoice, error) = await _documentsStorage.Get<Invoice>(invoiceNumber);
            if (isFailure)
                return Result.Failure<DocumentRegistrationInfo>(error);
            
            var receipt = new Receipt
            {
                Data = _serializer.SerializeObject(data),
                InvoiceId = invoice.Id,
                ServiceSource = invoice.ServiceSource,
                ServiceType = invoice.ServiceType,
                ParentReferenceCode = invoice.ParentReferenceCode
            };
            
            return await _documentsStorage
                .Register(receipt, (id, regDate) => $"R{id:000}/{regDate.Year}" );
        }


        public async Task<List<(DocumentRegistrationInfo RegistrationInfo, TReceiptData Data)>> Get<TReceiptData>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode)
        {
            var receipts = await _documentsStorage.Get<Receipt>(serviceType, serviceSource, referenceCode);
            return receipts
                .Select(r => (r.GetRegistrationInfo(), _serializer.DeserializeObject<TReceiptData>(r.Data)))
                .ToList();
        }


        private readonly IPaymentDocumentsStorage _documentsStorage;
        private readonly IJsonSerializer _serializer;
    }
}