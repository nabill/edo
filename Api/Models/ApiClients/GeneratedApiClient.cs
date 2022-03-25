using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.ApiClients;

public readonly struct GeneratedApiClient
{
    public GeneratedApiClient(string name, string password)
    {
        Name = name;
        Password = password;
    }
    
    
    /// <summary>
    /// Name for generated api client
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///  Password for generated api client
    /// </summary>
    public string Password { get; init; }
    
}