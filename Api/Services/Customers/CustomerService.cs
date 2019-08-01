using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
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

            var createdCustomer = new Customer
            {
                Title = customerRegistration.Title,
                FirstName = customerRegistration.FirstName,
                LastName = customerRegistration.LastName,
                Position = customerRegistration.Position,
                Email = customerRegistration.Email,
                TokenHash = HashGenerator.ComputeHash(customerRegistration.UserToken)
            };
            
            _context.Customers.Add(createdCustomer);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCustomer);
        }

        public async Task<Result<CustomerInfo>> GetByToken(string userToken)
        {
            var tokenHash = HashGenerator.ComputeHash(userToken);
            var customer = await _context.Customers.GetByTokenHash(tokenHash);
            
            return customer is null
                ? Result.Fail<CustomerInfo>("Could not find customer")
                : Result.Ok(new CustomerInfo(customer.Email,
                    customer.LastName,
                    customer.FirstName,
                    customer.Title,
                    customer.Position));
        }

        private async ValueTask<Result> Validate(CustomerRegistrationInfo customerRegistration)
        {
            var fieldValidateResult = GenericValidator<CustomerRegistrationInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Title).NotEmpty();
                v.RuleFor(c => c.FirstName).NotEmpty();
                v.RuleFor(c => c.LastName).NotEmpty();
                v.RuleFor(c => c.UserToken).NotEmpty();
                v.RuleFor(c => c.Email).NotEmpty().EmailAddress();
            }, customerRegistration);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return Result.Combine(
                await CheckTokenIsUnique(customerRegistration.UserToken),
                await CheckEmailIsUnique(customerRegistration.Email));
        }

        private async Task<Result> CheckTokenIsUnique(string token)
        {
            return await _context.Customers.AnyAsync(c => c.TokenHash == HashGenerator.ComputeHash(token))
                ? Result.Fail("User is already registered")
                : Result.Ok();
        }

        private async Task<Result> CheckEmailIsUnique(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email.ToLower().Equals(email.ToLower()))
                ? Result.Fail("Email is already in use")
                : Result.Ok();
        }
        
        private readonly EdoContext _context;
    }
}