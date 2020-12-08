using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPaymentService : IMarkupPaymentService
    {
        public MarkupPaymentService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result> Pay(string referenceCode)
        {
            return await GetMarkupBooking();


            async Task<Result<MoneyAmount>> GetMarkupBooking()
            {
                var query = from markup in _context.MarkupPolicies
                    join bookingMarkup in _context.BookingMarkups on markup.Id equals bookingMarkup.PolicyId
                    where bookingMarkup.ReferenceCode == referenceCode && markup.ScopeType == MarkupPolicyScopeType.Agent
                    select new MoneyAmount(bookingMarkup.Amount, bookingMarkup.Currency);

                var moneyAmount = await query.SingleOrDefaultAsync();

                return moneyAmount.Equals(default)
                    ? Result.Failure<MoneyAmount>("Nothing to pay")
                    : moneyAmount;
            }
        }


        private readonly EdoContext _context;
    }
}