using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly EdoContext _context;

        public PaymentService(EdoContext context, ICustomerContext customerContext)
        {
            _context = context;
            _customerContext = customerContext;
        }

        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();

        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();
        private readonly ICustomerContext _customerContext;

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);

        public async Task<Result<List<CardInfo>>> GetAvailableCards()
        {
            var (_, companyFailture, company, companyError) = await _customerContext.GetCompany();
            if (!companyFailture)
                return Result.Fail<List<CardInfo>>(companyError);

            var (_, customerFailture, customer, customerError) = await _customerContext.GetCompany();
            if (!customerFailture)
                return Result.Fail<List<CardInfo>>(customerError);

            var query = from card in _context.Cards
                join compCard in _context.CompanyCardRelations on card.Id equals compCard.CardId into companyCards
                from companyCard in companyCards.DefaultIfEmpty()
                join cusCard in _context.CustomerCardRelations on card.Id equals cusCard.CardId into customerCards
                from customerCard in customerCards.DefaultIfEmpty()
                where companyCard.CompanyId == company.Id || customerCard.CustomerId == customer.Id
                select ToCardInfo(card, companyCard != null ? CardOwner.Company : CardOwner.Customer);

            var cards = await query.ToListAsync();

            return Result.Ok(cards);
        }

        static CardInfo ToCardInfo(Card card, CardOwner owner) =>
            new CardInfo()
            {
                CardHolderName = card.CardHolderName,
                CardNumber = card.CardNumber,
                ExpiryDate = card.ExpiryDate,
                Id = card.Id,
                Owner = owner
            };
    }
}