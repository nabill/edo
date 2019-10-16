using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Customers;

namespace HappyTravel.Edo.UnitTests.Infrastructure
{
    public static class CustomerInfoFactory
    {
        public static CustomerInfo GetByCustomerId(int customerId)
        {
            return new CustomerInfo(customerId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, Maybe<int>.None, true);
        }
    }
}