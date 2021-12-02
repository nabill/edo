using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class DiscountFunctionService : IDiscountFunctionService
    {
        public DiscountFunctionService(IDiscountStorage discountStorage)
        {
            _discountStorage = discountStorage;
        }
        
        
        public async ValueTask<PriceProcessFunction> Get(MarkupPolicy policy, MarkupSubjectInfo subject)
        {
            // Discounts are only supported for global markups for now
            if (policy.SubjectScopeType != SubjectMarkupScopeTypes.Global)
                return price => new ValueTask<MoneyAmount>(price);

            return moneyAmount =>
            {
                var currentAmount = moneyAmount;
                foreach (var discount in GetAgentDiscounts())
                {
                    currentAmount = new MoneyAmount
                    {
                        Amount = currentAmount.Amount * (100 - discount.DiscountPercent) / 100,
                        Currency = currentAmount.Currency
                    };
                }

                return new ValueTask<MoneyAmount>(currentAmount);
            };

            
            List<Discount> GetAgentDiscounts() 
                => _discountStorage.Get(d => d.TargetPolicyId == policy.Id && 
                    d.TargetAgencyId == subject.AgencyId && 
                    d.IsActive);
        }

        
        private readonly IDiscountStorage _discountStorage;
    }
}