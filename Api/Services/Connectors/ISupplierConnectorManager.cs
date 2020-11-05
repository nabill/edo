using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        ISupplierConnector Get(Suppliers key);
    }
}