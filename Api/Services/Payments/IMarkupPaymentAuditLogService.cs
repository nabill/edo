using System.Threading.Tasks;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IMarkupPaymentAuditLogService
    {
        public Task Write(MarkupPaymentLog paymentLog);
    }
}