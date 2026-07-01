using FaithFlow.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Extensions;

public static class DatabaseExtensions
{
    public static bool IsPostgresProvider(IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"];
        return string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "PostgreSQL", StringComparison.OrdinalIgnoreCase);
    }

    public static void AddFaithFlowDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (IsPostgresProvider(configuration))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });
    }

    public static void ApplyFaithFlowMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (IsPostgresProvider(app.Configuration))
        {
            context.Database.Migrate();
            return;
        }

        if (app.Environment.IsDevelopment())
        {
            context.Database.EnsureCreated();
        }
    }
}
