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

        public PaymentService(EdoContext context, ICustomerContext customerContext, IPayfortService payfortService)
        {
            _context = context;
            _customerContext = customerContext;
            _payfortService = payfortService;
        }

        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();

        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();
        private readonly ICustomerContext _customerContext;
        private readonly IPayfortService _payfortService;

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);

        public async Task<Result<List<CardInfo>>> GetAvailableCards()
        {
            var (_, companyFailture, company, companyError) = await _customerContext.GetCompany();
            if (!companyFailture)
                return Result.Fail<List<CardInfo>>(companyError);

            var (_, customerFailture, customer, customerError) = await _customerContext.GetCompany();
            if (!customerFailture)
                return Result.Fail<List<CardInfo>>(customerError);

            var query = from card in _context.Cards
                join compCard in _context.CompanyCardRelations on card.Id equals compCard.CardId into companyCards
                from companyCard in companyCards.DefaultIfEmpty()
                join cusCard in _context.CustomerCardRelations on card.Id equals cusCard.CardId into customerCards
                from customerCard in customerCards.DefaultIfEmpty()
                where companyCard.CompanyId == company.Id || customerCard.CustomerId == customer.Id
                select ToCardInfo(card, companyCard != null ? CardOwner.Company : CardOwner.Customer);

            var cards = await query.ToListAsync();

            return Result.Ok(cards);
        }

        static CardInfo ToCardInfo(Card card, CardOwner owner) =>
            new CardInfo()
            {
                HolderName = card.HolderName,
                Number = card.Number,
                ExpiryDate = card.ExpiryDate,
                Id = card.Id,
                Owner = owner
            };

        public async Task<Result<PaymentResponse>> NewCardPay(NewCardPaymentRequest request, string languageCode, string ipAddress)
        {
            var (_, isValidationFailture, validationError) = await Validate(request);
            if (isValidationFailture)
                return Result.Fail<PaymentResponse>(validationError);

            var (_, isFailture, token, error) = await _payfortService.Tokenization(
                new TokenizationRequest(request.Number, request.HolderName, request.SecurityCode, request.ExpiryDate, request.RememberMe, languageCode));
            if (isFailture)
                return Result.Fail<PaymentResponse>(error);

            var (_, _, customer, _) = await _customerContext.GetCustomer();
            if (request.RememberMe)
            {
                var card = _context.Cards.Add(new Card()
                {
                    ExpiryDate = token.ExpiryDate,
                    HolderName = token.CardHolderName,
                    Number = token.CardNumber,
                    Token = token.TokenName
                });
                _context.CustomerCardRelations.Add(new CustomerCardRelation()
                {
                    CustomerId = customer.Id,
                    CardId = card.Entity.Id
                });

                await _context.SaveChangesAsync();
            }
            return await MakePayment(new PaymentRequest(request.Amount, request.Currency, request.SecurityCode, token.TokenName, request.RememberMe, $"{customer.FirstName} {customer.LastName}",
                customer.Email, ipAddress, request.ReferenceCode, languageCode));
        }

        public async Task<Result<PaymentResponse>> SavedCardPay(SavedCardPaymentRequest request, string languageCode, string ipAddress)
        {
            var (_, isFailture, error) = await Validate(request);
            if (isFailture)
                return Result.Fail<PaymentResponse>(error);

            var (_, _, customer, _) = await _customerContext.GetCustomer();
            var card = await _context.Cards.FindAsync(request.CardId);
            return await MakePayment(new PaymentRequest(request.Amount, request.Currency, request.SecurityCode, card.Token, true, $"{customer.FirstName} {customer.LastName}",
                customer.Email, ipAddress, request.ReferenceCode, languageCode));
        }

        private async Task<Result<PaymentResponse>> MakePayment(PaymentRequest request)
        {
            var (_, isFailture, payment, error) = await _payfortService.Payment(request);
            if (isFailture)
                return Result.Fail<PaymentResponse>(error);
            var booking = await _context.Bookings.Where(b => b.ReferenceCode == request.ReferenceCode).FirstAsync();
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

        private async Task<Result> Validate(NewCardPaymentRequest request)
        {
            var fieldValidateResult = GenericValidator<NewCardPaymentRequest>.Validate(v =>
            {
                v.RuleFor(c => c.Amount).NotEmpty();
                v.RuleFor(c => c.Currency).NotEmpty().IsInEnum();
                v.RuleFor(c => c.HolderName).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
                v.RuleFor(c => c.Number).NotEmpty();
                v.RuleFor(c => c.ExpiryDate).NotEmpty();
            }, request);

            if (fieldValidateResult.IsFailure)
                return  fieldValidateResult;

            return Result.Combine(await CheckReferenceCode(request.ReferenceCode));
        }

        private async Task<Result> Validate(SavedCardPaymentRequest request)
        {
            var fieldValidateResult = GenericValidator<SavedCardPaymentRequest>.Validate(v =>
            {
                v.RuleFor(c => c.Amount).NotEmpty();
                v.RuleFor(c => c.Currency).NotEmpty().IsInEnum();
                v.RuleFor(c => c.CardId).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
            }, request);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return Result.Combine(await CheckUserCanUseCard(request.CardId),
                                  await CheckReferenceCode(request.ReferenceCode));
        }

        private async Task<Result> CheckUserCanUseCard(int cardId)
        {
            var (_, companyFailture, company, companyError) = await _customerContext.GetCompany();
            if (!companyFailture)
                return Result.Fail(companyError);

            var (_, customerFailture, customer, customerError) = await _customerContext.GetCustomer();
            if (!customerFailture)
                return Result.Fail(customerError);

            var query = from card in _context.Cards
                        join compCard in _context.CompanyCardRelations on card.Id equals compCard.CardId into companyCards
                        from companyCard in companyCards.DefaultIfEmpty()
                        join cusCard in _context.CustomerCardRelations on card.Id equals cusCard.CardId into customerCards
                        from customerCard in customerCards.DefaultIfEmpty()
                        where card.Id == cardId && (companyCard != null || customerCard != null)
                        select 1;

            return await query.AnyAsync()
                ? Result.Ok()
                : Result.Fail("User cannot pay with selected payment card");
        }

        private async Task<Result> CheckReferenceCode(string referenceCode)
        {
            var booking = await _context.Bookings.Where(b => b.ReferenceCode == referenceCode).FirstOrDefaultAsync();
            if (booking == null)
                return Result.Fail("Invalid Reference code");
            if (new[] { BookingStatusCodes.Cancelled, BookingStatusCodes.Invalid, BookingStatusCodes.Rejected}.Contains(booking.Status))
                return Result.Fail($"Invalid booking status: {booking.Status.ToString()}");
            return Result.Ok();
        }

        private readonly EdoContext _context;
    }
}
