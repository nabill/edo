using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class DataProviderOptions
    {
        public string Netstorming { get; set; }
        public string Illusions { get; set; }
        public string Etg { get; set; }
        public List<Common.Enums.Suppliers> EnabledProviders { get; set; }
    }
}
