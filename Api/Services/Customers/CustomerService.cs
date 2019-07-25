using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(EdoContext context, HashGenerator hashGenerator)
        {
            _context = context;
            _hashGenerator = hashGenerator;
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
                TokenHash = _hashGenerator.ComputeHash(customerRegistration.UserToken)
            };
            
            _context.Customers.Add(createdCustomer);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCustomer);
        }

        public IList<Currency> GetAvailableCurrencies()
        {
            return Enum.GetValues(typeof(Currency))
                .Cast<Currency>()
                .ToList();
        }

        public IList<PaymentMethod> GetAvailablePaymentMethods()
        {
            return Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .ToList();
        }

        private async ValueTask<Result> Validate(CustomerRegistrationInfo customerRegistration)
        {
            return Result.Combine(
                CheckNotEmpty(customerRegistration.Email, nameof(customerRegistration.Email)),
                CheckNotEmpty(customerRegistration.FirstName, nameof(customerRegistration.FirstName)),
                CheckNotEmpty(customerRegistration.LastName, nameof(customerRegistration.LastName)),
                CheckNotEmpty(customerRegistration.UserToken, nameof(customerRegistration.UserToken)),
                CheckNotEmpty(customerRegistration.Title, nameof(customerRegistration.Title)),
                await CheckEmailIsUnique(customerRegistration.Email),
                await CheckTokenIsUnique(customerRegistration.UserToken));
        }

        private async Task<Result> CheckTokenIsUnique(string token)
        {
            return await _context.Customers.AnyAsync(c => c.TokenHash == _hashGenerator.ComputeHash(token))
                ? Result.Fail("User is already registered")
                : Result.Ok();
        }

        private async Task<Result> CheckEmailIsUnique(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                ? Result.Fail("Email is already in use")
                : Result.Ok();
        }

        private static Result CheckNotEmpty(string value, string propertyName)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? Result.Fail($"Value of {propertyName} cannot be empty") 
                : Result.Ok();
        }
        
        private readonly EdoContext _context;
        private readonly HashGenerator _hashGenerator;
    }
}