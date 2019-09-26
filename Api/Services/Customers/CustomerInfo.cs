using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public readonly struct CustomerInfo
    {
        public CustomerInfo(Customer customer, Company company, Branch branch, bool isMaster)
        {
            Customer = customer;
            Company = company;
            Branch = branch;
            IsMaster = isMaster;
        }
        
        public void Deconstruct(out Customer customer, out Company company, out Maybe<Branch> branch, out bool isMaster)
        {
            customer = Customer;
            company = Company;
            branch = Branch;
            isMaster = IsMaster;
        }
        
        public bool Equals(CustomerInfo other) => (Customer, Company, Branch, IsMaster)
            == ((other.Customer, other.Company, other.Branch, other.IsMaster));

        public override bool Equals(object obj) => obj is CustomerInfo other && Equals(other);

        public override int GetHashCode() => (Customer, Company, Branch, IsMaster).GetHashCode();
        
        public Customer Customer { get; }
        public Company Company { get; }
        public Maybe<Branch> Branch { get; }
        public bool IsMaster { get; }
    }
}