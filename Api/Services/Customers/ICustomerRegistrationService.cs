using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerRegistrationService
    {
        Task<Result> RegisterWithCompany(CustomerRegistrationInfo customerData, CompanyRegistrationInfo companyData, 
            string externalIdentity);

        Task<Result> RegisterInvited(CustomerRegistrationInfo requestCustomerRegistrationInfo,
            string invitationCode, string externalIdentity);
    }
}