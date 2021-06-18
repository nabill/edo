using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IFixHtIdService
    {
        Task FillEmptyHtIds();
    }
}