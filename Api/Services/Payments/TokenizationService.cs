using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class TokenizationService : ITokenizationService
    {
        public TokenizationService(EdoContext context, IPayfortService payfortService, ICreditCardService cardService, IMemoryFlow flow)
        {
            _context = context;
            _payfortService = payfortService;
            _cardService = cardService;
            _flow = flow;
        }

        public async Task<Result<GetTokenResponse>> GetOneTimeToken(GetOneTimeTokenRequest request, string languageCode, Customer customer)
        {
            var (_, isValidationFailure, validationError) = Validate(request);
            if (isValidationFailure)
                return Result.Fail<GetTokenResponse>(validationError);

            var (_, isFailure, token, error) = await _payfortService.Tokenize(
                new TokenizationRequest(request.Number, request.HolderName, request.SecurityCode, request.ExpirationDate, false, languageCode));
            if (isFailure)
                return Result.Fail<GetTokenResponse>(error);

            return Result.Ok(StoreToken(new StoredTokenInfo(token.TokenName, customer.Id, PaymentTokenType.OneTime, null)));
        }

        public async Task<Result<GetTokenResponse>> GetToken(GetTokenRequest request, Customer customer, Company company)
        {
            var (_, isFailure, error) = await Validate(request, customer, company);
            if (isFailure)
                return Result.Fail<GetTokenResponse>(error);

            var card = await _context.CreditCards.FindAsync(request.CardId);
            return Result.Ok(StoreToken(new StoredTokenInfo(card.Token, customer.Id, PaymentTokenType.Stored,card.Id)));
        }


        public Result<StoredTokenInfo> GetStoredToken(string tokenId, Customer customer)
        {
            if (!_flow.TryGetValue<StoredTokenInfo>(_flow.BuildKey(KeyPrefix, tokenId),
                out var token))
                return Result.Fail<StoredTokenInfo>($"Cannot find stored token by id {tokenId}");
            if (token.CustomerId != customer.Id)
                return Result.Fail<StoredTokenInfo>("User doesn't have access to use this credit card");

            return Result.Ok(token);
        }

        private GetTokenResponse StoreToken(StoredTokenInfo storedToken)
        {
            var id = Guid.NewGuid().ToString("N");
            _flow.Set(
                _flow.BuildKey(KeyPrefix,id),
                storedToken,
                ExpirationPeriod);
            return new GetTokenResponse(id, storedToken.TokenType);
        }

        private Result Validate(GetOneTimeTokenRequest  request)
        {
            var fieldValidateResult = GenericValidator<GetOneTimeTokenRequest>.Validate(v =>
            {
                v.RuleFor(c => c.HolderName).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
                v.RuleFor(c => c.Number).NotEmpty();
                v.RuleFor(c => c.ExpirationDate).NotEmpty();
            }, request);

            if (fieldValidateResult.IsFailure)
                return  fieldValidateResult;

            return Result.Ok();
        }

        private async Task<Result> Validate(GetTokenRequest request, Customer customer, Company company)
        {
            var fieldValidateResult = GenericValidator<GetTokenRequest>.Validate(v => { v.RuleFor(c => c.CardId).NotEmpty(); }, request);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return Result.Combine(await _cardService.IsAvailable(request.CardId, customer, company));
        }

        private const string KeyPrefix = nameof(StoredTokenInfo) + "PaymentTokenInfo";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromHours(5);
        private readonly EdoContext _context;
        private readonly IPayfortService _payfortService;
        private readonly ICreditCardService _cardService;
        private readonly IMemoryFlow _flow;
    }
}
