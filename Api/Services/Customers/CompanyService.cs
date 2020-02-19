using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Branches;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(EdoContext context,
            IAccountManagementService accountManagementService,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService,
            ICustomerContext customerContext, 
            ICustomerPermissionManagementService permissionManagementService)
        {
            _context = context;
            _accountManagementService = accountManagementService;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
            _customerContext = customerContext;
            _permissionManagementService = permissionManagementService;
        }


        public async Task<Result<Company>> Add(CompanyRegistrationInfo company)
        {
            var (_, isFailure, error) = Validate(company);
            if (isFailure)
                return Result.Fail<Company>(error);

            var now = _dateTimeProvider.UtcNow();
            var createdCompany = new Company
            {
                Address = company.Address,
                City = company.City,
                CountryCode = company.CountryCode,
                Fax = company.Fax,
                Name = company.Name,
                Phone = company.Phone,
                Website = company.Website,
                PostalCode = company.PostalCode,
                PreferredCurrency = company.PreferredCurrency,
                PreferredPaymentMethod = company.PreferredPaymentMethod,
                State = CompanyStates.PendingVerification,
                Created = now,
                Updated = now
            };

            _context.Companies.Add(createdCompany);
            await _context.SaveChangesAsync();
            
            var defaultBranch = new Branch
            {
                Title = createdCompany.Name,
                CompanyId = createdCompany.Id,
                IsDefault = true,
                Created = now,
                Modified = now,
            };
            _context.Branches.Add(defaultBranch);
            
            await _context.SaveChangesAsync();
            return Result.Ok(createdCompany);
        }


        public Task<Result<Branch>> AddBranch(int companyId, BranchInfo branch)
        {
            return CheckCompanyExists()
                .Ensure(HasPermissions, "Permission to create branches denied")
                .Ensure(IsBranchTitleUnique, $"Branch with title {branch.Title} already exists")
                .OnSuccess(SaveBranch);


            async Task<bool> HasPermissions()
            {
                var (_, isFailure, customerInfo, _) = await _customerContext.GetCustomerInfo();
                if (isFailure)
                    return false;

                return customerInfo.IsMaster && customerInfo.CompanyId == companyId;
            }


            async Task<Result> CheckCompanyExists()
            {
                return await _context.Companies.AnyAsync(c => c.Id == companyId)
                    ? Result.Ok()
                    : Result.Fail("Could not find company with specified id");
            }


            async Task<bool> IsBranchTitleUnique()
            {
                return !await _context.Branches.Where(b => b.CompanyId == companyId &&
                        EF.Functions.ILike(b.Title, branch.Title))
                    .AnyAsync();
            }

            
            async Task<Branch> SaveBranch()
            {
                var now = _dateTimeProvider.UtcNow();
                var createdBranch = new Branch
                {
                    Title = branch.Title,
                    CompanyId = companyId,
                    IsDefault = false,
                    Created = now,
                    Modified = now,
                };
                _context.Branches.Add(createdBranch);
                await _context.SaveChangesAsync();

                return createdBranch;
            }
        }


        public Task<Branch> GetDefaultBranch(int companyId)
            => _context.Branches
                .SingleAsync(b => b.CompanyId == companyId && b.IsDefault);


        public Task<Result> VerifyAsFullyAccessed(int companyId, string verificationReason)
        {
            return Verify(companyId, company => Result.Ok(company)
                    .OnSuccess(c => SetVerified(c, CompanyStates.FullAccess, verificationReason))
                    .OnSuccess(_ => Task.FromResult(Result.Ok())) // HACK: conversion hack because can't map tasks
                    .OnSuccess(() => SetPermissions(companyId, GetPermissionSet))
                    .OnSuccess(() => WriteToAuditLog(companyId, verificationReason)));


            InCompanyPermissions GetPermissionSet(bool isMaster)
                => isMaster 
                    ? PermissionSets.FullAccessMaster 
                    : PermissionSets.FullAccessDefault;
        }


        public Task<Result> VerifyAsReadOnly(int companyId, string verificationReason)
        {
            return Verify(companyId, company => Result.Ok(company)
                    .OnSuccess(c => SetVerified(c, CompanyStates.ReadOnly, verificationReason))
                    .OnSuccess(CreatePaymentAccount)
                    .OnSuccess(() => SetPermissions(companyId, GetPermissionSet))
                    .OnSuccess(() => WriteToAuditLog(companyId, verificationReason)));


            Task<Result> CreatePaymentAccount(Company company)
                => _accountManagementService
                    .Create(company, company.PreferredCurrency);


            InCompanyPermissions GetPermissionSet(bool isMaster)
                => isMaster 
                    ? PermissionSets.ReadOnlyMaster 
                    : PermissionSets.ReadOnlyDefault;
        }


        private Task<List<CustomerContainer>> GetCustomers(int companyId)
            => _context.CustomerCompanyRelations
                .Where(r => r.CompanyId == companyId)
                .Select(r => new CustomerContainer(r.CustomerId, r.BranchId, r.Type))
                .ToListAsync();


        private async Task<Result> SetPermissions(int companyId, Func<bool, InCompanyPermissions> isMasterCondition)
        {
            foreach (var customer in await GetCustomers(companyId))
            {
                var permissions = isMasterCondition.Invoke(customer.Type == CustomerCompanyRelationTypes.Master);
                var (_, isFailure, error) = await _permissionManagementService.SetInCompanyPermissions(companyId, customer.BranchId, customer.Id, permissions);
                if (isFailure)
                    return Result.Fail(error);
            }

            return Result.Ok();
        }


        private Task SetVerified(Company company, CompanyStates state, string verificationReason)
        {
            var now = _dateTimeProvider.UtcNow();
            string reason;
            if (string.IsNullOrEmpty(company.VerificationReason))
                reason = verificationReason;
            else
                reason = company.VerificationReason + Environment.NewLine + verificationReason;

            company.State = state;
            company.VerificationReason = reason;
            company.Verified = now;
            company.Updated = now;
            _context.Update(company);

            return _context.SaveChangesAsync();
        }


        private Task<Result> Verify(int companyId, Func<Company, Task<Result>> verificationFunc)
        {
            return Result.Ok()
                .Ensure(HasVerificationRights, "Permission denied")
                .OnSuccess(GetCompany)
                .OnSuccessWithTransaction(_context, verificationFunc);


            Task<bool> HasVerificationRights() => Task.FromResult(true);// _administratorContext.HasPermission(AdministratorPermissions.CompanyVerification);


            async Task<Result<Company>> GetCompany()
            {
                var company = await _context.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
                return company == default
                    ? Result.Fail<Company>($"Could not find company with id {companyId}")
                    : Result.Ok(company);
            }
        }


        private static Result Validate(in CompanyRegistrationInfo companyRegistration)
        {
            return GenericValidator<CompanyRegistrationInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty().Matches(@"^[0-9]{3,30}$");
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$").When(i => !string.IsNullOrWhiteSpace(i.Fax));
            }, companyRegistration);
        }


        private Task WriteToAuditLog(int companyId, string verificationReason) 
            => _managementAuditService.Write(ManagementEventType.CompanyVerification, new CompanyVerifiedAuditEventData(companyId, verificationReason));


        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerPermissionManagementService _permissionManagementService;
        private readonly IManagementAuditService _managementAuditService;


        private readonly struct CustomerContainer
        {
            public CustomerContainer(int id, int branchId, CustomerCompanyRelationTypes type)
            {
                Id = id;
                BranchId = branchId;
                Type = type;
            }


            public int Id { get; }
            public int BranchId { get; }
            public CustomerCompanyRelationTypes Type { get; }
        }
    }
}