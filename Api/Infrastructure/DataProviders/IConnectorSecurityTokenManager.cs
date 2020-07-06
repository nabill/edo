using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Infrastructure.DataProviders
{
    public interface IConnectorSecurityTokenManager
    {
        Task Refresh();

        Task<string> Get();
    }
}