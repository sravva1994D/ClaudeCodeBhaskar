using Microsoft.AspNetCore.Identity;
using PatientManagement.Business.Interfaces;

namespace PatientManagement.Web.Security;

// Adapter over the ASP.NET Core password hasher (salted PBKDF2; PDR-04: hasher only, custom Users table).
// It lives in the Web project so the Business layer holds no ASP.NET types.
public sealed class AspNetPasswordHasher : IPasswordHasher
{
    // The hasher's user parameter is not used for the hash, so no user object is needed.
    private static readonly PasswordHasher<object> Inner = new();

    public string Hash(string password) => Inner.HashPassword(null!, password);

    public bool Verify(string hash, string password) =>
        Inner.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
