using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HappyTravel.Edo.Data
{
    public class EdoContextFactory : IDesignTimeDbContextFactory<EdoContext>
    {
        public EdoContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder<EdoContext>();
            var connectionString = string.Format("Server={0};Port={1};Database=edo;Userid={2};Password={3};",
                "116.203.114.8",
                "5433",
                "postgres",
                "WgQPXLDwFvwmIB0t");
			
            dbContextOptions.UseNpgsql(connectionString, builder => builder.UseNetTopologySuite());
			
            return new EdoContext(dbContextOptions.Options);
        }
    }
}