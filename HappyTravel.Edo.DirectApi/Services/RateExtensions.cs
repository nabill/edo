using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    internal static class RateExtensions
    {
        public static Rate MapFromEdoModel(this Api.Models.Accommodations.Rate rate) 
            => new Rate(finalPrice: rate.FinalPrice,
                gross: rate.Gross,
                discounts: rate.Discounts,
                type: rate.Type,
                description: rate.Description);
    }
}