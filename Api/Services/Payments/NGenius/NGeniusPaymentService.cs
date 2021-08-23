using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusPaymentService
    {
        public NGeniusPaymentService(EdoContext context, IDateTimeProvider dateTimeProvider, IBookingRecordManager bookingRecordManager, NGeniusClient client, 
            ICreditCardsManagementService creditCardsManagementService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _bookingRecordManager = bookingRecordManager;
            _client = client;
            _creditCardsManagementService = creditCardsManagementService;
        }


        public async Task<Result<NGeniusPaymentResponse>> Authorize(NewCreditCardRequest request, string languageCode, string ipAddress, IPaymentCallbackService paymentCallbackService, AgentContext agent)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(request.ReferenceCode);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);

            return await CreateOrderRequest(OrderTypes.Auth, request.ReferenceCode, booking.Currency, booking.TotalPrice, agent, request.Card)
                .Bind(r => _client.CreateOrder(r))
                .Bind(StorePaymentResults);

            // TODO: Firstly enable tokenization on NGenius side, then implement storing card

            async Task<Result<NGeniusPaymentResponse>> StorePaymentResults(NGeniusPaymentResponse paymentResult)
            {
                var payment = await CreatePayment(ipAddress, booking.TotalPrice.ToMoneyAmount(booking.Currency), null, paymentResult);
                var (_, isFailure, error) = await paymentCallbackService.ProcessPaymentChanges(payment);

                return isFailure
                    ? Result.Failure<NGeniusPaymentResponse>(error)
                    : Result.Success(paymentResult);
            }
        }


        public async Task<Result<NGeniusPaymentResponse>> Authorize(SavedCreditCardRequest request, string languageCode, string ipAddress,
            IPaymentCallbackService paymentCallbackService, AgentContext agent)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(request.ReferenceCode);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);
            
            var creditCard = await _creditCardsManagementService.Get(request.CardId, agent);
            if (creditCard.IsFailure)
                return Result.Failure<NGeniusPaymentResponse>(creditCard.Error);
            
            var savedCard = new SavedCard
            {
                CardToken = creditCard.Value.Token,
                CardHolderName = creditCard.Value.HolderName,
                Expiry = creditCard.Value.ExpirationDate,
                MaskedPan = creditCard.Value.MaskedNumber,
                Scheme = string.Empty, // TODO: add new property to credit card
                Cvv = request.Cvv,
                RecaptureCsc = false
            };

            return await CreateOrderRequest(OrderTypes.Auth, booking.ReferenceCode, booking.Currency, booking.TotalPrice, agent, savedCard: savedCard)
                .Bind(r => _client.CreateOrder(r))
                .Bind(StorePaymentResults);


            async Task<Result<NGeniusPaymentResponse>> StorePaymentResults(NGeniusPaymentResponse paymentResult)
            {
                var payment = await CreatePayment(ipAddress, booking.TotalPrice.ToMoneyAmount(booking.Currency), request.CardId, paymentResult);
                var (_, isFailure, error) = await paymentCallbackService.ProcessPaymentChanges(payment);

                return isFailure
                    ? Result.Failure<NGeniusPaymentResponse>(error)
                    : Result.Success(paymentResult);
            }
        }
        
        
        public async Task<Result<NGeniusPaymentResponse>> Pay(NewCreditCardRequest request, string languageCode, string ipAddress, IPaymentCallbackService paymentCallbackService, AgentContext agent)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(request.ReferenceCode);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);

            return await CreateOrderRequest(OrderTypes.Sale, request.ReferenceCode, booking.Currency, booking.TotalPrice, agent, request.Card)
                .Bind(r => _client.CreateOrder(r))
                .Bind(StorePaymentResults);


            async Task<Result<NGeniusPaymentResponse>> StorePaymentResults(NGeniusPaymentResponse paymentResult)
            {
                var payment = await CreatePayment(ipAddress, booking.TotalPrice.ToMoneyAmount(booking.Currency), null, paymentResult);
                var (_, isFailure, error) = await paymentCallbackService.ProcessPaymentChanges(payment);

                return isFailure
                    ? Result.Failure<NGeniusPaymentResponse>(error)
                    : Result.Success(paymentResult);
            }
        }


        private static int ToNGeniusAmount(MoneyAmount moneyAmount) 
            => decimal.ToInt32(moneyAmount.Amount * (decimal)Math.Pow(10, moneyAmount.Currency.GetDecimalDigitsCount()));


        private async Task<Edo.Data.Payments.Payment> CreatePayment(string ipAddress, MoneyAmount price, int? cardId, NGeniusPaymentResponse paymentResult)
        {
            var now = _dateTimeProvider.UtcNow();

            var info = new CreditCardPaymentInfo(customerIp: ipAddress, 
                externalId: paymentResult.PaymentId,
                message: string.Empty, 
                authorizationCode: string.Empty, 
                expirationDate: paymentResult.Payment.Expiry,
                internalReferenceCode: paymentResult.OrderReference);

            var payment = new Edo.Data.Payments.Payment
            {
                Amount = price.Amount,
                Currency = price.Currency.ToString(),
                AccountNumber = paymentResult.Payment.Pan,
                Created = now,
                Modified = now,
                Status = paymentResult.Status.ToPaymentStatus(),
                Data = JsonConvert.SerializeObject(info),
                AccountId = cardId,
                PaymentMethod = PaymentTypes.CreditCard,
                PaymentProcessor = PaymentProcessors.NGenius,
                ReferenceCode = paymentResult.MerchantOrderReference
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }


        private static Result<OrderRequest> CreateOrderRequest(string orderType, string referenceCode, Currencies currency, decimal price, AgentContext agent, Payment? card = null, SavedCard? savedCard = null)
        {
            return new OrderRequest
            {
                Order = new Order
                {
                    Action = orderType,
                    Amount = new NGeniusAmount
                    {
                        CurrencyCode = currency.ToString(),
                        Value = ToNGeniusAmount(price.ToMoneyAmount(currency))
                    },
                    MerchantOrderReference = referenceCode,
                    BillingAddress = new NGeniusBillingAddress
                    {
                        FirstName = agent.FirstName,
                        LastName = agent.LastName
                    },
                    Email = agent.Email
                },
                Payment = card,
                SavedCard = savedCard
            };
        }
        
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly ICreditCardsManagementService _creditCardsManagementService;
        private readonly NGeniusClient _client;
    }
}