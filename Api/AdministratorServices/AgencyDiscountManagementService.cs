using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
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
            => _context.Discounts.Where(d => d.TargetAgencyId == agencyId)
                .Select(d => new DiscountInfo
                {
                    DiscountPercent = d.DiscountPercent,
                    Description = d.Description
                })
                .ToListAsync();


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


        public Task<Result> Add(int agencyId, DiscountInfo discountInfo)
        {
            return Validate(discountInfo)
                .Tap(async () =>
                {
                    _context.Discounts.Add(new Discount
                    {
                        DiscountPercent = discountInfo.DiscountPercent,
                        Description = discountInfo.Description,
                        IsActive = true,
                        TargetAgencyId = agencyId
                    });
                    await _context.SaveChangesAsync();
                });
        }


        public async Task<Result> Update(int agencyId, int discountId, DiscountInfo discountInfo)
        {
            return await Get(agencyId, discountId)
                .Check(_ => Validate(discountInfo))
                .Map(d => Update(d, discount =>
                {
                    discount.DiscountPercent = discountInfo.DiscountPercent;
                    discount.Description = discountInfo.Description;
                }));
        }


        private async Task<Result<Discount>> Get(int agencyId, int discountId)
            => await _context.Discounts.SingleOrDefaultAsync(d => d.Id == discountId) ??
                Result.Failure<Discount>($"Could not find discount with id {discountId}");


        private async Task Update(Discount discount, Action<Discount> updateAction)
        {
            updateAction(discount);
            _context.Update(discount);
            await _context.SaveChangesAsync();
        }


        private Result Validate(DiscountInfo discount)
            => discount.DiscountPercent <= MaxDiscountPercent
                ? Result.Success()
                : Result.Failure($"Could not set discount percent with value more than {MaxDiscountPercent}");


        private const decimal MaxDiscountPercent = 5;
        
        private readonly EdoContext _context;
    }
}