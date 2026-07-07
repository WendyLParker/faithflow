using FaithFlow.Backend.Data;
using FaithFlow.Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FaithFlow.Backend;

/// <summary>
/// Used by EF Core CLI (dotnet ef migrations/database update) at design time.
/// Reads the same appsettings.json configuration as the running app so migrations
/// are generated/applied against whichever provider (SQLite or Postgres) is
/// actually configured for the current environment, instead of always assuming Postgres.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=faithflow.db";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (DatabaseExtensions.IsPostgresProvider(configuration))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
