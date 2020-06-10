using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Account balance info
    /// </summary>
    public class CounterpartyBalanceInfo
    {
        public CounterpartyBalanceInfo(decimal balance, Currencies currency)
        {
            Balance = balance;
            Currency = currency;
        }


        /// <summary>
        ///     Balance
        /// </summary>
        public decimal Balance { get; }


        /// <summary>
        ///     Account currency
        /// </summary>
        public Currencies Currency { get; }
    }
}