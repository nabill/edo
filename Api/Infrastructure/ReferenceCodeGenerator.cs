using HappyTravel.Edo.Api.Infrastructure.Constants;
using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class ReferenceCodeGenerator
    {
        public static string Generate(string serviceType, string countryCode, long idn)
        {
            //temporary solution 
            //TODO: change
            var randomElement = DateTime.UtcNow.Ticks;

            return string.Join('-', serviceType, countryCode, idn, randomElement);
        }
    }
}