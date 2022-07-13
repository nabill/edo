using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IPaymentDocumentsStorage
    {
        Task<DocumentRegistrationInfo> Register<TPaymentDocumentEntity>(TPaymentDocumentEntity documentEntity, Func<int, DateTimeOffset, string> numberGenerator)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity;


        Task Update<TPaymentDocumentEntity>(List<TPaymentDocumentEntity> documentEntities)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity;


        public Task<List<TPaymentDocumentEntity>> Get<TPaymentDocumentEntity>(ServiceTypes serviceType,
            ServiceSource serviceSource, string referenceCode)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity;


        Task<Result<TPaymentDocumentEntity>> Get<TPaymentDocumentEntity>(string number)
            where TPaymentDocumentEntity : class, IPaymentDocumentEntity;
    }
}