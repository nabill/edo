using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupServiceTests;

public class MarkupFunctionTypeOrderingTests
{
    [Fact]
    public void Policies_in_scope_should_be_ordered_by_function_type()
    {
        var policyService = CreateMarkupPolicyService(Policies);
        var subjectInfo = new MarkupSubjectInfo
        {
            AgencyId = 22,
            AgentId = 33,
            AgencyAncestors = new List<int>()
        };

        var orderedPolicies = policyService.Get(subjectInfo, new MarkupDestinationInfo());
        
        Assert.Equal(3, orderedPolicies[0].Id);
        Assert.Equal(1, orderedPolicies[1].Id);
        Assert.Equal(2, orderedPolicies[2].Id);
    }


    private static IMarkupPolicyService CreateMarkupPolicyService(List<MarkupPolicy> policies)
    {
        var policyStorageMock = new Mock<IMarkupPolicyStorage>();
        policyStorageMock.Setup(s => s.Get(It.IsAny<Func<MarkupPolicy,bool>>())).Returns(policies);
        return new MarkupPolicyService(policyStorageMock.Object);
    }


    private static readonly List<MarkupPolicy> Policies = new()
    {
        new MarkupPolicy
        {
            Id = 1,
            SubjectScopeType = SubjectMarkupScopeTypes.Agent,
            SubjectScopeId = "33",
            FunctionType = MarkupFunctionType.Percent
        },
        new MarkupPolicy
        {
            Id = 2,
            FunctionType = MarkupFunctionType.Fixed,
        },
        new MarkupPolicy
        {
            Id = 3,
            SubjectScopeType = SubjectMarkupScopeTypes.Agency,
            SubjectScopeId = "22",
            FunctionType = MarkupFunctionType.Percent,
        },
    };
       
}