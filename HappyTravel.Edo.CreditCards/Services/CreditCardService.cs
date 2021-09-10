using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.CreditCards.Infrastructure.Logging;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.CreditCards.Services
{
    public class CreditCardService
    {
        public CreditCardService(ICreditCardProvider creditCardProvider, ILogger<CreditCardService> logger)
        {
            _creditCardProvider = creditCardProvider;
            _logger = logger;
        }


        public async Task<Result<CreditCardInfo>> Get(string referenceCode, DateTime activationDate, DateTime dueDate, MoneyAmount amount)
        {
            _logger.LogCreditCardServiceCardRequested(dueDate, referenceCode, amount.Amount, amount.Currency.ToString());
                    
            var (_, isFailure, creditCard, error) = await _creditCardProvider.Get(referenceCode, amount, activationDate, dueDate);

            if (isFailure)
            {
                _logger.LogCreditCardServiceCardFailure(referenceCode, error);
                return Result.Failure<CreditCardInfo>(error);
            }
            
            _logger.LogCreditCardServiceCardSuccess(referenceCode);
            return creditCard;
        }
        
        
        private readonly ICreditCardProvider _creditCardProvider;
        private readonly ILogger<CreditCardService> _logger;
    }
}