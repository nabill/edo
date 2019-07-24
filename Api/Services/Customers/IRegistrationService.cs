using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Companies;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface IRegistrationService
    {
        ValueTask<Result> RegisterMasterCustomer(CompanyRegistrationInfo company, CustomerRegistrationInfo masterCustomer);
    }
}