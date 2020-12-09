using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct BookingMarkupPayment
    {
        public BookingMarkupPayment(int bookingMarkupId, string referenceCode, in MoneyAmount moneyAmount, int agencyAccountId)
        {
            BookingMarkupId = bookingMarkupId;
            ReferenceCode = referenceCode;
            MoneyAmount = moneyAmount;
            AgencyAccountId = agencyAccountId;
        }


        public int BookingMarkupId { get; }
        public string ReferenceCode { get; }
        public MoneyAmount MoneyAmount { get; }
        public int AgencyAccountId { get; }
    }
}