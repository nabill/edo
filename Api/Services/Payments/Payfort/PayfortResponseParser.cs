using System;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.Payfort
{
    public class PayfortResponseParser : IPayfortResponseParser
    {
        public PayfortResponseParser(IPayfortSignatureService signatureService)
        {
            _signatureService = signatureService;
        }


        public Result<CreditCardPaymentResult> ParsePaymentResponse(JObject response)
        {
            return ParseResponse<PayfortPaymentResponse>(response)
                .OnSuccess(CheckResponseSignature)
                .OnSuccess(CreateResult);

            Result<PayfortPaymentResponse> CheckResponseSignature(PayfortPaymentResponse model) => _signatureService.Check(response, model);


            Result<CreditCardPaymentResult> CreateResult(PayfortPaymentResponse model)
            {
                var (_, isFailure, amount, error) = FromPayfortAmount(model.Amount, model.Currency);
                if (isFailure)
                    return Result.Fail<CreditCardPaymentResult>(error);

                return Result.Ok(new CreditCardPaymentResult(
                    referenceCode: model.SettlementReference,
                    secure3d: model.Secure3d,
                    authorizationCode: model.AuthorizationCode,
                    externalCode: model.FortId,
                    expirationDate: model.ExpirationDate,
                    cardNumber: model.CardNumber,
                    status: GetStatus(model),
                    message: $"{model.ResponseCode}: {model.ResponseMessage}",
                    amount: amount,
                    merchantReference: model.MerchantReference));


                CreditCardPaymentStatuses GetStatus(PayfortPaymentResponse payment)
                {
                    switch (payment.ResponseCode)
                    {
                        case PayfortConstants.PaymentSuccessResponseCode:
                        case PayfortConstants.AuthorizationSuccessResponseCode: return CreditCardPaymentStatuses.Success;
                        case PayfortConstants.PaymentSecure3dResponseCode: return CreditCardPaymentStatuses.Secure3d;
                        default: return CreditCardPaymentStatuses.Failed;
                    }
                }
            }
        }


        public Result<T> ParseResponse<T>(JObject response)
        {
            var model = response.ToObject<T>(PayfortSerializationSettings.Serializer);
            return model == null
                ? Result.Fail<T>($"Invalid payfort payment response: '{response}'")
                : Result.Ok(model);
        }


        private static Result<decimal> FromPayfortAmount(string amountString, string currencyString)
        {
            if (!Enum.TryParse<Currencies>(currencyString, out var currency))
                return Result.Fail<decimal>($"Invalid currency in response: {currencyString}");

            if (!decimal.TryParse(amountString, out var amount))
                return Result.Fail<decimal>("");

            var result = amount / PayfortConstants.ExponentMultipliers[currency];
            return Result.Ok(result);
        }


        private readonly IPayfortSignatureService _signatureService;
    }
}