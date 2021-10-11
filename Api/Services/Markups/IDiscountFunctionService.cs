using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IDiscountFunctionService
    {
        ValueTask<PriceProcessFunction> Get(MarkupPolicy policy, MarkupSubjectInfo subject);
    }
}