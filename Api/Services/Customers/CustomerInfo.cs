using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public readonly struct CustomerInfo
    {
        public CustomerInfo(int customerId, string firstName, string lastName, string email,
            string title, string position, int companyId, string companyName, Maybe<int> branchId, bool isMaster,
            CustomerCompanyPermissions permissionsInCompany)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            CompanyId = companyId;
            CompanyName = companyName;
            BranchId = branchId;
            IsMaster = isMaster;
            PermissionsInCompany = permissionsInCompany;
        }
        
        public void Deconstruct(out int customerId, out int companyId, out Maybe<int> branchId, out bool isMaster)
        {
            customerId = CustomerId;
            companyId = CompanyId;
            branchId = BranchId;
            isMaster = IsMaster;
        }
        
        public bool Equals(CustomerInfo other) => (CustomerId, CompanyId, BranchId, IsMaster)
            == ((other.CustomerId, other.CompanyId, other.BranchId, other.IsMaster));

        public override bool Equals(object obj) => obj is CustomerInfo other && Equals(other);

        public override int GetHashCode() => (CustomerId, CompanyId, BranchId, IsMaster).GetHashCode();
        
        public int CustomerId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public int CompanyId { get; }
        public string CompanyName { get; }
        public Maybe<int> BranchId { get; }
        public bool IsMaster { get; }
        public CustomerCompanyPermissions PermissionsInCompany { get; }
        public string Title { get; }
        public string Position { get; }
    }
}