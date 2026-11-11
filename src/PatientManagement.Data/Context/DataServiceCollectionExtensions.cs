using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Repositories;

namespace PatientManagement.Data.Context;

/// <summary>
/// Registers the data layer: the DbContext and, later, the repositories (AD-04).
/// </summary>
public static class DataServiceCollectionExtensions
{
    public const string ConnectionStringName = "PatientManagement";

    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                "Set it in appsettings.Development.json or with 'dotnet user-secrets'. " +
                "See Documentation/04-Implementation/01-Setup-Guide.md.");

        services.AddDbContext<PatientManagementDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
