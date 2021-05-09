using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IIdentityUserInfoService
    {
        Task<string> GetUserEmail();
    }
}