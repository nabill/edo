using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Companies;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICompanyService
    {
        Result<Company> Create(CompanyRegistrationInfo company);
    }
}