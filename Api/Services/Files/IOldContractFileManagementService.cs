using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IOldContractFileManagementService
    {
        Task<string> ReuploadToAgencies();
    }
}