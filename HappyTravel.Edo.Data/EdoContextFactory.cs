using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Edo.Data
{
    public class EdoContextFactory : IDesignTimeDbContextFactory<EdoContext>
    {
        public EdoContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder<EdoContext>();
            dbContextOptions.UseNpgsql(GetConnectionString(), builder => builder.UseNetTopologySuite());
            return new EdoContext(dbContextOptions.Options);
        }


        private static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("contextSettings.json", false, true)
                .Build();

            var dbOptions = GetDbOptions(configuration);
            
            return string.Format(ConnectionStringTemplate,
                dbOptions["host"],
                dbOptions["port"],
                dbOptions["userId"],
                dbOptions["password"]);
        }


        private static Dictionary<string, string> GetDbOptions(IConfiguration configuration)
        {
            using var vaultClient = new VaultClient.VaultClient(new VaultOptions
            {
                BaseUrl = new Uri(Environment.GetEnvironmentVariable(configuration["Vault:Endpoint"]), UriKind.Absolute),
                Engine = configuration["Vault:Engine"],
                Role = configuration["Vault:Role"]
            });
            vaultClient.Login(Environment.GetEnvironmentVariable(configuration["Vault:Token"])).Wait();

            return vaultClient.Get(configuration["EDO:Database:Options"]).Result;
        }


        private const string ConnectionStringTemplate = "Server={0};Port={1};Database=edo;Userid={2};Password={3};";
    }
}