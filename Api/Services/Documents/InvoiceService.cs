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
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService(EdoContext context,
            IJsonSerializer jsonSerializer,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _jsonSerializer = jsonSerializer;
            _dateTimeProvider = dateTimeProvider;
        }
        
        
        public async Task<DocumentRegistrationInfo> Register<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode, TInvoiceData data)
        {
            var date = _dateTimeProvider.UtcNow().Date;
            var invoice = new Invoice
            {
                ServiceType = serviceType,
                ServiceSource = serviceSource,
                ParentReferenceCode = referenceCode,
                Date = date,
                Data = _jsonSerializer.SerializeObject(data),
            };
            
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();
            return invoice.GetRegistrationInfo();
        }


        public async Task<List<(DocumentRegistrationInfo Metadata, TInvoiceData Data)>> Get<TInvoiceData>(ServiceTypes serviceType, ServiceSource serviceSource, string referenceCode)
        {
            var invoices = await _context.Invoices
                .Where(i => i.ParentReferenceCode == referenceCode &&
                    i.ServiceType == serviceType &&
                    i.ServiceSource == serviceSource)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return invoices
                .Select(i => (i.GetRegistrationInfo(), _jsonSerializer.DeserializeObject<TInvoiceData>(i.Data)))
                .ToList();
        }
        
        
        private readonly EdoContext _context;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}