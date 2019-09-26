using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyManagementService : IMarkupPolicyManagementService
    {
        public MarkupPolicyManagementService(EdoContext context,
            ICustomerContext customerContext,
            IAdministratorContext administratorContext,
            IDateTimeProvider dateTimeProvider,
            IMarkupPolicyTemplateService templateService)
        {
            _context = context;
            _customerContext = customerContext;
            _administratorContext = administratorContext;
            _dateTimeProvider = dateTimeProvider;
            _templateService = templateService;
        }

        public Task<Result> AddPolicy(MarkupPolicyData policyData)
        {
            return ValidatePolicy(policyData)
                .OnSuccess(CheckPermissions)
                .OnSuccess(CreateMarkupFunction)
                .OnSuccess(SavePolicy);
            
            Task<Result> CheckPermissions()
            {
                return CheckUserManagePermissions(policyData.Scope);
            }
            
            Result<Expression<Func<decimal, decimal>>> CreateMarkupFunction()
            {
                var templateId = policyData.Settings.TemplateId;
                var templateSettings = policyData.Settings.TemplateSettings;
                
                return _templateService
                    .CreateExpression(templateId, templateSettings);
            }
            
            async Task<Result> SavePolicy(Expression<Func<decimal, decimal>> expression)
            {
                var now = _dateTimeProvider.UtcNow();
                var scope = policyData.Scope;
                
                var policy = new MarkupPolicy
                {
                    Description = policyData.Settings.Description,
                    Order = policyData.Settings.Order,
                    ScopeType = scope.Type,
                    Target = policyData.Target,
                    BranchId = scope.BranchId,
                    CompanyId = scope.CompanyId,
                    CustomerId = scope.CustomerId,
                    TemplateSettings = policyData.Settings.TemplateSettings,
                    Created = now,
                    Modified = now,
                    TemplateId = policyData.Settings.TemplateId,
                    Function = expression
                };
                
                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }

        public Task<Result> DeletePolicy(int policyId)
        {
            return GetPolicyScope()
                .OnSuccess(CheckUserManagePermissions)
                .OnSuccess(DeletePolicy);
            
            async Task<Result<MarkupPolicyScope>> GetPolicyScope()
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
                if (policy == null)
                    return Result.Fail<MarkupPolicyScope>("Could not find policy");

                var scopeType = policy.ScopeType;
                var scopeData = new MarkupPolicyScope(scopeType,
                    policy.CompanyId, 
                    policy.BranchId,
                    policy.CustomerId);
                
                return Result.Ok(scopeData);
            }

            async Task DeletePolicy()
            {
                var policy = await _context.MarkupPolicies
                    .SingleOrDefaultAsync(p => p.Id == policyId);
                
                _context.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Result> UpdatePolicy(int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Fail("Could not find policy");
            
            return await Result.Ok()
                .OnSuccess(CheckPermissions)
                .OnSuccess(CreateMarkupFunction)
                .OnSuccess(UpdatePolicy);

            Task<Result> CheckPermissions()
            {
                var scopeData = new MarkupPolicyScope(policy.ScopeType,
                    policy.CompanyId,
                    policy.BranchId,
                    policy.CustomerId);

                return CheckUserManagePermissions(scopeData);
            }
            
            Result<Expression<Func<decimal, decimal>>> CreateMarkupFunction()
            {
                return _templateService
                    .CreateExpression(settings.TemplateId, settings.TemplateSettings);
            }

            async Task<Result> UpdatePolicy(Expression<Func<decimal, decimal>> expression)
            {
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Function = expression;
                policy.Modified = _dateTimeProvider.UtcNow();

                var validateResult = ValidatePolicy(GetPolicyData(policy));
                if (validateResult.IsFailure)
                    return validateResult;
                
                _context.Update(policy);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }

        public async Task<Result<List<MarkupPolicyData>>> GetPoliciesForScope(MarkupPolicyScope scope)
        {
            var (_, isFailure, error) = await CheckUserManagePermissions(scope);
            if (isFailure)
                return Result.Fail<List<MarkupPolicyData>>(error);

            var policies = (await GetPolicies())
                .Select(GetPolicyData)
                .ToList();

            return Result.Ok(policies);

            Task<List<MarkupPolicy>> GetPolicies()
            {
                switch (scope.Type)
                {
                    case MarkupPolicyScopeType.Global:
                    {
                        return _context.MarkupPolicies
                            .Where(p => p.ScopeType == MarkupPolicyScopeType.Global)
                            .ToListAsync();
                    }
                    case MarkupPolicyScopeType.Company:
                    {
                        return _context.MarkupPolicies
                            .Where(p => p.ScopeType == MarkupPolicyScopeType.Company && p.CompanyId == scope.CompanyId)
                            .ToListAsync();
                    }
                    case MarkupPolicyScopeType.Branch:
                    {
                        return _context.MarkupPolicies
                            .Where(p => p.ScopeType == MarkupPolicyScopeType.Company && p.BranchId == scope.BranchId)
                            .ToListAsync();
                    }
                    case MarkupPolicyScopeType.Customer:
                    {
                        return _context.MarkupPolicies
                            .Where(p => p.ScopeType == MarkupPolicyScopeType.Company && p.CustomerId == scope.CustomerId)
                            .ToListAsync();
                    }
                    default:
                    {
                        return Task.FromResult(new List<MarkupPolicy>(0));
                    }
                }
            }
        }
     
        private async Task<Result> CheckUserManagePermissions(MarkupPolicyScope scope)
        {
            var hasAdminPermissions = await _administratorContext.HasPermission(AdministratorPermissions.MarkupManagement);
            if (hasAdminPermissions)
                return Result.Ok();
            
            var (_, isFailure, customerData, error) = await _customerContext.GetCustomerInfo();
            if (isFailure)
                return Result.Fail(error);

            switch (scope.Type)
            {
                case MarkupPolicyScopeType.Customer:
                {
                    var isMasterCustomerInUserCompany = customerData.Company.Id == scope.CompanyId 
                        && customerData.IsMaster;

                    return isMasterCustomerInUserCompany
                        ? Result.Ok()
                        : Result.Fail("Permission denied");
                }
                case MarkupPolicyScopeType.Branch:
                {
                    var branch = await _context.Branches
                        .SingleOrDefaultAsync(b => b.Id == scope.BranchId);
                    
                    if(branch == null)
                        return Result.Fail("Could not find branch");
                    
                    var isMasterCustomerInBranchCompany = customerData.Company.Id == branch.CompanyId
                        && customerData.IsMaster;

                    return isMasterCustomerInBranchCompany
                        ? Result.Ok()
                        : Result.Fail("Permission denied");
                }
                default:
                    return Result.Fail("Permission denied");
            }
        }

        private static MarkupPolicyData GetPolicyData(MarkupPolicy policy)
        {
            return new MarkupPolicyData(policy.Target,
                new MarkupPolicySettings(policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order),
                new MarkupPolicyScope(policy.ScopeType,
                    policy.CompanyId,
                    policy.BranchId, 
                    policy.CustomerId));
        }
        
        private Result ValidatePolicy(MarkupPolicyData policyData)
        {
            return Result.Ok()
                .Ensure(ScopeIsValid, "Invalid scope data")
                .Ensure(TargetIsValid, "Invalid policy target");

            bool ScopeIsValid()
            {
                var scope = policyData.Scope;
                switch (scope.Type)
                {
                    case MarkupPolicyScopeType.Global:
                    {
                        return scope.CompanyId == null &&
                            scope.CustomerId == null &&
                            scope.BranchId == null;
                    }
                    case MarkupPolicyScopeType.Company:
                    {
                        return scope.CompanyId != null &&
                            scope.CustomerId == null &&
                            scope.BranchId == null;
                    }
                    case MarkupPolicyScopeType.Branch:
                    {
                        return scope.CompanyId == null &&
                            scope.CustomerId == null &&
                            scope.BranchId != null;
                    }
                    case MarkupPolicyScopeType.Customer:
                    {
                        return scope.CompanyId == null &&
                            scope.CustomerId != null &&
                            scope.BranchId == null;
                    }
                    default: return false;
                }
            }

            bool TargetIsValid() => policyData.Target != MarkupPolicyTarget.NotSpecified;
        }
        
        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IAdministratorContext _administratorContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMarkupPolicyTemplateService _templateService;
    }
}