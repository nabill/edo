using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly EdoContext _context;

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

        public async Task<Result> NewCardPay(NewCardPaymentRequest request, string lang)
        {
            var (_, isValidationFailture, validationError) = await Validate(request);
            if (isValidationFailture)
                return Result.Fail(validationError);

            var (_, isFailture, token, error) = await _payfortService.Tokenization(
                new Models.Payments.Payfort.TokenizationRequest(request.Number, request.HolderName, request.SecurityCode, request.ExpiryDate, request.RememberMe), lang);
            if (isFailture)
                return Result.Fail(error);
            if (request.RememberMe)
            {
                var card = _context.Cards.Add(new Card()
                {
                    ExpiryDate = token.ExpiryDate,
                    HolderName = token.CardHolderName,
                    Number = token.CardNumber,
                    Token = token.TokenName
                });
                var (_, _, customer, _) = await _customerContext.GetCustomer();
                _context.CustomerCardRelations.Add(new CustomerCardRelation()
                {
                    CustomerId = customer.Id,
                    CardId = card.Entity.Id
                });

                await _context.SaveChangesAsync();
            }

            throw new NotImplementedException();
        }

        public async Task<Result> SavedCardPay(SavedCardPaymentRequest request)
        {
            var (_, isFailture, error) = await Validate(request);
            if (isFailture)
                return Result.Fail(error);

            throw new NotImplementedException();
        }

        private Task<Result> Validate(NewCardPaymentRequest request)
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
                return  Task.FromResult(fieldValidateResult);

            return Task.FromResult(Result.Ok());
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

            return Result.Combine(await CheckUserCanUseCard(request.CardId));
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
    }
}