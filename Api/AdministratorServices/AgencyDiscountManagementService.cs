using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencyDiscountManagementService : IAgencyDiscountManagementService
    {
        public AgencyDiscountManagementService(EdoContext context)
        {
            _context = context;
        }


        public Task<List<DiscountInfo>> Get(int agencyId)
        {
            return (from discount in _context.Discounts
                join markupPolicy in _context.MarkupPolicies on discount.TargetPolicyId equals markupPolicy.Id
                select new DiscountInfo
                {
                    DiscountPercent = discount.DiscountPercent,
                    Description = discount.Description,
                    TargetMarkupId = discount.TargetPolicyId,
                    TargetPolicyDescription = markupPolicy.Description
                }).ToListAsync();
        }


        public async Task<Result> Activate(int agencyId, int discountId)
        {
            return await Get(agencyId, discountId)
                .Map(d => Update(d, discount => discount.IsActive = true));
        }


        public async Task<Result> Deactivate(int agencyId, int discountId)
        {
            return await Get(agencyId, discountId)
                .Map(d => Update(d, discount => discount.IsActive = false));
        }


        public Task<Result> Add(int agencyId, CreateDiscountRequest createDiscountRequest)
        {
            return ValidatePercent(createDiscountRequest.DiscountPercent)
                .Tap(async () =>
                {
                    _context.Discounts.Add(new Discount
                    {
                        DiscountPercent = createDiscountRequest.DiscountPercent,
                        Description = createDiscountRequest.Description,
                        TargetPolicyId = createDiscountRequest.TargetMarkupId,
                        IsActive = true,
                        TargetAgencyId = agencyId
                    });
                    await _context.SaveChangesAsync();
                });
        }


        public async Task<Result> Update(int agencyId, int discountId, EditDiscountRequest editDiscountRequest)
        {
            return await Get(agencyId, discountId)
                .Check(_ => ValidatePercent(editDiscountRequest.DiscountPercent))
                .Map(d => Update(d, discount =>
                {
                    discount.DiscountPercent = editDiscountRequest.DiscountPercent;
                    discount.Description = editDiscountRequest.Description;
                }));
        }


        private async Task<Result<Discount>> Get(int agencyId, int discountId)
            => await _context.Discounts.SingleOrDefaultAsync(d => d.Id == discountId && d.TargetAgencyId == agencyId) ??
                Result.Failure<Discount>($"Could not find discount with id {discountId}");


        private async Task Update(Discount discount, Action<Discount> updateAction)
        {
            updateAction(discount);
            _context.Update(discount);
            await _context.SaveChangesAsync();
        }


        private Result ValidatePercent(decimal discountPercent)
            => discountPercent <= MaxDiscountPercent
                ? Result.Success()
                : Result.Failure($"Could not set discount percent with value more than {MaxDiscountPercent}");


        private const decimal MaxDiscountPercent = 5;
        
        private readonly EdoContext _context;
    }
}