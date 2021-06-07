using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        ISupplierConnector Get(Suppliers key);
    }
}