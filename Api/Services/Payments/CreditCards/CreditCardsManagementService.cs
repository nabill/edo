using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardsManagementService : ICreditCardsManagementService
    {
        public CreditCardsManagementService(EdoContext context, IOptions<PayfortOptions> options)
        {
            _context = context;
            _options = options.Value;
        }


        public async Task<List<CreditCardInfo>> Get(AgentContext agentContext)
        {
            var agentId = agentContext.AgentId;
            var cards = await _context.CreditCards
                .Where(card => card.OwnerType == CreditCardOwnerType.Agent && card.OwnerId == agentId)
                .Select(ToCardInfo)
                .ToListAsync();

            return cards;
        }


        public Task Save(CreditCardInfo cardInfo, string token, AgentContext agentContext)
        {
            int ownerId;
            switch (cardInfo.OwnerType)
            {
                case CreditCardOwnerType.Agent:
                    ownerId = agentContext.AgentId;
                    break;
                default: throw new NotImplementedException();
            }
            
            var card = new CreditCard
            {
                ExpirationDate = cardInfo.ExpirationDate,
                HolderName = cardInfo.HolderName,
                MaskedNumber = cardInfo.Number,
                Token = token,
                OwnerId = ownerId,
                OwnerType = cardInfo.OwnerType
            };
            _context.CreditCards.Add(card);
            return _context.SaveChangesAsync();
        }


        public async Task<Result> Delete(int cardId, AgentContext agentContext)
        {
            var (_, isFailure, card, error) = await GetEntity(cardId, agentContext);
            if (isFailure)
                return Result.Failure(error);

            _context.CreditCards.Remove(card);
            await _context.SaveChangesAsync();
            return Result.Success();
        }


        public TokenizationSettings GetTokenizationSettings() => new TokenizationSettings(_options.AccessCode, _options.Identifier, _options.TokenizationUrl);

        public Task<Result<string>> GetToken(int cardId, AgentContext agentContext)
        {
            return GetCreditCard(cardId, agentContext)
                .Map(c=> c.Token);
        }


        public Task<Result<CreditCard>> Get(int cardId, AgentContext agentContext)
        {
            return GetCreditCard(cardId, agentContext);
        }


        private async Task<Result<CreditCard>> GetCreditCard(int cardId, AgentContext agentContext)
        {
            var card = await _context.CreditCards.SingleOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                return Result.Failure<CreditCard>($"Cannot find credit card by id {cardId}");

            if (card.OwnerType == CreditCardOwnerType.Agent && card.OwnerId != agentContext.AgentId)
                Result.Failure<CreditCardInfo>("User doesn't have access to use this credit card");

            return Result.Success(card);
        }


        private async Task<Result<CreditCard>> GetEntity(int cardId, AgentContext agentContext)
        {
            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                return Result.Failure<CreditCard>($"Cannot find credit card by id {cardId}");

            if (card.OwnerType == CreditCardOwnerType.Agent && card.OwnerId != agentContext.AgentId)
                Result.Failure<CreditCard>("User doesn't have access to use this credit card");

            return Result.Success(card);
        }


        private static readonly Expression<Func<CreditCard, CreditCardInfo>> ToCardInfo = card =>
            new CreditCardInfo(card.Id, card.MaskedNumber, card.ExpirationDate, card.HolderName, card.OwnerType);

        private static readonly Func<CreditCard, CreditCardInfo> ToCardInfoFunc = ToCardInfo.Compile();

        private readonly EdoContext _context;
        private readonly PayfortOptions _options;
    }
}