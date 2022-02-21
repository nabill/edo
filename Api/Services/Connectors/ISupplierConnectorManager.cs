
using System;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface ISupplierConnectorManager
    {
        [Obsolete("Integer supplier Id will be removed soon. Use GetByCode method instead")]
        ISupplierConnector Get(int key);

        ISupplierConnector GetByCode(string supplierCode);
    }
}