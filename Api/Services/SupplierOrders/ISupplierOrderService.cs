using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.SupplierOrders
{
    public interface ISupplierOrderService
    {
        Task Add(string referenceCode, ServiceTypes serviceType, MoneyAmount convertedPrice, MoneyAmount supplierPrice, Deadline deadline, Suppliers supplier, DateTime paymentDate);

        Task Cancel(string referenceCode);

        Task Discard(string referenceCode);
    }
}