using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerCompanyRelation
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public InCounterpartyPermissions InCounterpartyPermissions { get; set; }
        public int BranchId { get; set; }
        public CustomerCounterpartyRelationTypes Type { get; set; }


        public override bool Equals(object obj) => obj is CustomerCompanyRelation other && Equals(other);


        public bool Equals(CustomerCompanyRelation other)
            => Equals((CustomerId, CompanyId, InCounterpartyPermissions, BranchId, Type),
                (other.CustomerId, other.CompanyId, other.InCounterpartyPermissions, other.BranchId, other.Type));


        public override int GetHashCode() => (CustomerId, CompanyId, InCompanyPermissions: InCounterpartyPermissions, BranchId, Type).GetHashCode();
    }
}