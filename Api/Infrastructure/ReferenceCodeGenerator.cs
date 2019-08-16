using HappyTravel.Edo.Api.Infrastructure.Constants;
using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class ReferenceCodeGenerator
    {
        public static string Generate(string serviceType, string residency, long idn)
        {
            //temporary solution 
            //TODO: change
            var randomElement = DateTime.UtcNow.Ticks;

            return string.Join('-', serviceType, residency, idn, randomElement);
        }
    }
}