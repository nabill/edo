using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Suppliers;

namespace HappyTravel.Edo.Api.Services.SupplierOrders
{
    public class SupplierOrderService : ISupplierOrderService
    {
        public SupplierOrderService(EdoContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }
        
        
        public Task Add(string referenceCode, ServiceTypes serviceType, decimal supplierPrice)
        {
            var now = _dateTimeProvider.UtcNow();
            var supplierOrder = new SupplierOrder
            {
                Created = now,
                Modified = now,
                Price = supplierPrice,
                State = SupplierOrderState.Created,
                DataProvider = DataProviders.Netstorming,
                Type = serviceType,
                ReferenceCode = referenceCode
            };

            _context.Add(supplierOrder);
            return _context.SaveChangesAsync();
        }
        
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}