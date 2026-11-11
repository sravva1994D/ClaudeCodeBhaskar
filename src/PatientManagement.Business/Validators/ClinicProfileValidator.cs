using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Validators;

// Clinic header rules (Documentation/02-Planning/13-Validation-Strategy.md; PDR-15; D-M5-6).
// Fields may be blank: an incomplete header is shown as a notice on the print view, not refused here.
// Only the column lengths are enforced.
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

        AddLengthError(input.ClinicName, ClinicNameMaxLength, "Clinic name", "ClinicName", errors);
        AddLengthError(input.Address, AddressMaxLength, "Address", "Address", errors);
        AddLengthError(input.Phone, PhoneMaxLength, "Phone", "Phone", errors);
        AddLengthError(input.DoctorName, DoctorNameMaxLength, "Doctor name", "DoctorName", errors);
        AddLengthError(input.RegistrationNumber, RegistrationNumberMaxLength, "Registration number", "RegistrationNumber", errors);

        return errors;
    }

    private static void AddLengthError(string? text, int maxLength, string label, string field, List<ResultMessage> errors)
    {
        if (text is not null && text.Trim().Length > maxLength)
        {
            errors.Add(new ResultMessage("ClinicProfile." + field + ".TooLong",
                $"{label} must be at most {maxLength} characters.", field));
        }
    }
}
