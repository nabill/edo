using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Infrastructure
{
    public static class CustomerInfoFactory
    {
        public static CustomerInfo GetByCustomerId(int customerId)
        {
            return new CustomerInfo(customerId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, default, true, InCounterpartyPermissions.All);
        }


        public static CustomerInfo CreateByWithCounterpartyAndAgency(int customerId, int counterpartyId, int agencyId)
        {
            return new CustomerInfo(customerId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, counterpartyId, string.Empty, agencyId, true, InCounterpartyPermissions.All);
        }
    }
}