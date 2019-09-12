using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(EdoContext context, ICustomerContext customerContext, IPayfortService payfortService, ICreditCardService cardService)
        {
            _context = context;
            _customerContext = customerContext;
            _payfortService = payfortService;
            _cardService = cardService;
        }

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);

        public async Task<Result<PaymentResponse>> PayWithNewCreditCard(PaymentWithNewCreditCardRequest  request, string languageCode, string ipAddress)
        {
            var (_, isValidationFailure, validationError) = await Validate(request);
            if (isValidationFailure)
                return Result.Fail<PaymentResponse>(validationError);

            var (_, isFailure, token, error) = await _payfortService.Tokenize(
                new TokenizationRequest(request.Number, request.HolderName, request.SecurityCode, request.ExpirationDate, request.RememberMe, languageCode));
            if (isFailure)
                return Result.Fail<PaymentResponse>(error);

            var (_, _, customer, _) = await _customerContext.GetCustomer();
            if (request.RememberMe)
                await _cardService.Create(new CreditCard()
                {
                    ExpirationDate = token.ExpirationDate,
                    HolderName = token.CardHolderName,
                    Number = token.CardNumber,
                    Token = token.TokenName,
                    CustomerId = customer.Id
                });

            return await MakePayment(new PaymentRequest(request.Amount, request.Currency, request.SecurityCode, token.TokenName, request.RememberMe, $"{customer.FirstName} {customer.LastName}",
                customer.Email, ipAddress, request.ReferenceCode, languageCode));
        }

        public async Task<Result<PaymentResponse>> PayWithExistingCard(PaymentWithExistingCreditCardRequest request, string languageCode, string ipAddress)
        {
            var (_, isFailure, error) = await Validate(request);
            if (isFailure)
                return Result.Fail<PaymentResponse>(error);

            var (_, _, customer, _) = await _customerContext.GetCustomer();
            var card = await _context.CreditCards.FindAsync(request.CardId);
            return await MakePayment(new PaymentRequest(request.Amount, request.Currency, request.SecurityCode, card.Token, true, $"{customer.FirstName} {customer.LastName}",
                customer.Email, ipAddress, request.ReferenceCode, languageCode));
        }

        private async Task<Result<PaymentResponse>> MakePayment(PaymentRequest request)
        {
            var (_, isFailure, payment, error) = await _payfortService.Pay(request);
            if (isFailure)
                return Result.Fail<PaymentResponse>(error);
            var booking = await _context.Bookings.FirstAsync(b => b.ReferenceCode == request.ReferenceCode);
            _context.Payments.Add(new Payment()
            {
                Amount = request.Amount,
                BookingId = booking.Id,
                CustomerIp = request.CustomerIp,
                CardHolderName = payment.CardHolderName,
                CardNumber = payment.CardNumber,
                Currency = request.Currency,
                Created = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return Result.Ok(new PaymentResponse(payment.Secure3d));
        }

        private async Task<Result> Validate(PaymentWithNewCreditCardRequest  request)
        {
            var fieldValidateResult = GenericValidator<PaymentWithNewCreditCardRequest>.Validate(v =>
            {
                v.RuleFor(c => c.Amount).NotEmpty();
                v.RuleFor(c => c.Currency).NotEmpty().IsInEnum();
                v.RuleFor(c => c.HolderName).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
                v.RuleFor(c => c.Number).NotEmpty();
                v.RuleFor(c => c.ExpirationDate).NotEmpty();
            }, request);

            if (fieldValidateResult.IsFailure)
                return  fieldValidateResult;

            return Result.Combine(await CheckReferenceCode(request.ReferenceCode));
        }

        private async Task<Result> Validate(PaymentWithExistingCreditCardRequest request)
        {
            var fieldValidateResult = GenericValidator<PaymentWithExistingCreditCardRequest>.Validate(v =>
            {
                v.RuleFor(c => c.Amount).NotEmpty();
                v.RuleFor(c => c.Currency).NotEmpty().IsInEnum();
                v.RuleFor(c => c.CardId).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
            }, request);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return Result.Combine(await _cardService.CanUseCard(request.CardId),
                                  await CheckReferenceCode(request.ReferenceCode));
        }

        private async Task<Result> CheckReferenceCode(string referenceCode)
        {
            var booking = await _context.Bookings.Where(b => b.ReferenceCode == referenceCode).FirstOrDefaultAsync();
            if (booking == null)
                return Result.Fail("Invalid Reference code");
            
            if (InvalidBookingStatuses.Contains(booking.Status))
                return Result.Fail($"Invalid booking status: {booking.Status.ToString()}");
            
            return Result.Ok();
        }

        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();

        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();

        private static readonly BookingStatusCodes[] InvalidBookingStatuses = new[]
            {BookingStatusCodes.Cancelled, BookingStatusCodes.Invalid, BookingStatusCodes.Rejected};

        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IPayfortService _payfortService;
        private readonly ICreditCardService _cardService;
    }
}
