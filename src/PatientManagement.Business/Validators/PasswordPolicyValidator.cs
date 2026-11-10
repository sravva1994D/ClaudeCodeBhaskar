using PatientManagement.Business.Common;

namespace PatientManagement.Business.Validators;

// Password rules (PDR-02; Documentation/02-Planning/13-Validation-Strategy.md §3.7):
// 8 to 16 characters, with at least one upper case letter, lower case letter, digit and symbol.
// A symbol is any character that is not a letter, a digit or whitespace. Whitespace is allowed but does not count as a symbol.
// Returns one error per failed rule. An empty result means the password meets the policy.
public static class PasswordPolicyValidator
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 16;
    private const string Field = "Password";

    public static IReadOnlyList<ResultMessage> Validate(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return [new ResultMessage("Password.Required", "Enter a password.", Field)];
        }

        var errors = new List<ResultMessage>();

        if (password.Length < MinimumLength)
        {
            errors.Add(new ResultMessage("Password.TooShort",
                $"The password must be at least {MinimumLength} characters.", Field));
        }

        if (password.Length > MaximumLength)
        {
            errors.Add(new ResultMessage("Password.TooLong",
                $"The password must be at most {MaximumLength} characters.", Field));
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add(new ResultMessage("Password.NoUpperCase",
                "The password must include an upper case letter.", Field));
        }

        if (!password.Any(char.IsLower))
        {
            errors.Add(new ResultMessage("Password.NoLowerCase",
                "The password must include a lower case letter.", Field));
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add(new ResultMessage("Password.NoDigit",
                "The password must include a digit.", Field));
        }

        if (!password.Any(IsSymbol))
        {
            errors.Add(new ResultMessage("Password.NoSymbol",
                "The password must include a symbol.", Field));
        }

        return errors;
    }

    private static bool IsSymbol(char character) =>
        !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character);
}
