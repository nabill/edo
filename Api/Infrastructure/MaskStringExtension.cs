using System;
using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class MaskStringExtension
    {
        public static string Mask(this string str)
        {
            // TODO: replace with production ready mask function
            return new string((from ch in str let random = new Random() let replace = random.Next(0, 2) > 0 select replace ? '*' : ch).ToArray());
        }
    }
}