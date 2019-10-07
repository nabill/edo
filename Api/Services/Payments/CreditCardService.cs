using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class CreditCardService : ICreditCardService
    {
        public CreditCardService(EdoContext context, IOptions<PayfortOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<List<CreditCardInfo>> Get(CustomerInfo customerInfo)
        {
            var customerId = customerInfo.Customer.Id;
            var companyId = customerInfo.Company.Id;
            var cards = await _context.CreditCards
                    .Where(card => card.OwnerType == CreditCardOwnerType.Company && card.OwnerId == companyId ||
                        card.OwnerType == CreditCardOwnerType.Customer && card.OwnerId == customerId)
                    .Select(ToCardInfo)
                    .ToListAsync();

            return cards;
        }

        public async Task<Result<CreditCardInfo>> Save(SaveCreditCardRequest request, CustomerInfo customerInfo)
        {
            int ownerId;
            switch (request.OwnerType)
            {
                case CreditCardOwnerType.Company:
                    ownerId = customerInfo.Company.Id;
                    break;
                case CreditCardOwnerType.Customer:
                    ownerId = customerInfo.Customer.Id;
                    break;
                default: throw new NotImplementedException();
            }
            var (_, isFailure, error) = Validate(request);
            if (isFailure)
                return Result.Fail<CreditCardInfo>(error);

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
            _context.CreditCards.Add(card);
            await _context.SaveChangesAsync();
            var info = ToCardInfoFunc(card);
            return Result.Ok(info);
        }

        public async Task<Result> Delete(int cardId, CustomerInfo customerInfo)
        {
            var (_, isFailure, card, error) = await Get(cardId, customerInfo);
            if (isFailure)
                return Result.Fail(error);

            _context.CreditCards.Remove(card);
            await _context.SaveChangesAsync();
            return MapCardInfo(card);
        }


        public TokenizationSettings GetTokenizationSettings() =>
            new TokenizationSettings(_options.AccessCode, _options.Identifier, _options.TokenizationUrl);


        private static readonly Expression<Func<CreditCard, CreditCardInfo>> ToCardInfo = (card) =>
            new CreditCardInfo(card.Id, card.MaskedNumber, card.ExpirationDate, card.HolderName, card.OwnerType, card.Token);

        private static readonly Func<CreditCard, CreditCardInfo> ToCardInfoFunc = ToCardInfo.Compile();

        private static Result<CreditCardInfo> MapCardInfo(CreditCard card) => 
            Result.Ok(ToCardInfoFunc(card));

        private Result Validate(SaveCreditCardRequest request)
        {
            return GenericValidator<SaveCreditCardRequest>.Validate(v =>
            {
                v.RuleFor(c => c.HolderName).NotEmpty();
                v.RuleFor(c => c.ReferenceCode).NotEmpty();
                v.RuleFor(c => c.Number).NotEmpty();
                v.RuleFor(c => c.ExpirationDate).NotEmpty();
                v.RuleFor(c => c.Token).NotEmpty();
                v.RuleFor(c => c.OwnerType).IsInEnum();
            }, request);
        }

        private async Task<Result<CreditCard>> Get(int cardId, CustomerInfo customerInfo)
        {
            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                return Result.Fail<CreditCard>($"Cannot find credit card by id {cardId}");
            
            if (card.OwnerType == CreditCardOwnerType.Company && card.OwnerId != customerInfo.Company.Id ||
                    card.OwnerType == CreditCardOwnerType.Customer && card.OwnerId != customerInfo.Customer.Id)
                Result.Fail<CreditCard>("User doesn't have access to use this credit card");

            return Result.Ok(card);
        }

        private readonly EdoContext _context;
        private readonly PayfortOptions _options;
    }
}
