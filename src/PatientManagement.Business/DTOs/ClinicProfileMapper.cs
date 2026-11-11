using PatientManagement.Data.Entities;

namespace PatientManagement.Business.DTOs;

// Maps the clinic profile entity to DTOs. A missing row reads as a blank header.
public static class ClinicProfileMapper
{
    public static ClinicProfileDto ToDto(ClinicProfile? profile) => new(
        profile?.ClinicName,
        profile?.Address,
        profile?.Phone,
        profile?.DoctorName,
        profile?.RegistrationNumber);

    // Blank text is stored as no text. Other text is kept, apart from surrounding spaces.
    public static ClinicProfile ToEntity(ClinicProfileInput input) => new()
    {
        ClinicName = Clean(input.ClinicName),
        Address = Clean(input.Address),
        Phone = Clean(input.Phone),
        DoctorName = Clean(input.DoctorName),
        RegistrationNumber = Clean(input.RegistrationNumber)
    };

    private static string? Clean(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
