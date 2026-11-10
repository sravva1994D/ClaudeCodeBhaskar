using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class UserRepository(PatientManagementDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
