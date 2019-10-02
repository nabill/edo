using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Common.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PayfortSignatureService : IPayfortSignatureService
    {
        public PayfortSignatureService(ILogger<PayfortSignatureService> logger)
        {
            _logger = logger;
        }

        public string Calculate(IDictionary<string, string> model, string pass)
        {
            var filteredValues = model
                .Where(kv => kv.Key != "signature" && kv.Value != null)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}");
            var str = $"{pass}{string.Join("", filteredValues)}{pass}";
            using (var sha = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        public string Calculate(JObject model, string pass)
        {
            var dict = model.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>()?.ToString());
            return Calculate(dict, pass);
        }

        private readonly ILogger<PayfortSignatureService> _logger;
    }
}
