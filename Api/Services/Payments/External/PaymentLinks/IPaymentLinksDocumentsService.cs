using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.Data.PaymentLinks;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public interface IPaymentLinksDocumentsService
    {
        Task GenerateInvoice(PaymentLink link);

        Task<(DocumentRegistrationInfo RegistrationInfo, PaymentLinkInvoiceData Data)> GetInvoice(PaymentLinkData link);

        Task<Result<(DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data)>> GenerateReceipt(PaymentLinkData link);
    }
}