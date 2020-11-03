using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IMultiProviderAvailabilityStorage
    {
        Task<(Suppliers Supplier, TObject Result)[]> Get<TObject>(string keyPrefix, List<Suppliers> suppliers, bool isCachingEnabled = false);

        Task Save<TObjectType>(string keyPrefix, TObjectType @object, Suppliers supplier);
    }
}