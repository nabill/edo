using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PayfortSignatureService : IPayfortSignatureService
    {
        public PayfortSignatureService(ILogger<PayfortSignatureService> logger, IOptions<PayfortOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Result<string> Calculate(IDictionary<string, string> model, SignatureTypes type)
        {
            if (!Enum.IsDefined(typeof(SignatureTypes), type) || type == SignatureTypes.Unknown)
            {
                return Result.Fail<string>("Invalid signature type");
            }

            var pass = type == SignatureTypes.Request ? _options.ShaRequestPhrase : _options.ShaResponsePhrase;
            var filteredValues = model
                .Where(kv => kv.Key != "signature" && kv.Value != null)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}");
            
            var str = $"{pass}{string.Join("", filteredValues)}{pass}";
            using (var sha = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                return Result.Ok(BitConverter.ToString(hash).Replace("-", String.Empty).ToLower());
            }
        }

        public Result<string> Calculate(JObject model, SignatureTypes type)
        {
            var dict = model.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>()?.ToString());
            return Calculate(dict, type);
        }

        private readonly ILogger<PayfortSignatureService> _logger;
        private readonly PayfortOptions _options;
    }
}
