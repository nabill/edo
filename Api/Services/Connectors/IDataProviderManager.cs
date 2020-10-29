using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface IDataProviderManager
    {
        IDataProvider Get(Suppliers key);
    }
}