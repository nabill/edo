using System;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InvitationInfoExtensions
    {
        public static UserInvitationData ToUserInvitationData(this UserDescriptionInfo info, string email = null)
        {
            var newInfo = new UserDescriptionInfo(info.Title, info.FirstName, info.LastName, info.Position, email ?? info.Email);
            return new UserInvitationData(newInfo, default);
        }


        public static UserInvitationData ToUserInvitationData(this RegisterInvitedAgencyRequest request)
            => new UserInvitationData(request.RegistrationInfo, request.ChildAgencyRegistrationInfo);


        public static UserInvitationData ToUserInvitationData(this CreateChildAgencyInvitationRequest request)
            => new UserInvitationData(request.UserRegistrationInfo, request.ChildAgencyRegistrationInfo.ToAgencyInfo());


        private static AgencyInfo ToAgencyInfo(this ChildAgencyRegistrationInfo info)
            => new AgencyInfo(info.Name, default, default, info.Address, info.BillingEmail, info.City, info.CountryCode, info.CountryName,
                info.Fax, info.Phone, info.PostalCode, info.Website, info.VatNumber);
    }
}
