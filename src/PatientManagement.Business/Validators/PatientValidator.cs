using System.Net.Mail;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Validators;

// Patient rules (Documentation/02-Planning/13-Validation-Strategy.md §3.1, §3.2; PDR-01, PDR-22, D-M2-3, D4).
// Returns one error per failed rule. Errors block the save. Duplicate warnings are handled by PatientService.
public static class PatientValidator
{
    public const int FullNameMaxLength = 200;
    public const int PhoneMaxLength = 20;
    public const int EmailMaxLength = 256;
    public const int AddressMaxLength = 500;

    public const int PhoneMinDigits = 7;
    public const int PhoneMaxDigits = 15;

    // PDR-01: the only gender values.
    public static readonly IReadOnlyList<string> AllowedGenders = ["Male", "Female", "Other"];

    public static IReadOnlyList<ResultMessage> Validate(PatientInput input, DateOnly today)
    {
        var errors = new List<ResultMessage>();

        ValidateFullName(input.FullName, errors);
        ValidateDateOfBirth(input.DateOfBirth, today, errors);
        ValidateGender(input.Gender, errors);
        ValidatePhone(input.Phone, errors);
        ValidateEmail(input.Email, errors);
        ValidateAddress(input.Address, errors);

        return errors;
    }

    // An empty search term is an error (D4, approved 2026-11-10). Any other term is searched as entered.
    public static IReadOnlyList<ResultMessage> ValidateSearchTerm(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return [new ResultMessage("Search.Empty", "Enter a name or phone number to search.", "Term")];
        }

        return [];
    }

    private static void ValidateFullName(string? fullName, List<ResultMessage> errors)
    {
        var value = fullName?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            errors.Add(new ResultMessage("Patient.FullName.Required", "Enter the full name.", "FullName"));
        }
        else if (value.Length > FullNameMaxLength)
        {
            errors.Add(new ResultMessage("Patient.FullName.TooLong",
                $"The full name must be at most {FullNameMaxLength} characters.", "FullName"));
        }
    }

    private static void ValidateDateOfBirth(DateOnly? dateOfBirth, DateOnly today, List<ResultMessage> errors)
    {
        if (dateOfBirth is null)
        {
            errors.Add(new ResultMessage("Patient.DateOfBirth.Required", "Enter the date of birth.", "DateOfBirth"));
        }
        else if (dateOfBirth.Value > today)
        {
            errors.Add(new ResultMessage("Patient.DateOfBirth.Future",
                "The date of birth cannot be in the future.", "DateOfBirth"));
        }
    }

    private static void ValidateGender(string? gender, List<ResultMessage> errors)
    {
        if (string.IsNullOrWhiteSpace(gender))
        {
            errors.Add(new ResultMessage("Patient.Gender.Required", "Select a gender.", "Gender"));
        }
        else if (!AllowedGenders.Contains(gender))
        {
            errors.Add(new ResultMessage("Patient.Gender.Invalid", "Select Male, Female or Other.", "Gender"));
        }
    }

    // 7 to 15 digits. A single leading + is allowed, and spaces and hyphens may separate the digits (PDR-22).
    private static void ValidatePhone(string? phone, List<ResultMessage> errors)
    {
        var value = phone?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            errors.Add(new ResultMessage("Patient.Phone.Required", "Enter the phone number.", "Phone"));
            return;
        }

        if (value.Length > PhoneMaxLength)
        {
            errors.Add(new ResultMessage("Patient.Phone.TooLong",
                $"The phone number must be at most {PhoneMaxLength} characters.", "Phone"));
            return;
        }

        var digits = value.Count(char.IsAsciiDigit);
        var hasOnlyAllowedCharacters = value.All(c => char.IsAsciiDigit(c) || c == ' ' || c == '-' || c == '+');
        var plusIsOnlyFirst = value.IndexOf('+', 1) < 0;

        if (!hasOnlyAllowedCharacters || !plusIsOnlyFirst || digits < PhoneMinDigits || digits > PhoneMaxDigits)
        {
            errors.Add(new ResultMessage("Patient.Phone.Invalid",
                $"The phone number must have {PhoneMinDigits} to {PhoneMaxDigits} digits. Spaces, hyphens and a leading + are allowed.",
                "Phone"));
        }
    }

    private static void ValidateEmail(string? email, List<ResultMessage> errors)
    {
        var value = email?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            errors.Add(new ResultMessage("Patient.Email.Required", "Enter the email address.", "Email"));
            return;
        }

        if (value.Length > EmailMaxLength)
        {
            errors.Add(new ResultMessage("Patient.Email.TooLong",
                $"The email address must be at most {EmailMaxLength} characters.", "Email"));
            return;
        }

        if (!IsStandardEmail(value))
        {
            errors.Add(new ResultMessage("Patient.Email.Invalid", "Enter a valid email address.", "Email"));
        }
    }

    private static void ValidateAddress(string? address, List<ResultMessage> errors)
    {
        var value = address?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            errors.Add(new ResultMessage("Patient.Address.Required", "Enter the address.", "Address"));
        }
        else if (value.Length > AddressMaxLength)
        {
            errors.Add(new ResultMessage("Patient.Address.TooLong",
                $"The address must be at most {AddressMaxLength} characters.", "Address"));
        }
    }

    // A single plain address. Display-name forms such as "Name <a@b>" are refused.
    private static bool IsStandardEmail(string value)
    {
        try
        {
            return new MailAddress(value).Address == value;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
