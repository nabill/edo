using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class BookingStatusUpdateOptions
    {
        public List<Suppliers> DisabledSuppliers { get; set; }
    }
}