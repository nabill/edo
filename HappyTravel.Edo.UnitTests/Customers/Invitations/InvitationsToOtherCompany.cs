using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.UnitTests.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.Invitations
{
    public class InvitationsToOtherCompany
    {
        public InvitationsToOtherCompany()
        {
            var customer = CustomerInfoFactory.CreateByWithCompanyAndBranch(It.IsAny<int>(), CustomerCompanyId, It.IsAny<int>());
            var customerContext = new Mock<ICustomerContext>();
            customerContext
                .Setup(c => c.GetCustomer())
                .ReturnsAsync(customer);
            
            _invitationService = new CustomerInvitationService(customerContext.Object,
                Mock.Of<IOptions<CustomerInvitationOptions>>(),
                Mock.Of<IUserInvitationService>(),
                Mock.Of<ICompanyService>());
        }
        
        [Fact]
        public async Task Sending_invitation_to_other_company_should_be_permitted()
        {
            var invitationInfoWithOtherCompany = new CustomerInvitationInfo(It.IsAny<CustomerEditableInfo>(),
                OtherCompanyId, It.IsAny<string>());
            
            var (_, isFailure, _) = await _invitationService.Send(invitationInfoWithOtherCompany);
            
            Assert.True(isFailure);
        }
        
        [Fact]
        public async Task Creating_invitation_to_other_company_should_be_permitted()
        {
            var invitationInfoWithOtherCompany = new CustomerInvitationInfo(It.IsAny<CustomerEditableInfo>(),
                OtherCompanyId, It.IsAny<string>());
            
            var (_, isFailure, _, _) = await _invitationService.Create(invitationInfoWithOtherCompany);
            
            Assert.True(isFailure);
        }
        
        private readonly CustomerInvitationService _invitationService;
        private const int CustomerCompanyId = 123;
        private const int OtherCompanyId = 122;
    }
}