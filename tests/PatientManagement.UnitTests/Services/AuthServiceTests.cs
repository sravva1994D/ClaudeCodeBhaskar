using NSubstitute;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for sign-in, forgot-password and the seed account. The repository and the hasher are substitutes,
// so no database or ASP.NET types are used. Test IDs follow CLAUDE.md §4.2 and §10.
public class AuthServiceTests
{
    private const string Username = "test.user";
    private const string Email = "test.user@example.test";
    private const string Password = "Abcdef1!";

    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher hasher = Substitute.For<IPasswordHasher>();
    private readonly AuthService service;

    public AuthServiceTests()
    {
        // A fake hash: "hashed:" plus the password. It is never the plain password, and it verifies only with that password.
        hasher.Hash(Arg.Any<string>()).Returns(call => "hashed:" + call.Arg<string>());
        hasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0) == "hashed:" + call.ArgAt<string>(1));

        service = new AuthService(users, hasher);
    }

    private static User ExistingUser() => new()
    {
        Id = 1,
        Username = Username,
        Email = Email,
        PasswordHash = "hashed:" + Password
    };

    [Fact]
    public async Task NFR_SEC_001_sign_in_succeeds_with_the_right_password()
    {
        users.GetByUsernameAsync(Username, Arg.Any<CancellationToken>()).Returns(ExistingUser());

        var result = await service.SignInAsync(Username, Password);

        Assert.True(result.IsSuccess);
        Assert.Equal(Username, result.Value!.Username);
    }

    [Fact]
    public async Task NFR_SEC_001_sign_in_fails_with_a_wrong_password()
    {
        users.GetByUsernameAsync(Username, Arg.Any<CancellationToken>()).Returns(ExistingUser());

        var result = await service.SignInAsync(Username, "Wrong-Pass1");

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthService.InvalidCredentialsCode, result.Errors.Single().Code);
    }

    [Fact]
    public async Task NFR_SEC_001_unknown_username_and_wrong_password_give_the_same_message()
    {
        users.GetByUsernameAsync(Username, Arg.Any<CancellationToken>()).Returns(ExistingUser());
        users.GetByUsernameAsync("unknown.user", Arg.Any<CancellationToken>()).Returns((User?)null);

        var wrongPassword = await service.SignInAsync(Username, "Wrong-Pass1");
        var unknownUser = await service.SignInAsync("unknown.user", Password);

        Assert.Equal(wrongPassword.Errors.Single(), unknownUser.Errors.Single());
    }

    [Fact]
    public async Task NFR_SEC_001_sign_in_with_empty_fields_fails_without_a_lookup()
    {
        var result = await service.SignInAsync("", "");

        Assert.False(result.IsSuccess);
        await users.DidNotReceiveWithAnyArgs().GetByUsernameAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_reset_with_no_email_match_shows_the_pdr_03_message()
    {
        users.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await service.ResetPasswordAsync(Email, "Newpass1!", "Newpass1!");

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthService.ResetNoMatchMessage, result.Errors.Single().Message);
        hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await users.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_reset_with_an_email_match_saves_the_new_hash_and_signs_in()
    {
        var user = ExistingUser();
        users.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(user);

        var result = await service.ResetPasswordAsync(Email, "Newpass1!", "Newpass1!");

        Assert.True(result.IsSuccess);
        Assert.Equal(Username, result.Value!.Username);
        Assert.Equal("hashed:Newpass1!", user.PasswordHash);
        await users.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NFR_SEC_001_reset_with_mismatched_confirmation_is_refused_before_any_lookup()
    {
        var result = await service.ResetPasswordAsync(Email, "Newpass1!", "Different1!");

        Assert.False(result.IsSuccess);
        Assert.Equal("Auth.PasswordMismatch", result.Errors.Single().Code);
        await users.DidNotReceiveWithAnyArgs().GetByEmailAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_reset_with_a_weak_password_returns_the_policy_errors_and_saves_nothing()
    {
        var result = await service.ResetPasswordAsync(Email, "weak", "weak");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Code == "Password.TooShort");
        await users.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_seed_account_is_created_with_a_hash_not_the_password()
    {
        users.AnyAsync(Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.CreateInitialAccountAsync(Username, Email, Password);

        Assert.True(result.IsSuccess);
        await users.Received(1).AddAsync(
            Arg.Is<User>(user =>
                user.Username == Username &&
                user.Email == Email &&
                user.PasswordHash == "hashed:" + Password &&
                user.PasswordHash != Password),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NFR_SEC_001_seed_account_is_refused_when_an_account_exists()
    {
        users.AnyAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await service.CreateInitialAccountAsync(Username, Email, Password);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthService.AccountExistsCode, result.Errors.Single().Code);
        await users.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_seed_account_requires_a_username_and_a_valid_email()
    {
        users.AnyAsync(Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.CreateInitialAccountAsync("  ", "not-an-email", Password);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Code == "Username.Required");
        Assert.Contains(result.Errors, error => error.Code == "Email.Invalid");
        await users.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task NFR_SEC_001_seed_account_applies_the_password_policy()
    {
        users.AnyAsync(Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.CreateInitialAccountAsync(Username, Email, "short");

        Assert.False(result.IsSuccess);
        await users.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
