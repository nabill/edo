using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Agents;

public class AgencySupplierSettings
{
    public int AgencyId { get; set; }
    public Dictionary<string, bool> EnabledSuppliers { get; set; }
}