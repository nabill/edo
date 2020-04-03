using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerCounterpartyRelation
    {
        public int CustomerId { get; set; }
        public int CounterpartyId { get; set; }
        public InCounterpartyPermissions InCounterpartyPermissions { get; set; }
        public int BranchId { get; set; }
        public CustomerCounterpartyRelationTypes Type { get; set; }


        public override bool Equals(object obj) => obj is CustomerCounterpartyRelation other && Equals(other);


        public bool Equals(CustomerCounterpartyRelation other)
            => Equals((CustomerId, CounterpartyId, InCounterpartyPermissions, BranchId, Type),
                (other.CustomerId, other.CounterpartyId, other.InCounterpartyPermissions, other.BranchId, other.Type));


        public override int GetHashCode() => (CustomerId, CounterpartyId, InCounterpartyPermissions, BranchId, Type).GetHashCode();
    }
}