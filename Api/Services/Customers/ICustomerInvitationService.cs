using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerInvitationService
    {
        Task<Result> Send(CustomerInvitationInfo invitationInfo);

        Task Accept(string invitationCode);

        Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode);

        Task<Result<string>> Create(CustomerInvitationInfo request);
    }
}