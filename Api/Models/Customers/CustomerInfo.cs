using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfo
    {
        public CustomerInfo(int customerId, string firstName, string lastName, string email,
            string title, string position, int counterpartyId, string counterpartyName, int branchId, bool isMaster,
            InCounterpartyPermissions inCounterpartyPermissions)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            BranchId = branchId;
            IsMaster = isMaster;
            InCounterpartyPermissions = inCounterpartyPermissions;
        }


        public void Deconstruct(out int customerId, out int companyId, out int branchId, out bool isMaster)
        {
            customerId = CustomerId;
            companyId = CounterpartyId;
            branchId = BranchId;
            isMaster = IsMaster;
        }


        public bool Equals(CustomerInfo other)
            => (CustomerId, CompanyId: CounterpartyId, BranchId, IsMaster)
                == (other.CustomerId, other.CounterpartyId, other.BranchId, other.IsMaster);


        public override bool Equals(object obj) => obj is CustomerInfo other && Equals(other);


        public override int GetHashCode() => (CustomerId, CompanyId: CounterpartyId, BranchId, IsMaster).GetHashCode();


        public int CustomerId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public int CounterpartyId { get; }
        public string CounterpartyName { get; }
        public int BranchId { get; }
        public bool IsMaster { get; }
        public InCounterpartyPermissions InCounterpartyPermissions { get; }
        public string Title { get; }
        public string Position { get; }
    }
}