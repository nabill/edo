using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Company;

public class CompanyBank
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string RoutingCode { get; set; }
    public string SwiftCode { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }

    public List<CompanyAccount> CompanyAccounts { get; set; }
}
