using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public delegate ValueTask<decimal> AggregatedMarkupFunction(decimal supplierPrice, Currencies currency);
}