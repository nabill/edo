using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IPaymentDocumentsStorage
    {
        Task<DocumentRegistrationInfo> Register<TDocumentData, TPaymentDocumentEntity>(ServiceTypes serviceType, ServiceSource serviceSource,
            string referenceCode, TDocumentData data)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity, new();


        Task<List<(DocumentRegistrationInfo Metadata, TDocumentData Data)>> Get<TDocumentData, TPaymentDocument>(ServiceTypes serviceType,
            ServiceSource serviceSource, string referenceCode)
            where TPaymentDocument : class, IPaymentDocumentEntity, new();
    }
}