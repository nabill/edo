using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerCompanyRelation
    {
        public Customer Customer { get; set; }

        public int CustomerId { get; set; }

        public Company Company { get; set; }

        public int CompanyId { get; set; }

        public RelationType Type { get; set; }
    }
}