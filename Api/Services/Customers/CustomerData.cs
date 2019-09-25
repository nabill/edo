using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public readonly struct CustomerData
    {
        public CustomerData(Customer customer, Company company, bool isMaster)
        {
            Customer = customer;
            Company = company;
            IsMaster = isMaster;
        }
        
        public Customer Customer { get; }
        public Company Company { get; }
        public bool IsMaster { get; }
    }
}