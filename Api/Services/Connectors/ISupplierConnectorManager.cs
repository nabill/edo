namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        ISupplierConnector Get(string supplierCode);
    }
}