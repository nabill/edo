﻿using System.Linq;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    internal static class RateExtensions
    {
        public static Rate MapFromEdoModel(this Api.Models.Accommodations.Rate rate) 
            => new(totalPrice: rate.FinalPrice,
                gross: rate.Gross,
                discounts: rate.Discounts
                    .Select(d => new Discount(d.Percent, d.Description))
                    .ToList(),
                description: rate.Description);
    }
}