using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class BookingStatusUpdate
    {
        public List<Suppliers> DisabledSuppliers { get; set; }
    }
}