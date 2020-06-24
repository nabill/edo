using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinksDocumentsService : IPaymentLinksDocumentsService
    {
        public PaymentLinksDocumentsService(IInvoiceService invoiceService, IReceiptService receiptService)
        {
            _invoiceService = invoiceService;
            _receiptService = receiptService;
        }


        public Task GenerateInvoice(PaymentLinkData link)
        {
            var amount = new MoneyAmount(link.Amount, link.Currency);
            var invoice = new PaymentLinkInvoiceData(amount, link.ServiceType, link.Comment);
            return _invoiceService.Register(link.ServiceType, ServiceSource.PaymentLinks, link.ReferenceCode, invoice);
        }


        public async Task<(DocumentRegistrationInfo RegistrationInfo, PaymentLinkInvoiceData Data)> GetInvoice(PaymentLinkData link)
            => (await _invoiceService
                    .Get<PaymentLinkInvoiceData>(link.ServiceType, ServiceSource.PaymentLinks, link.ReferenceCode))
                .Single();


        public async Task<Result<(DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data)>> GenerateReceipt(PaymentLinkData link)
        {
            var (invoiceInfo, _) = await GetInvoice(link);

            var receipt = new PaymentReceipt(
                link.Amount,
                link.Currency,
                PaymentMethods.CreditCard,
                link.ReferenceCode);

            var (_, isFailure, regInfo, error) = await _receiptService.Register(invoiceInfo.Number, receipt);
            if (isFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(error);

            return (regInfo, receipt);
        }


        private readonly IInvoiceService _invoiceService;
        private readonly IReceiptService _receiptService;
    }
}