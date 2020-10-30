using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Infrastructure.SupplierConnectors
{
    public interface IConnectorSecurityTokenManager
    {
        Task Refresh();

        Task<string> Get();
    }
}