using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Data
{
    internal static class DataSeeder
    {
        public static void AddData(ModelBuilder builder)
        {
            AddTestCustomer(builder);
            AddTestAdmin(builder);
        }


        private static void AddTestAdmin(ModelBuilder builder)
        {
            builder.Entity<Administrator>().HasData(new Administrator
            {
                Id = -1,
                Email = "testAdmin@happytravel.com",
                FirstName = "FirstName",
                LastName = "LastName",
                IdentityHash = "postman",
                Position = "Position"
            });
        }


        private static void AddTestCustomer(ModelBuilder builder)
        {
            builder.Entity<Counterparty>().HasData(new Counterparty
            {
                Id = -1,
                Name = "Test counterparty",
                Address = "Address",
                City = "City",
                Fax = "Fax",
                Phone = "Phone",
                CountryCode = "IT",
                State = CounterpartyStates.PendingVerification,
                Website = "https://happytravel.com",
                PostalCode = "400055",
                PreferredCurrency = Currencies.USD,
                PreferredPaymentMethod = PaymentMethods.CreditCard
            });
            builder.Entity<Customer>().HasData(new Customer
            {
                Id = -1,
                Email = "test@happytravel.com",
                FirstName = "FirstName",
                LastName = "LastName",
                IdentityHash = "postman",
                Title = "Mr.",
                Position = "Position"
            });
            builder.Entity<CustomerCounterpartyRelation>().HasData(new CustomerCounterpartyRelation
            {
                Type = CustomerCounterpartyRelationTypes.Master,
                CounterpartyId = -1,
                CustomerId = -1,
                BranchId = -1
            });
            builder.Entity<Branch>().HasData(new Branch
            {
                Id = -1,
                Title = "Test branch",
                CounterpartyId = -1
            });
        }
    }
}