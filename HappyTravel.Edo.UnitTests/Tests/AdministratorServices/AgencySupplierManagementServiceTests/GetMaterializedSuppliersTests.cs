using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.AgencySupplierManagementServiceTests;

public class GetMaterializedSuppliersTests
{
    [Fact]
    public async Task Test_disabled_supplier()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", EnablementState = EnablementState.Disabled },
            new() { Code = "illusions", EnablementState = EnablementState.Disabled },
            new() { Code = "etg", EnablementState = EnablementState.Disabled }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 2,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", true },
                    { "illusions", true }
                }
            },
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", true },
                    { "illusions", false }
                }
            }
        };
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.False(suppliers.ContainsKey("netstorming"));
        Assert.False(suppliers.ContainsKey("illusions"));
        Assert.False(suppliers.ContainsKey("etg"));
    }


    [Fact]
    public async Task Test_test_only_supplier()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", EnablementState = EnablementState.TestOnly },
            new() { Code = "illusions", EnablementState = EnablementState.TestOnly },
            new() { Code = "etg", EnablementState = EnablementState.TestOnly }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 2,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", true },
                    { "illusions", true }
                }
            },
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", true },
                    { "illusions", false }
                }
            }
        };
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.True(suppliers["netstorming"]);
        Assert.False(suppliers["illusions"]);
        Assert.False(suppliers["etg"]);
    }


    [Fact]
    public async Task Test_enabled_supplier()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", EnablementState = EnablementState.Enabled },
            new() { Code = "illusions", EnablementState = EnablementState.Enabled },
            new() { Code = "etg", EnablementState = EnablementState.Enabled }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 2,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "etg", true },
                    { "illusions", false }
                }
            },
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "etg", false },
                    { "netstorming", false },
                    { "illusions", true }
                }
            }
        };
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.False(suppliers["netstorming"]);
        Assert.False(suppliers["illusions"]);
        Assert.False(suppliers["etg"]);
    }


    [Fact]
    public async Task Test_enabled_supplier_without_root()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", EnablementState = EnablementState.Enabled },
            new() { Code = "illusions", EnablementState = EnablementState.Enabled },
            new() { Code = "etg", EnablementState = EnablementState.Enabled }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 2,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "etg", true },
                    { "illusions", false }
                }
            },
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "etg", false },
                    { "netstorming", false },
                    { "illusions", true }
                }
            }
        };
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(2);

        Assert.True(suppliers["netstorming"]);
        Assert.False(suppliers["illusions"]);
        Assert.True(suppliers["etg"]);
    }


    private IAgencySupplierManagementService CreateAgencySupplierManagementService(List<SlimSupplier> defaultSuppliers, List<AgencySystemSettings> agencySupplierSettings)
    {
        var suppliersOptionsStorage = GetSupplierOptionsStorage(defaultSuppliers);
        var dbContext = GetDbContext(agencySupplierSettings);
        var service = new AgencySupplierManagementService(dbContext, suppliersOptionsStorage);
        return service;
    }


    private ISupplierOptionsStorage GetSupplierOptionsStorage(List<SlimSupplier> defaultSuppliers)
    {
        var mock = new Mock<ISupplierOptionsStorage>();
        mock.Setup(m => m.GetAll()).Returns(Result.Success(defaultSuppliers));
        return mock.Object;
    }


    private EdoContext GetDbContext(List<AgencySystemSettings> agencySystemSettings)
    {
        var edoContextMock = new Mock<EdoContext>(new DbContextOptions<EdoContext>());

        var strategy = new ExecutionStrategyMock();

        var dbFacade = new Mock<DatabaseFacade>(edoContextMock.Object);
        dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
        edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

        edoContextMock
            .Setup(c => c.AgencySystemSettings)
            .Returns(DbSetMockProvider.GetDbSetMock(agencySystemSettings));

        edoContextMock
            .Setup(c => c.Agencies)
            .Returns(DbSetMockProvider.GetDbSetMock(agencies));

        return edoContextMock.Object;
    }

    private List<Agency> agencies = new()
    {
        new Agency
        {
            Id = 1,
            ParentId = 2
        },
        new Agency
        {
            Id = 2
        }
    };
}