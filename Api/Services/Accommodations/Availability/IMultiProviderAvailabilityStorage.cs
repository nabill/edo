using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IMultiProviderAvailabilityStorage
    {
        Task<(int SupplierId, TObject Result)[]> Get<TObject>(string keyPrefix, List<int> suppliers, bool isCachingEnabled = false);

        Task Save<TObjectType>(string keyPrefix, TObjectType @object, int supplierId);
    }
}