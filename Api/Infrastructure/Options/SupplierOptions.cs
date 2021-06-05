using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class SupplierOptions
    {
        public string Netstorming { get; set; }
        public string Illusions { get; set; }
        public string Etg { get; set; }
        public string DirectContracts { get; set; }
        public string Rakuten { get; set; }
        public string Columbus { get; set; }
        public string TravelgateXTest { get; set; }
        public string Jumeirah { get; set; }
        public List<Suppliers> EnabledSuppliers { get; set; }
    }
}
