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
        
        
       public async Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, 
           DateTime activationDate, DateTime dueDate, string supplierCode, string accommodationName, 
           string passengerFirstName, string passengerLastName, DateTimeOffset checkinDate, DateTimeOffset checkoutDate)
        {
            // Passing null to credit card types before we'll support the types in contracts
            var (_, isFailure, virtualCreditCard, error) = await _vccService.IssueVirtualCreditCard(referenceCode, moneyAmount, null, activationDate, dueDate, new Dictionary<string, string>
            {
                {"Supplier", supplierCode},
                {"AccommodationName", accommodationName},
                {"PassengerFirstName", passengerFirstName},
                {"PassengerLastName", passengerLastName},
                {"CheckinDate", checkinDate.ToString()},
                {"CheckoutDate", checkoutDate.ToString()},
            });
            if (isFailure)
                return Result.Failure<CreditCardInfo>(error);

            return new CreditCardInfo(Number: virtualCreditCard.Number,
                ExpiryDate: virtualCreditCard.Expiry.DateTime,
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
