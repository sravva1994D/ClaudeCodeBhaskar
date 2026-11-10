using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using PatientManagement.Business.Interfaces;

namespace PatientManagement.Web.Security;

// Sign-in cookie and global authorization (NFR-SEC-001; Documentation/02-Planning/12-Authentication-Design.md §4).
public static class SecurityServiceCollectionExtensions
{
    public const string CookieName = "PatientManagement.Auth";
    public const string LoginPath = "/Account/Login";
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromHours(24);

    public static IServiceCollection AddPatientManagementSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                // Secure only when the request is HTTPS. Localhost is plain HTTP (PDR-21).
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = IdleTimeout;
                options.SlidingExpiration = true;
                options.LoginPath = LoginPath;
            });

        // Every page needs a signed-in user unless it says otherwise (AllowAnonymous).
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
