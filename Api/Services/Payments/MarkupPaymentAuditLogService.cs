using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class MarkupPaymentAuditLogService : IMarkupPaymentAuditLogService
    {
        public MarkupPaymentAuditLogService(EdoContext context)
        {
            _context = context;
        }

        public async Task Write(MarkupPaymentLog paymentLog)
        {
            _context.MarkupPaymentLogs.Add(paymentLog);
            await _context.SaveChangesAsync();
        }


        private readonly EdoContext _context;
    }
}