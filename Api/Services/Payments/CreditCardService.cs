using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments.CreditCard;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class CreditCardService : ICreditCardService
    {
        public CreditCardService(EdoContext context)
        {
            _context = context;
        }

        public async Task<List<CreditCardInfo>> Get(Customer customer, Company company)
        {
            var customerId = customer.Id;
            var companyId = company.Id;
            var cards = await _context.CreditCards
                    .Where(card => card.OwnerType == CreditCardOwnerType.Company && card.OwnerId == companyId ||
                        card.OwnerType == CreditCardOwnerType.Customer && card.OwnerId == customerId)
                    .Select(ToCardInfo)
                    .ToListAsync();

            return cards;
        }

        public async Task<Result> IsAvailable(int cardId, Customer customer, Company company)
        {
            var customerId = customer.Id;
            var companyId = company.Id;
            var query = _context.CreditCards
                .Where(card => card.Id == cardId && (card.OwnerType == CreditCardOwnerType.Company && card.OwnerId == companyId ||
                    card.OwnerType == CreditCardOwnerType.Customer && card.OwnerId == customerId));

            return await query.AnyAsync()
                ? Result.Ok()
                : Result.Fail("User doesn't have access to use this credit card");
        }

        public async Task<Result<CreditCardInfo>> Save(SaveCreditCardRequest request, int ownerId)
        {
            var (_, isValidationFailure, validationError) = Validate(request);
            if (isValidationFailure)
                return Result.Fail<CreditCardInfo>(validationError);

            var card = new CreditCard()
            {
                ReferenceCode = request.ReferenceCode,
                ExpirationDate = request.ExpirationDate,
                HolderName = request.HolderName,
                MaskedNumber = request.Number,
                Token = request.Token,
                OwnerId = ownerId,
                OwnerType = request.OwnerType
            };
            await _context.CreditCards.AddAsync(card);
            await _context.SaveChangesAsync();
            var info = ToCardInfoFunc(card);
            return Result.Ok(info);
        }

        public async Task<Result> Delete(int cardId, Customer customer, Company company)
        {
            var (_, isValidationFailure, validationError) = await IsAvailable(cardId, customer, company);
            if (isValidationFailure)
                return Result.Fail(validationError);

            var card = await _context.CreditCards.FindAsync(cardId);
            _context.CreditCards.Remove(card);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        private static readonly Expression<Func<CreditCard, CreditCardInfo>> ToCardInfo = (card) =>
            new CreditCardInfo(card.Id, card.MaskedNumber, card.ExpirationDate, card.HolderName, card.OwnerType, card.Token);

        private static readonly Func<CreditCard, CreditCardInfo> ToCardInfoFunc = ToCardInfo.Compile();

        private Result Validate(SaveCreditCardRequest  request)
        {
            var fieldValidateResult = GenericValidator<SaveCreditCardRequest>.Validate(v =>
            {
                v.RuleFor(c => c.HolderName).NotEmpty();
                v.RuleFor(c => c.ReferenceCode).NotEmpty();
                v.RuleFor(c => c.Number).NotEmpty();
                v.RuleFor(c => c.ExpirationDate).NotEmpty();
                v.RuleFor(c => c.Token).NotEmpty();
                v.RuleFor(c => c.OwnerType).IsInEnum();
            }, request);

            return fieldValidateResult.IsFailure ? fieldValidateResult : Result.Ok();
        }

        private readonly EdoContext _context;
    }
}
