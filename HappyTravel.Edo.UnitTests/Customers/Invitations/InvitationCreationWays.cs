using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.UnitTests.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.Invitations
{
    public class InvitationCreationWays
    {
        public InvitationCreationWays()
        {
            var customer = CustomerInfoFactory.CreateByWithCompanyAndBranch(It.IsAny<int>(), CustomerCompanyId, It.IsAny<int>());
            var customerContext = new Mock<ICustomerContext>();
            customerContext
                .Setup(c => c.GetCustomer())
                .ReturnsAsync(customer);

            _userInvitationService = new FakeUserInvitationService();
            var companyServiceMock = new Mock<ICompanyService>();

            companyServiceMock
                .Setup(c => c.Get(It.IsAny<int>()))
                .ReturnsAsync(Result.Ok(FakeCompanyInfo));

            var optionsMock = new Mock<IOptions<CustomerInvitationOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new CustomerInvitationOptions
            {
                EdoPublicUrl = It.IsAny<string>(),
                MailTemplateId = It.IsAny<string>()
            });

            _invitationService = new CustomerInvitationService(customerContext.Object,
                optionsMock.Object,
                _userInvitationService,
                companyServiceMock.Object);
        }


        [Fact]
        public async Task Different_ways_should_create_same_invitations()
        {
            var invitationInfo = new CustomerInvitationInfo(It.IsAny<CustomerRegistrationInfo>(),
                CustomerCompanyId, It.IsAny<string>());

            await _invitationService.Send(invitationInfo);
            await _invitationService.Create(invitationInfo);

            Assert.Equal(_userInvitationService.CreatedInvitationInfo.GetType(), _userInvitationService.SentInvitationInfo.GetType());
            Assert.Equal(_userInvitationService.CreatedInvitationInfo, _userInvitationService.SentInvitationInfo);
        }
        
        
        private readonly CustomerInvitationService _invitationService;
        private const int CustomerCompanyId = 123;

        private static readonly CompanyInfo FakeCompanyInfo =
            new CompanyInfo("SomeName", default, default, default, default, default, default, default, default, default);

        private readonly FakeUserInvitationService _userInvitationService;
    }

    public class FakeUserInvitationService : IUserInvitationService
    {
        public Task<Result> Send<TInvitationData, TMessagePayload>(string email, TInvitationData invitationInfo,
            Func<TInvitationData, string, TMessagePayload> messagePayloadGenerator, string mailTemplateId,
            UserInvitationTypes invitationType)
        {
            SentInvitationInfo = invitationInfo;
            return Task.FromResult(Result.Ok());
        }


        public Task<Result<string>> Create<TInvitationData>(string email, TInvitationData invitationInfo, UserInvitationTypes invitationType)
        {
            CreatedInvitationInfo = invitationInfo;
            return Task.FromResult(Result.Ok(string.Empty));
        }


        public Task Accept(string invitationCode) => throw new NotImplementedException();


        public Task<Result<TInvitationData>> GetPendingInvitation<TInvitationData>(string invitationCode, UserInvitationTypes invitationType)
            => throw new NotImplementedException();


        public object SentInvitationInfo { get; set; }

        public object CreatedInvitationInfo { get; set; }
    }
}