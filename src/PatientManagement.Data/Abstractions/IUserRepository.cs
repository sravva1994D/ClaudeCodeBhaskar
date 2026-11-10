using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to the single account (Documentation/02-Planning/11-Repository-Design.md §2.1).
// There is no single-account fallback: sign-in always matches the username (DEC-018).
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Saves the entity's current values, for example a new password hash.
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
