using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.Service
{
    public class CustomerServiceTests
    {
        public CustomerServiceTests(Mock<EdoContext> edoContextMock)
        {
            edoContextMock.Setup(x => x.Companies).Returns(DbSetMockProvider.GetDbSetMock(_companies));
            edoContextMock.Setup(x => x.Branches).Returns(DbSetMockProvider.GetDbSetMock(_branches));
            edoContextMock.Setup(x => x.Customers).Returns(DbSetMockProvider.GetDbSetMock(_customers));
            edoContextMock.Setup(x => x.CustomerCompanyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));
            edoContextMock.Setup(x => x.MarkupPolicies).Returns(DbSetMockProvider.GetDbSetMock(new List<MarkupPolicy>()));

            var customerContextMock = new Mock<ICustomerContext>();
            customerContextMock.Setup(x => x.GetCustomer())
                .Returns(new ValueTask<CustomerInfo>(_customerInfo));

            _customerService = new CustomerService(edoContextMock.Object, new DefaultDateTimeProvider(),
                customerContextMock.Object, new MarkupPolicyTemplateService());
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(2, 0)]
        public async Task Company_or_branch_mismatch_must_fail_get_customer(int companyId, int branchId)
        {
            var (_, isFailure, _, _) = await _customerService.GetCustomer(companyId, branchId, 0);
            Assert.True(isFailure);
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(2, 0)]
        public async Task Company_or_branch_mismatch_must_fail_get_customers(int companyId, int branchId)
        {
            var (_, isFailure, _, _) = await _customerService.GetCustomers(companyId, branchId);
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Not_found_customer_must_fail()
        {
            var (_, isFailure, _, _) = await _customerService.GetCustomer(1, 1, 0);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Found_customer_must_match()
        {
            var expectedCustomer = new CustomerInfo(1, "fn", "ln", "email", "title", "pos", 1, "comName",
                1, true, InCompanyPermissions.ObserveMarkupInBranch);

            var (isSuccess, _, actualCustomer, _) = await _customerService.GetCustomer(1, 1, 1);

            Assert.True(isSuccess);
            Assert.Equal(actualCustomer, expectedCustomer);
        }

        [Fact]
        public async Task Found_customer_list_must_match()
        {
            var expectedCustomers = new List<SlimCustomerInfo>
            {
                new SlimCustomerInfo(1, "fn", "ln", default, 1, "comName", 1, "branchName", ""),
                new SlimCustomerInfo(2, "fn2", "ln2", default, 1, "comName", 1, "branchName", "")
            };

            var (isSuccess, _, actualCustomers, _) = await _customerService.GetCustomers(1, 1);

            Assert.True(isSuccess);
            Assert.Equal(expectedCustomers, actualCustomers);
        }

        private readonly IEnumerable<Customer> _customers = new []
        {
            new Customer
            {
                Id = 1,
                Email = "email",
                FirstName = "fn",
                LastName = "ln",
                Position = "pos",
                Title = "title"
            },
            new Customer
            {
                Id = 2,
                Email = "email2",
                FirstName = "fn2",
                LastName = "ln2",
                Position = "pos2",
                Title = "title2"
            },
        };

        private readonly IEnumerable<Company> _companies = new[]
        {
            new Company
            {
                Id = 1,
                Name = "comName"
            }
        };

        private readonly IEnumerable<Branch> _branches = new[]
        {
            new Branch
            {
                Id = 1,
                CompanyId = 1,
                IsDefault = true,
                Title = "branchName"
            }
        };

        private readonly IEnumerable<CustomerCompanyRelation> _relations = new[]
        {
            new CustomerCompanyRelation
            {
                CompanyId = 1,
                BranchId = 1,
                CustomerId = 1,
                Type = CustomerCompanyRelationTypes.Master,
                InCompanyPermissions = InCompanyPermissions.ObserveMarkupInBranch
            },
            new CustomerCompanyRelation
            {
                CompanyId = 1,
                BranchId = 1,
                CustomerId = 2,
                Type = CustomerCompanyRelationTypes.Regular,
                InCompanyPermissions = InCompanyPermissions.ObserveMarkupInCompany
            }
        };

        private static readonly CustomerInfo _customerInfo = CustomerInfoFactory.GetByWithCompanyAndBranch(10, 1, 1);
        private readonly CustomerService _customerService;
    }
}
