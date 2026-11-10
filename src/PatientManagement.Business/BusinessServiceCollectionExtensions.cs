using Microsoft.Extensions.DependencyInjection;

namespace PatientManagement.Business;

/// <summary>
/// Registers the business layer: service interfaces and implementations (CLAUDE.md §6).
/// Services are registered with the feature work that needs them.
/// </summary>
public static class BusinessServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        return services;
    }
}
