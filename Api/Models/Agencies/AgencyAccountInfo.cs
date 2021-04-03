using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyAccountInfo
    {
        /// <summary>
        /// Account id
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// Currency
        /// </summary>
        public Currencies Currency { get; init; }
        
        /// <summary>
        /// Balance
        /// </summary>
        public MoneyAmount Balance { get; init; }
    }
}