using System;
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
            AgencyInfo childAgencyRegistrationInfo)
        {
            UserRegistrationInfo = userRegistrationInfo;
            ChildAgencyRegistrationInfo = childAgencyRegistrationInfo;
        }

        /// <summary>
        /// Prefilled user registration info.
        /// </summary>
        [Required]
        public UserDescriptionInfo UserRegistrationInfo { get; }

        /// <summary>
        /// Prefilled child agency registration info. Used only for child agency invitations.
        /// </summary>
        public AgencyInfo ChildAgencyRegistrationInfo { get; }
    }
}
