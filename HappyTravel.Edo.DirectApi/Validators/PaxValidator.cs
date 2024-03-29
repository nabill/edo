﻿using FluentValidation;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class PaxValidator : AbstractValidator<Pax>
    {
        public PaxValidator()
        {
            RuleFor(p => p.Age).GreaterThanOrEqualTo(0).LessThan(100);
            RuleFor(p => p.Title).NotEmpty().IsInEnum();
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.LastName).NotEmpty();
        }
    }
}