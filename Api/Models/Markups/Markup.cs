using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public class Markup
    {
        public PriceProcessFunction Function { get; set; }

        public List<MarkupPolicy> Policies { get; set; }
        
        public static Markup Empty => new Markup
        {
            Function = price =>  new ValueTask<MoneyAmount>(price),
            Policies = new List<MarkupPolicy>()
        };
    }
}