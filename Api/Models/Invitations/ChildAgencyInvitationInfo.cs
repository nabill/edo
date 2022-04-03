using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Invitations
{
    public readonly struct ChildAgencyInvitationInfo
    {
        [JsonConstructor]
        public ChildAgencyInvitationInfo(string name)
        {
            Name = name;
        }


        /// <summary>
        ///     Name of the agency.
        /// </summary>
        [Required]
        public string Name { get; }
    }
}