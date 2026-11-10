using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Sign-in, forgot-password and the seed account (Documentation/02-Planning/10-Service-Design.md §2.1, DEC-018).
public interface IAuthService
{
    Task<Result<SignedInUser>> SignInAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<Result<SignedInUser>> ResetPasswordAsync(
        string email, string newPassword, string confirmPassword, CancellationToken cancellationToken = default);

    Task<Result<SignedInUser>> CreateInitialAccountAsync(
        string username, string email, string password, CancellationToken cancellationToken = default);
}
