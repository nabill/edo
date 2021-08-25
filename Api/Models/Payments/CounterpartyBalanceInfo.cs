using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Account balance info
    /// </summary>
    public class CounterpartyBalanceInfo
    {
        public CounterpartyBalanceInfo(int accountId, decimal balance, Currencies currency)
        {
            AccountId = accountId;
            Balance = balance;
            Currency = currency;
        }


        /// <summary>
        ///     Account ID
        /// </summary>
        public int AccountId { get; }

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