using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InvitationInfoExtensions
    {
        public static UserInvitationData ToUserInvitationData(this UserDescriptionInfo info)
            => new (info, default, default);


        public static UserInvitationData ToUserInvitationData(this RegisterInvitedAgencyRequest request)
            => new (request.RegistrationInfo, request.ChildAgencyRegistrationInfo, default);


        public static UserInvitationData ToUserInvitationData(this CreateChildAgencyInvitationRequest request, int[] roleIds)
            => new (request.UserRegistrationInfo,
                new RegistrationAgencyInfo(
                    request.ChildAgencyRegistrationInfo.Name,
                    request.ChildAgencyRegistrationInfo.Address,
                    request.ChildAgencyRegistrationInfo.BillingEmail,
                    request.ChildAgencyRegistrationInfo.City,
                    request.ChildAgencyRegistrationInfo.CountryCode,
                    request.ChildAgencyRegistrationInfo.Fax,
                    request.ChildAgencyRegistrationInfo.Phone,
                    request.ChildAgencyRegistrationInfo.PostalCode,
                    request.ChildAgencyRegistrationInfo.Website,
                    request.ChildAgencyRegistrationInfo.VatNumber,
                    string.Empty,
                    PaymentTypes.None),
                roleIds);

        
        public static UserInvitationData ToUserInvitationData(this SendAgentInvitationRequest request)
            => new (request.RegistrationInfo, default, request.RoleIds);
        
        
        public static UserInvitationData ToUserInvitationData(this SendAdminInvitationRequest request)
            => new (request.RegistrationInfo, default, request.RoleIds);
    }
}
