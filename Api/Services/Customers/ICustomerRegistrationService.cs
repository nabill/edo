using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerRegistrationService
    {
        Task<Result> RegisterWithCounterparty(CustomerEditableInfo customerData, CounterpartyInfo counterpartyData,
            string externalIdentity, string email);


        Task<Result> RegisterInvited(CustomerEditableInfo registrationInfo,
            string invitationCode, string externalIdentity, string email);
    }
}