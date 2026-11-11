using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Validators;

// Clinic header rules (Documentation/02-Planning/13-Validation-Strategy.md; PDR-15).
// All five fields are required on save (PDR-15, as restored on 2026-11-11), and each has a column length.
// A header that has never been saved still reads as blank; the print view shows a notice for that case.
public static class ClinicProfileValidator
{
    public const int ClinicNameMaxLength = 200;
    public const int AddressMaxLength = 500;
    public const int PhoneMaxLength = 20;
    public const int DoctorNameMaxLength = 200;
    public const int RegistrationNumberMaxLength = 100;

    public static IReadOnlyList<ResultMessage> Validate(ClinicProfileInput input)
    {
        var errors = new List<ResultMessage>();

        AddRequiredAndLengthErrors(input.ClinicName, ClinicNameMaxLength, "Clinic name", "ClinicName", errors);
        AddRequiredAndLengthErrors(input.Address, AddressMaxLength, "Address", "Address", errors);
        AddRequiredAndLengthErrors(input.Phone, PhoneMaxLength, "Phone", "Phone", errors);
        AddRequiredAndLengthErrors(input.DoctorName, DoctorNameMaxLength, "Doctor name", "DoctorName", errors);
        AddRequiredAndLengthErrors(input.RegistrationNumber, RegistrationNumberMaxLength, "Registration number", "RegistrationNumber", errors);

        return errors;
    }

    private static void AddRequiredAndLengthErrors(string? text, int maxLength, string label, string field, List<ResultMessage> errors)
    {
        var trimmed = text?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            errors.Add(new ResultMessage("ClinicProfile." + field + ".Required", $"{label} is required.", field));
            return;
        }

        if (trimmed.Length > maxLength)
        {
            errors.Add(new ResultMessage("ClinicProfile." + field + ".TooLong",
                $"{label} must be at most {maxLength} characters.", field));
        }
    }
}
