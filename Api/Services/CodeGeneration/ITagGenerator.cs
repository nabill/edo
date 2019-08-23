using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.CodeGeneration
{
    public interface ITagGenerator
    {
        Task<string> GenerateReferenceCode(ServiceTypes serviceType, string destinationCode, long itineraryNumber);
        Task<long> GenerateItn();
    }
}