using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class CreditCardService : ICreditCardService
    {
        public CreditCardService(EdoContext context, ICustomerContext customerContext)
        {
            _context = context;
            _customerContext = customerContext;
        }

        public async Task<Result<CreditCardInfo[]>> GetAvailableCards()
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (!companyFailure)
                return Result.Fail<CreditCardInfo[]>(companyError);

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCompany();
            if (!customerFailure)
                return Result.Fail<CreditCardInfo[]>(customerError);

            var cards = await _context.CreditCards
                    .Where(card => card.CompanyId == company.Id || card.CustomerId == customer.Id)
                    .Select(card => ToCardInfo(card))
                    .ToArrayAsync();

            return Result.Ok(cards);
        }

        public async Task<Result> CanUseCard(int cardId)
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (!companyFailure)
                return Result.Fail(companyError);

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (!customerFailure)
                return Result.Fail(customerError);

            var customerId = customer.Id;
            var companyId = company.Id;
            var query = _context.CreditCards
                .Where(card =>  card.Id == cardId && (card.CompanyId == customerId || card.CustomerId == companyId));

            return await query.AnyAsync()
                ? Result.Ok()
                : Result.Fail("User cannot pay with selected payment card");
        }

        public async Task<Result> Create(CreditCard card)
        { 
            _context.CreditCards.Add(card);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        private static CreditCardInfo ToCardInfo(CreditCard card) =>
            new CreditCardInfo()
            {
                HolderName = card.HolderName,
                Number = card.Number,
                ExpirationDate = card.ExpirationDate,
                Id = card.Id,
                OwnerType = card.CompanyId != null ? CreditCardOwnerType.Company : CreditCardOwnerType.Customer
            };

        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
    }
}
