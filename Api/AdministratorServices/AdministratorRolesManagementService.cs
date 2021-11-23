using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorRolesManagementService : IAdministratorRolesManagementService
    {
        public AdministratorRolesManagementService(EdoContext context)
        {
            _context = context;
        }


        public Task<List<AdministratorRoleInfo>> GetAll()
            => _context.AdministratorRoles
                .Select(r => r.ToAdministratorRoleInfo())
                .ToListAsync();


        public Task<Result> Add(AdministratorRoleInfo roleInfo)
        {
            return Validate(roleInfo)
                .Ensure(IsUnique, "A role with the same name or permission set already exists")
                .Tap(Add);


            async Task<bool> IsUnique()
                => !await _context.AdministratorRoles
                    .AnyAsync(r => r.Name.ToLower() == roleInfo.Name.ToLower() || r.Permissions == roleInfo.Permissions.ToFlags());


            Task Add()
            {
                _context.AdministratorRoles.Add(roleInfo.ToAdministratorRole());
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> Edit(int roleId, AdministratorRoleInfo roleInfo)
        {
            return await Validate(roleInfo)
                .Ensure(IsUnique, "A role with the same name or permission set already exists")
                .Bind(() => Get(roleId))
                .Tap(Edit);


            async Task<bool> IsUnique()
                => !await _context.AdministratorRoles
                    .AnyAsync(r => (r.Name.ToLower() == roleInfo.Name.ToLower() || r.Permissions == roleInfo.Permissions.ToFlags()) 
                        && r.Id != roleId);


            Task Edit(AdministratorRole role)
            {
                role.Name = roleInfo.Name;
                role.Permissions = roleInfo.Permissions.ToFlags();
                role.NotificationTypes = roleInfo.NotificationTypes ?? Array.Empty<NotificationTypes>();

                _context.Update(role);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> Delete(int roleId)
        {
            return await Get(roleId)
                .Ensure(IsUnused, "This role is in use and cannot be deleted")
                .Tap(Delete);


            async Task<bool> IsUnused(AdministratorRole _)
                => !await _context.Administrators.AnyAsync(r => r.AdministratorRoleIds.Contains(roleId));


            Task Delete(AdministratorRole role)
            {
                _context.AdministratorRoles.Remove(role);
                return _context.SaveChangesAsync();
            }
        }


        private async Task<Result<AdministratorRole>> Get(int roleId)
            => await _context.AdministratorRoles
                .SingleOrDefaultAsync(r => r.Id == roleId)
                    ?? Result.Failure<AdministratorRole>("A role with specified Id does not exist");


        private static Result Validate(AdministratorRoleInfo roleInfo)
            => GenericValidator<AdministratorRoleInfo>.Validate(v =>
                {
                    v.RuleFor(r => r.Name).NotEmpty();
                    v.RuleFor(r => r.Permissions).NotEmpty();
                },
                roleInfo);


        private readonly EdoContext _context;
    }
}
