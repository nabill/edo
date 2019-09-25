using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class Markup
    {
        public MarkupFunction Function { get; set; }
        
        public List<MarkupPolicy> Policies { get; set; } 
    }
    
    public delegate decimal MarkupFunction(decimal supplierPrice);
}