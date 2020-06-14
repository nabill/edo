using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Documents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class PaymentDocumentsStorage : IPaymentDocumentsStorage
    {
        public PaymentDocumentsStorage(EdoContext context,
            IJsonSerializer jsonSerializer,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _jsonSerializer = jsonSerializer;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<DocumentRegistrationInfo> Register<TDocumentData, TPaymentDocumentEntity>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode, TDocumentData data)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity, new()
        {
            var date = _dateTimeProvider.UtcNow().Date;
            var document = new TPaymentDocumentEntity
            {
                ServiceType = serviceType,
                ServiceSource = serviceSource,
                ParentReferenceCode = referenceCode,
                Date = date,
                Data = _jsonSerializer.SerializeObject(data)
            };

            _context.Add(document);

            await _context.SaveChangesAsync();
            return document.GetRegistrationInfo();
        }


        public async Task<List<(DocumentRegistrationInfo Metadata, TDocumentData Data)>> Get<TDocumentData, TPaymentDocumentEntity>(ServiceTypes serviceType,
            ServiceSource serviceSource, string referenceCode)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity, new()
        {
            var documents = await _context.Set<TPaymentDocumentEntity>()
                .Where(i => i.ParentReferenceCode == referenceCode &&
                    i.ServiceType == serviceType &&
                    i.ServiceSource == serviceSource)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return documents
                .Select(i => (i.GetRegistrationInfo(), _jsonSerializer.DeserializeObject<TDocumentData>(i.Data)))
                .ToList();
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IJsonSerializer _jsonSerializer;
    }
}