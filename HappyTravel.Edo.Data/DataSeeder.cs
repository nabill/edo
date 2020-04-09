using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Data
{
    internal static class DataSeeder
    {
        public static void AddData(ModelBuilder builder)
        {
            AddTestAgent(builder);
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


        private static void AddTestAgent(ModelBuilder builder)
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
            builder.Entity<Agent>().HasData(new Agent
            {
                Id = -1,
                Email = "test@happytravel.com",
                FirstName = "FirstName",
                LastName = "LastName",
                IdentityHash = "postman",
                Title = "Mr.",
                Position = "Position"
            });
            builder.Entity<AgentCounterpartyRelation>().HasData(new AgentCounterpartyRelation
            {
                Type = AgentCounterpartyRelationTypes.Master,
                CounterpartyId = -1,
                AgentId = -1,
                AgencyId = -1
            });
            builder.Entity<Agency>().HasData(new Agency
            {
                Id = -1,
                Name = "Test agency",
                CounterpartyId = -1
            });
        }
    }
}