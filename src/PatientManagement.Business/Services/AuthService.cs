using System.Net.Mail;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

// Rules: Documentation/02-Planning/10-Service-Design.md §2.1, 12-Authentication-Design.md §3.
// Sign-in gives the same message for an unknown username and a wrong password.
public sealed class AuthService(IUserRepository users, IPasswordHasher hasher) : IAuthService
{
    public const string InvalidCredentialsCode = "Auth.InvalidCredentials";
    public const string InvalidCredentialsMessage = "The username or password is not correct.";

    // PDR-03 wording, shown when the email does not match an account.
    public const string ResetNoMatchCode = "Auth.ResetNoMatch";
    public const string ResetNoMatchMessage = "If the email matches an account, the password has been reset.";

    public const string AccountExistsCode = "Seed.AccountExists";

    public async Task<Result<SignedInUser>> SignInAsync(
        string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return InvalidCredentials();
        }

        var user = await users.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !hasher.Verify(user.PasswordHash, password))
        {
            return InvalidCredentials();
        }

        return Result<SignedInUser>.Ok(ToSignedInUser(user));
    }

    public async Task<Result<SignedInUser>> ResetPasswordAsync(
        string email, string newPassword, string confirmPassword, CancellationToken cancellationToken = default)
    {
        var errors = new List<ResultMessage>();

        if (!IsValidEmail(email))
        {
            errors.Add(EmailError());
        }

        if (newPassword != confirmPassword)
        {
            errors.Add(new ResultMessage("Auth.PasswordMismatch",
                "The new password and the confirmation do not match.", "ConfirmPassword"));
        }

        errors.AddRange(PasswordPolicyValidator.Validate(newPassword));

        if (errors.Count > 0)
        {
            return Result<SignedInUser>.Fail(errors.ToArray());
        }

        var user = await users.GetByEmailAsync(email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result<SignedInUser>.Fail(new ResultMessage(ResetNoMatchCode, ResetNoMatchMessage));
        }

        user.PasswordHash = hasher.Hash(newPassword);
        await users.UpdateAsync(user, cancellationToken);

        return Result<SignedInUser>.Ok(ToSignedInUser(user));
    }

    public async Task<Result<SignedInUser>> CreateInitialAccountAsync(
        string username, string email, string password, CancellationToken cancellationToken = default)
    {
        var errors = new List<ResultMessage>();

        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add(new ResultMessage("Username.Required", "Enter a username.", "Username"));
        }

        if (!IsValidEmail(email))
        {
            errors.Add(EmailError());
        }

        errors.AddRange(PasswordPolicyValidator.Validate(password));

        if (errors.Count > 0)
        {
            return Result<SignedInUser>.Fail(errors.ToArray());
        }

        if (await users.AnyAsync(cancellationToken))
        {
            return Result<SignedInUser>.Fail(new ResultMessage(
                AccountExistsCode, "An account already exists. Nothing was changed."));
        }

        var user = new User
        {
            Username = username.Trim(),
            Email = email.Trim(),
            PasswordHash = hasher.Hash(password)
        };

        await users.AddAsync(user, cancellationToken);

        return Result<SignedInUser>.Ok(ToSignedInUser(user));
    }

    private static Result<SignedInUser> InvalidCredentials() =>
        Result<SignedInUser>.Fail(new ResultMessage(InvalidCredentialsCode, InvalidCredentialsMessage));

    private static ResultMessage EmailError() =>
        new("Email.Invalid", "Enter a valid email address.", "Email");

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            return new MailAddress(email.Trim()).Address.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static SignedInUser ToSignedInUser(User user) => new(user.Id, user.Username);
}
