using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Hubs.Search
{
    public interface ISearchHub
    {
        public Task SearchStateChanged(Guid searchId);
    }
}