using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPayfortSignatureService
    {
        string Calculate(JObject model, string pass);
        string Calculate(IDictionary<string, string> model, string pass);
    }
}
