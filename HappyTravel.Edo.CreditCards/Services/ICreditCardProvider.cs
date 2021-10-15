using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.CreditCards.Services
{
    public interface ICreditCardProvider
    {
        Task<Result<CreditCardInfo>> Get(string referenceCode, MoneyAmount moneyAmount, DateTime activationDate, DateTime dueDate, Suppliers supplier, string accommodationName);

        Task<Result> ProcessAmountChange(string referenceCode, MoneyAmount newAmount);
    }
}