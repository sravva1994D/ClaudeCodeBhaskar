using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;

namespace PatientManagement.Business;

/// <summary>
/// Registers the business layer: service interfaces and implementations (CLAUDE.md §6).
/// </summary>
public static class BusinessServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        // The system clock, unless a test replaces it (CLAUDE.md §10).
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<AgeCalculator>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
