using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class SupplierOptions
    {
        public List<Suppliers> EnabledSuppliers { get; set; }
        public Dictionary<Suppliers, string> Endpoints { get; set; }
    }
}
