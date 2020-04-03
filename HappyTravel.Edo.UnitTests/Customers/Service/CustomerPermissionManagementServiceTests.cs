using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.Service
{
    public class CustomerPermissionManagementServiceTests
    {
        public CustomerPermissionManagementServiceTests(Mock<EdoContext> edoContextMock)
        {
            edoContextMock.Setup(x => x.CustomerCompanyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));

            _customerContextMock = new Mock<ICustomerContext>();

            _customerPermissionManagementService = new CustomerPermissionManagementService(edoContextMock.Object,
                _customerContextMock.Object, null);
        }


        [Fact]
        public async Task Set_without_permissions_must_fail()
        {
            SetActingCustomer(_customerInfoNoPermissions);

            var (_, isFailure, _, error) = await _customerPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("You have no acceptance to manage customers permissions", error);
        }

        [Fact]
        public async Task Set_with_different_counterparty_must_fail()
        {
            SetActingCustomer(_customerInfoDifferentCounterparty);

            var (_, isFailure, _, error) = await _customerPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("The customer isn't affiliated with the counterparty", error);
        }

        [Fact]
        public async Task Set_relation_not_found_must_fail()
        {
            SetActingCustomer(_customerInfoRegular);

            var (_, isFailure, _, error) = await _customerPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 0, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Could not find relation between the customer 0 and the counterparty 1", error);
        }

        [Fact]
        public async Task Set_revoke_last_management_must_fail()
        {
            SetActingCustomer(_customerInfoRegular);

            var (_, isFailure, _, error) = await _customerPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 2, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Cannot revoke last permission management rights", error);
        }

        [Fact]
        public async Task Set_must_susseed()
        {
            SetActingCustomer(_customerInfoRegular);

            var (isSuccess, _, _, _) = await _customerPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isSuccess);
        }

        private void SetActingCustomer(CustomerInfo customer) =>
            _customerContextMock.Setup(x => x.GetCustomer()).Returns(new ValueTask<CustomerInfo>(customer));

        private readonly IEnumerable<CustomerCompanyRelation> _relations = new[]
        {
            new CustomerCompanyRelation
            {
                CompanyId = 1,
                BranchId = 1,
                CustomerId = 1,
                Type = CustomerCounterpartyRelationTypes.Master,
                InCounterpartyPermissions = InCounterpartyPermissions.PermissionManagementInBranch
            },
            new CustomerCompanyRelation
            {
                CompanyId = 1,
                BranchId = 1,
                CustomerId = 2,
                Type = CustomerCounterpartyRelationTypes.Regular,
                InCounterpartyPermissions = InCounterpartyPermissions.PermissionManagementInCounterparty
            }
        };

        private static readonly CustomerInfo _customerInfoRegular = CustomerInfoFactory.CreateByWithCounterpartyAndBranch(10, 1, 1);
        private static readonly CustomerInfo _customerInfoDifferentCounterparty = CustomerInfoFactory.CreateByWithCounterpartyAndBranch(2, 2, 1);
        private static readonly CustomerInfo _customerInfoNoPermissions = new CustomerInfo(
            11, "", "", "", "", "", 1, "", 1, false, InCounterpartyPermissions.None);

        private readonly CustomerPermissionManagementService _customerPermissionManagementService;
        private readonly Mock<ICustomerContext> _customerContextMock;
    }
}
