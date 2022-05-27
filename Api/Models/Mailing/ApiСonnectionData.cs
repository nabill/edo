using HappyTravel.Edo.Api.Models.Bookings.Vouchers;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing;

public class ApiConnectionData : DataWithCompanyInfo
{
    public ApiConnectionData(string agencyName, string agencyId)
    {
        AgencyName = agencyName;
        AgencyId = agencyId;
    }


    public string AgencyName { get; }

    public string AgencyId { get; }
}
