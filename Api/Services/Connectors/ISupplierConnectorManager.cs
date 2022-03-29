using HappyTravel.Edo.Api.Services.CurrencyConversion;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        ISupplierConnector Get(string supplierCode, ClientTypes? clientType = null);
    }
}