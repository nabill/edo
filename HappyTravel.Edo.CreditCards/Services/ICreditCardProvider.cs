using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.CreditCards.Services
{
    public interface ICreditCardProvider
    {
        Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, 
            DateTime activationDate, DateTime dueDate, string supplierCode, string accommodationName, 
            string passengerName, DateTimeOffset checkinDate, DateTimeOffset checkoutDate);

        Task<Result> ProcessAmountChange(string referenceCode, MoneyAmount newAmount);
    }
}