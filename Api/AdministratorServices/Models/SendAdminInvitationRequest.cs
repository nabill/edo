using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Users;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct SendAdminInvitationRequest
    {
        [JsonConstructor]
        public SendAdminInvitationRequest(UserDescriptionInfo registrationInfo, int[] roleIds)
        {
            RegistrationInfo = registrationInfo;
            RoleIds = roleIds;
        }


        public void Deconstruct(out UserDescriptionInfo registrationInfo, out int[] roleIds)
        {
            registrationInfo = RegistrationInfo;
            roleIds = RoleIds;
        }
        
        
        /// <summary>
        ///    Registration information
        /// </summary>
        [Required]
        public UserDescriptionInfo RegistrationInfo { get; }
        
        
        /// <summary>
        ///     Roles for admin
        /// </summary>
        [Required]
        public int[] RoleIds { get; }
    }
}