using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Companies;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(EdoContext context)
        {
            _context = context;
        }
        
        public async Task<Result<Customer>> Create(CustomerRegistrationInfo customerRegistration)
        {
            var (_, isFailure, error) = await Validate(customerRegistration);
            if (isFailure)
                return Result.Fail<Customer>(error);

            var createdCustomer = new Customer()
            {
                Title = customerRegistration.Title,
                FirstName = customerRegistration.FirstName,
                LastName = customerRegistration.LastName,
                Position = customerRegistration.Position,
                Email = customerRegistration.Email,
                Login = customerRegistration.Login
            };
            
            _context.Customers.Add(createdCustomer);

            return Result.Ok(createdCustomer);
        }

        private async Task<Result> Validate(CustomerRegistrationInfo customerRegistration)
        {
            var isLoginAlreadyUsed = await _context.Customers
                .AnyAsync(c => c.Login.ToLower().Equals(customerRegistration.Login.ToLower()));
            
            if (isLoginAlreadyUsed)
                return Result.Fail("Login is already in use");

            var isEmailAlreadyUsed = await _context.Customers
                .AnyAsync(c => c.Email.ToLower().Equals(customerRegistration.Email.ToLower()));
            
            if(isEmailAlreadyUsed)
                return Result.Fail("Email address is already in use");
            
            // TODO: further validation

            return Result.Ok();
        }
        
        private readonly EdoContext _context;
    }
}