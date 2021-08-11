using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.MongoDb.Interfaces
{
    public interface IMongoDbStorage<TRecord>
    {
        IMongoQueryable<TRecord> Collection();
        Task Add(IEnumerable<TRecord> records);
        Task Add(TRecord record);
    }
}