using System;

namespace HappyTravel.Edo.Api.Services.Versioning
{
    public class VersionService : IVersionService
    {
        public string Get()
        {
            var version = Environment.GetEnvironmentVariable(VersionVariableName);
            return string.IsNullOrWhiteSpace(version) 
                ? VersionPlaceholder 
                : version;
        }


        private const string VersionPlaceholder = "undefined/local";
        private const string VersionVariableName = "BUILD_VERSION";
    }
}
