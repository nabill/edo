using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class BookingOptions
    {
        public List<Suppliers> DisableStatusUpdateForSuppliers { get; set; }
    }
}