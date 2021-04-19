using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Discounts
{
    public readonly struct DiscountApplicationResult<TDetails>
    {
        public TDetails Before { get; init; }
        public Discount Discount { get; init; }
        public TDetails After { get; init; }
    }
}