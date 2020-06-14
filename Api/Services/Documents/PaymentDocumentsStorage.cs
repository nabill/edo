using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Documents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class PaymentDocumentsStorage : IPaymentDocumentsStorage
    {
        public PaymentDocumentsStorage(EdoContext context,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<DocumentRegistrationInfo> Register<TPaymentDocumentEntity>(TPaymentDocumentEntity documentEntity)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity
        {
            documentEntity.Date = _dateTimeProvider.UtcNow();
            _context.Add(documentEntity);
            await _context.SaveChangesAsync();
            
            return documentEntity.GetRegistrationInfo();
        }


        public Task<List<TPaymentDocumentEntity>> Get<TPaymentDocumentEntity>(ServiceTypes serviceType,
            ServiceSource serviceSource, string referenceCode)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity
        {
            return _context.Set<TPaymentDocumentEntity>()
                .Where(i => i.ParentReferenceCode == referenceCode &&
                    i.ServiceType == serviceType &&
                    i.ServiceSource == serviceSource)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }


        public async Task<Result<TPaymentDocumentEntity>> Get<TPaymentDocumentEntity>(int id)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity
        {
            var document = await _context.Set<TPaymentDocumentEntity>()
                .SingleOrDefaultAsync(d => d.Id == id);

            return document == default
                ? Result.Failure<TPaymentDocumentEntity>("Could not find document")
                : Result.Ok(document);
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}