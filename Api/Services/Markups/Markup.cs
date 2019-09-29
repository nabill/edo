using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class Markup
    {
        public AggregatedMarkupFunction Function { get; set; }
        
        public List<MarkupPolicy> Policies { get; set; } 
    }
    
    public delegate ValueTask<decimal> AggregatedMarkupFunction(decimal supplierPrice, Currencies currency);
}