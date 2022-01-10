using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    internal static class RateExtensions
    {
        public static Rate MapFromEdoModel(this Api.Models.Accommodations.Rate rate) 
            => new Rate(finalPrice: rate.FinalPrice,
                gross: rate.Gross,
                discounts: rate.Discounts,
                description: rate.Description);
    }
}