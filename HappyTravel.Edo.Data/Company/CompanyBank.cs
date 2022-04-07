using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Company;

public class CompanyBank
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string RoutingCode { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }

    public List<CompanyAccount>? CompanyAccounts { get; set; }
}
