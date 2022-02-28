namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        ISupplierConnector GetByCode(string supplierCode);
    }
}