using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Edo.CreditCards.Options;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.CreditCards.Services
{
    public class ActualCreditCardProvider : ICreditCardProvider
    {
        public ActualCreditCardProvider(IOptions<ActualCreditCardOptions> options)
        {
            _options = options.Value;
        }
        

        public async Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, DateTime activationDate, DateTime dueDate)
        {
            return _options.Cards.TryGetValue(moneyAmount.Currency, out var cardInfo)
                ? await Task.FromResult(Result.Success(cardInfo))
                : await Task.FromResult(Result.Failure<CreditCardInfo>($"Could not get credit card for currency {moneyAmount.Currency}"));
        }
        
        private readonly ActualCreditCardOptions _options;
    }
}