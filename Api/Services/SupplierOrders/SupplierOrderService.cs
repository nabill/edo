using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.CreditCards.Services;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Suppliers;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.SupplierOrders
{
    public class SupplierOrderService : ISupplierOrderService
    {
        public SupplierOrderService(EdoContext context, ICreditCardProvider creditCardProvider, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _creditCardProvider = creditCardProvider;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task Add(string referenceCode, ServiceTypes serviceType, MoneyAmount convertedSupplierPrice, MoneyAmount originalSupplierPrice,
            Deadline deadline, Suppliers supplier, SupplierPaymentType paymentType, DateTime paymentDate)
        {
            var now = _dateTimeProvider.UtcNow();
            var supplierOrder = new SupplierOrder
            {
                Created = now,
                Modified = now,
                ConvertedPrice = convertedSupplierPrice.Amount,
                ConvertedCurrency = convertedSupplierPrice.Currency,
                Price = originalSupplierPrice.Amount,
                Currency = originalSupplierPrice.Currency,
                RefundableAmount = 0,
                State = SupplierOrderState.Created,
                Supplier = supplier,
                Type = serviceType,
                ReferenceCode = referenceCode,
                Deadline = deadline,
                PaymentDate = paymentDate,
                PaymentType = paymentType
            };

            _context.SupplierOrders.Add(supplierOrder);
            
            await _context.SaveChangesAsync();
            _context.Detach(supplierOrder);
        }


        public async Task Cancel(string referenceCode)
        {
            var orderToCancel = await _context.SupplierOrders
                .SingleOrDefaultAsync(o => o.ReferenceCode == referenceCode);

            if (orderToCancel == default)
                return;

            var applyingPolicy = orderToCancel.Deadline?.Policies
                .Where(p => p.FromDate >= _dateTimeProvider.UtcNow())
                .OrderByDescending(p => p.FromDate)
                .FirstOrDefault();

            if (applyingPolicy is not null)
                orderToCancel.RefundableAmount = (decimal) (100 - applyingPolicy.Percentage) * orderToCancel.Price;

            orderToCancel.State = SupplierOrderState.Canceled;
            _context.SupplierOrders.Update(orderToCancel);
            await _context.SaveChangesAsync();

            if (orderToCancel.PaymentType == SupplierPaymentType.CreditCard)
            {
                var moneyToCharge = orderToCancel.Price - orderToCancel.RefundableAmount;
                await _creditCardProvider.ProcessAmountChange(orderToCancel.ReferenceCode, new MoneyAmount(moneyToCharge, orderToCancel.Currency));
            }
        }
        
        
        public async Task Discard(string referenceCode)
        {
            var discardingOrder = await _context.SupplierOrders
                .SingleOrDefaultAsync(o => o.ReferenceCode == referenceCode);

            if (discardingOrder == default)
                return;

            discardingOrder.RefundableAmount = discardingOrder.Price;
            discardingOrder.State = SupplierOrderState.Discarded;
            _context.SupplierOrders.Update(discardingOrder);
            await _context.SaveChangesAsync();

            if (discardingOrder.PaymentType == SupplierPaymentType.CreditCard)
                await _creditCardProvider.ProcessAmountChange(discardingOrder.ReferenceCode, new MoneyAmount(0, discardingOrder.Currency));
        }


        private readonly EdoContext _context;
        private readonly ICreditCardProvider _creditCardProvider;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}