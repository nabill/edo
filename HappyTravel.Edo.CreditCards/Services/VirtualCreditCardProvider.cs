using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;
using HappyTravel.VccServiceClient.Services;

namespace HappyTravel.Edo.CreditCards.Services
{
    public class VirtualCreditCardProvider : ICreditCardProvider
    {
        public VirtualCreditCardProvider(IVccService vccService)
        {
            _vccService = vccService;
        }
        
        
        public async Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, DateTime activationDate, DateTime dueDate, Suppliers supplier, string accommodationName)
        {
            // Passing null to credit card types before we'll support the types in contracts
            var (_, isFailure, virtualCreditCard, error) = await _vccService.IssueVirtualCreditCard(referenceCode, moneyAmount, null, activationDate, dueDate, new Dictionary<string, string>
            {
                {"Supplier", EnumFormatters.FromDescription(supplier)},
                {"AccommodationName", accommodationName}
            });
            if (isFailure)
                return Result.Failure<CreditCardInfo>(error);

            return new CreditCardInfo(Number: virtualCreditCard.Number,
                ExpiryDate: virtualCreditCard.Expiry,
                HolderName: virtualCreditCard.Holder,
                SecurityCode: virtualCreditCard.Code);
        }


        public async Task<Result> ProcessAmountChange(string referenceCode, MoneyAmount newAmount)
        {
            if (newAmount.Amount == 0)
                return await _vccService.Delete(referenceCode);

            return await _vccService.ModifyAmount(referenceCode, newAmount);
        }


        private readonly IVccService _vccService;
    }
}