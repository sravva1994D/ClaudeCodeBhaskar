using PatientManagement.Business.Validators;

namespace PatientManagement.UnitTests.Validators;

// Unit tests for the password policy (PDR-02). Test IDs follow CLAUDE.md §4.2 and §10.
public class PasswordPolicyValidatorTests
{
    private static IReadOnlyList<string> CodesFor(string? password) =>
        PasswordPolicyValidator.Validate(password).Select(message => message.Code).ToArray();

    [Fact]
    public void NFR_SEC_001_accepts_a_valid_8_character_password()
    {
        Assert.Empty(PasswordPolicyValidator.Validate("Abcdef1!"));
    }

    [Fact]
    public void NFR_SEC_001_accepts_a_valid_16_character_password()
    {
        Assert.Empty(PasswordPolicyValidator.Validate("Abcdefghijklmn1!"));
    }

    [Fact]
    public void NFR_SEC_001_rejects_a_7_character_password_as_too_short()
    {
        Assert.Equal(["Password.TooShort"], CodesFor("Abcde1!"));
    }

    [Fact]
    public void NFR_SEC_001_rejects_a_17_character_password_as_too_long()
    {
        Assert.Equal(["Password.TooLong"], CodesFor("Abcdefghijklmno1!"));
    }

    [Fact]
    public void NFR_SEC_001_requires_an_upper_case_letter()
    {
        Assert.Equal(["Password.NoUpperCase"], CodesFor("abcdef1!"));
    }

    [Fact]
    public void NFR_SEC_001_requires_a_lower_case_letter()
    {
        Assert.Equal(["Password.NoLowerCase"], CodesFor("ABCDEF1!"));
    }

    [Fact]
    public void NFR_SEC_001_requires_a_digit()
    {
        Assert.Equal(["Password.NoDigit"], CodesFor("Abcdefgh!"));
    }

    [Fact]
    public void NFR_SEC_001_requires_a_symbol()
    {
        Assert.Equal(["Password.NoSymbol"], CodesFor("Abcdefg1"));
    }

    [Fact]
    public void NFR_SEC_001_whitespace_does_not_count_as_a_symbol()
    {
        Assert.Equal(["Password.NoSymbol"], CodesFor("Abcdef1 "));
    }

    [Fact]
    public void NFR_SEC_001_reports_every_failed_rule_at_once()
    {
        Assert.Equal(
            ["Password.TooShort", "Password.NoUpperCase", "Password.NoDigit", "Password.NoSymbol"],
            CodesFor("abcdef"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NFR_SEC_001_requires_a_password_to_be_entered(string? password)
    {
        Assert.Equal(["Password.Required"], CodesFor(password));
    }
}
