using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface IInvitationService
    {
        Task<Result> SendInvitation(RegularCustomerInvitation registrationInfo);
        Task<Result> AcceptInvitation(CustomerRegistrationInfo customerRegistrationInfo, string invitationCode, string identity);
        Task<Result<CustomerRegistrationInfo>> GetInvitationInfo(string invitationCode);
    }
}