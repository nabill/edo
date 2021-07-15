using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Management.HttpBasedAdministratorContextTests
{
    public class HasPermissionTests
    {
        public HasPermissionTests()
        {
            var edoContextMock = MockEdoContextFactory.Create();
            edoContextMock.Setup(x => x.Administrators)
                .Returns(DbSetMockProvider.GetDbSetMock(_administrators));
            edoContextMock.Setup(x => x.AdministratorRoles)
                .Returns(DbSetMockProvider.GetDbSetMock(_administratorRoles));

            var tokenInfoAccessorMock = new Mock<ITokenInfoAccessor>();
            tokenInfoAccessorMock.Setup(x => x.GetIdentity())
                .Returns("hash");

            _administratorContext = new HttpBasedAdministratorContext(edoContextMock.Object, tokenInfoAccessorMock.Object);
        }
        
        
        [Fact]
        public async Task Should_pass_if_admin_has_permission()
        {
            var hasPermission = await _administratorContext.HasPermission(AdministratorPermissions.AccountReplenish);
            
            Assert.True(hasPermission);
        }


        [Fact]
        public async Task Should_fail_if_admin_doesnt_have_permission()
        {
            var hasPermission = await _administratorContext.HasPermission(AdministratorPermissions.BookingManagement);
            
            Assert.False(hasPermission);
        }
        
        
        private readonly IEnumerable<Administrator> _administrators = new[]
        {
            new Administrator
            {
                Id = 0, 
                AdministratorRoleIds = new []{0},
                IdentityHash = HashGenerator.ComputeSha256("hash"),
                IsActive = true
            }
        };


        private readonly IEnumerable<AdministratorRole> _administratorRoles = new[]
        {
            new AdministratorRole
            {
                Id = 0,
                Permissions = AdministratorPermissions.AccountReplenish | AdministratorPermissions.AdministratorInvitation
            }
        };


        private readonly IAdministratorContext _administratorContext;
    }
}