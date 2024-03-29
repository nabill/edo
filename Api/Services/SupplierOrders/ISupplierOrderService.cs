using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.SupplierOrders
{
    public interface ISupplierOrderService
    {
        Task Add(string referenceCode, ServiceTypes serviceType, MoneyAmount convertedPrice, MoneyAmount supplierPrice, Deadline deadline, string supplierCode,
            SupplierPaymentType paymentType, DateTimeOffset paymentDate);

        Task Cancel(string referenceCode);

        Task Discard(string referenceCode);
    }
}