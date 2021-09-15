﻿using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusPaymentService
    {
        Task<Result<NGeniusPaymentResponse>> Authorize(string referenceCode, string ipAddress, AgentContext agent);

        Task<Result<NGeniusPaymentResponse>> Pay(string referenceCode, string ipAddress, string email, NGeniusBillingAddress billingAddress);

        Task<Result<CreditCardCaptureResult>> Capture(string paymentId, string orderReference, MoneyAmount amount);

        Task<Result<CreditCardVoidResult>> Void(string paymentId, string orderReference);
        
        Task<Result<CreditCardRefundResult>> Refund(string paymentId, string orderReference, string captureId, MoneyAmount amount);

        Task ProcessWebHook(JsonDocument request);
    }
}