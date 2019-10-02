using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerCompanyRelation
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        
        public int? BranchId { get; set; }
        public CustomerCompanyRelationTypes Type { get; set; }
    }
}