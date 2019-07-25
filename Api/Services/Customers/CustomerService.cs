using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;

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
            var (_, isFailure, error) = Validate(customerRegistration);
            if (isFailure)
                return Result.Fail<Customer>(error);

            var createdCustomer = new Customer
            {
                Title = customerRegistration.Title,
                FirstName = customerRegistration.FirstName,
                LastName = customerRegistration.LastName,
                Position = customerRegistration.Position,
                Email = customerRegistration.Email,
                TokenHash = _hashGenerator.GetHash(customerRegistration.UserToken)
            };
            
            _context.Customers.Add(createdCustomer);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCustomer);
        }

        private Result Validate(in CustomerRegistrationInfo customerRegistration)
        {
            return Result.Combine(
                CheckNotEmpty(customerRegistration.Email, nameof(customerRegistration.Email)),
                CheckNotEmpty(customerRegistration.FirstName, nameof(customerRegistration.FirstName)),
                CheckNotEmpty(customerRegistration.LastName, nameof(customerRegistration.LastName)),
                CheckNotEmpty(customerRegistration.UserToken, nameof(customerRegistration.UserToken)),
                CheckNotEmpty(customerRegistration.Title, nameof(customerRegistration.Title)));
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