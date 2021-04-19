using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyDiscountManagementService
    {
        Task<List<DiscountInfo>> Get(int agencyId);
        
        Task<Result> Activate(int agencyId, int discountId);
        
        Task<Result> Deactivate(int agencyId, int discountId);

        Task<Result> Add(int agencyId, DiscountInfo discountInfo);
        
        Task<Result> Update(int agencyId, int discountId, DiscountInfo discountInfo);
    }
}