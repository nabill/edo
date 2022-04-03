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
    public async Task Default_settings_should_apply_if_agency_has_no_settings()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", IsEnabled = true },
            new() { Code = "illusions", IsEnabled = false }
        };

        var agencySystemSettings = new List<AgencySystemSettings>(0);
        
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.True(suppliers["netstorming"]);
        Assert.False(suppliers["illusions"]);
    }
    
    
    [Fact]
    public async Task Default_suppliers_should_merge_to_agency_suppliers()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", IsEnabled = true },
            new() { Code = "illusions", IsEnabled = false },
            new() { Code = "etg", IsEnabled = false }
        };

        var agencySystemSettings = new List<AgencySystemSettings>()
        {
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>()
                {
                    { "netstorming", true },
                    { "etg", false }
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
    public async Task Enabled_agency_supplier_should_not_override_disabled_default()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", IsEnabled = false }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", true }
                }
            }
        };
        
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.False(suppliers["netstorming"]);
    }
    
    
    [Fact]
    public async Task Check_disabled_agency_supplier()
    {
        var defaultSuppliers = new List<SlimSupplier>()
        {
            new() { Code = "netstorming", IsEnabled = true }
        };

        var agencySystemSettings = new List<AgencySystemSettings>
        {
            new()
            {
                AgencyId = 1,
                EnabledSuppliers = new Dictionary<string, bool>
                {
                    { "netstorming", false }
                }
            }
        };
        
        var service = CreateAgencySupplierManagementService(defaultSuppliers, agencySystemSettings);

        var (_, _, suppliers, _) = await service.GetMaterializedSuppliers(1);

        Assert.False(suppliers["netstorming"]);
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
        
        return edoContextMock.Object;
    }
}