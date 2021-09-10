using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Models;
using HappyTravel.VccServiceClient.Services;

namespace HappyTravel.Edo.CreditCards.Services
{
    public class VirtualCreditCardProvider : ICreditCardProvider
    {
        public VirtualCreditCardProvider(IVccService vccService)
        {
            _vccService = vccService;
        }
        
        
        public async Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, DateTime activationDate, DateTime dueDate)
        {
            var (_, isFailure, virtualCreditCard, error) = await _vccService.IssueVirtualCreditCard(referenceCode, moneyAmount, activationDate, dueDate, new Dictionary<string, string>());
            if (isFailure)
                return Result.Failure<CreditCardInfo>(error);

            return new CreditCardInfo(Number: virtualCreditCard.Number,
                ExpiryDate: virtualCreditCard.Expiry,
                HolderName: virtualCreditCard.Holder,
                SecurityCode: virtualCreditCard.Code);
        }
        
        
        private readonly IVccService _vccService;
    }
}