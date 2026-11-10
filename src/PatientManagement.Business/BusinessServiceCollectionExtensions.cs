using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
