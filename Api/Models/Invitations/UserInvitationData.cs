﻿using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Users;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Invitations
{
    public readonly struct UserInvitationData
    {
        [JsonConstructor]
        public UserInvitationData(UserDescriptionInfo userRegistrationInfo,
            // Think about removing such a special property from there
            RegistrationAgencyInfo childAgencyRegistrationInfo,
            int[] roleIds)
        {
            UserRegistrationInfo = userRegistrationInfo;
            ChildAgencyRegistrationInfo = childAgencyRegistrationInfo;
            RoleIds = roleIds;
        }

        /// <summary>
        /// Prefilled user registration info.
        /// </summary>
        [Required]
        public UserDescriptionInfo UserRegistrationInfo { get; }

        /// <summary>
        /// Prefilled child agency registration info. Used only for child agency invitations.
        /// </summary>
        public RegistrationAgencyInfo ChildAgencyRegistrationInfo { get; }

        
        /// <summary>
        /// Role Ids assigned to the user
        /// </summary>
        public int[] RoleIds { get; }

        public override int GetHashCode()
            => (UserRegistrationInfo, ChildAgencyRegistrationInfo, AgentRoleIds: RoleIds).GetHashCode();


        public bool Equals(UserInvitationData other)
            => UserRegistrationInfo.Equals(other.UserRegistrationInfo)
                && ChildAgencyRegistrationInfo.Equals(other.ChildAgencyRegistrationInfo)
                && RoleIds.Equals(other.RoleIds);


        public override bool Equals(object obj)
            => obj is UserInvitationData other && Equals(other);
    }
}
