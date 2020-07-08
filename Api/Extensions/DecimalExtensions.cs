using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class DecimalExtensions
    {
        public static bool IsGreaterThan(this decimal a, decimal b) => a.CompareTo(b) > 0;
        public static bool IsGreaterOrEqualThan(this decimal a, decimal b) => a.CompareTo(b) >= 0;
        public static bool IsLessThan(this decimal a, decimal b) => a.CompareTo(b) < 0;
        public static bool IsLessOrEqualThan(this decimal a, decimal b) => a.CompareTo(b) <= 0;
    }
}
