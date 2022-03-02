using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IMultiProviderAvailabilityStorage
    {
        Task<(string SupplierCode, TObject Result)[]> Get<TObject>(string keyPrefix, List<string> suppliers, bool isCachingEnabled = false);

        Task Save<TObjectType>(string keyPrefix, TObjectType @object, string supplierCode);
    }
}