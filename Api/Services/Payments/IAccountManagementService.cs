using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IAccountManagementService
    {
        Task<Result> CreateAccount(Company company, Currencies currency);
        Task<Result> ChangeCreditLimit(int accountId, decimal creditLimit);
    }
}