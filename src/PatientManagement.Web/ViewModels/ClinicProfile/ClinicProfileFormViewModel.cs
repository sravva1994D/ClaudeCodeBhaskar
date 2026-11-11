using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.ClinicProfile;

// The clinic header form (SCR-14; FR-RX-002, PDR-15). Every field is optional; the service checks the lengths.
// Field names match the service's error fields, so each error appears next to its input.
public sealed class ClinicProfileFormViewModel
{
    public string? ClinicName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? DoctorName { get; set; }

    public string? RegistrationNumber { get; set; }

    public static ClinicProfileFormViewModel From(ClinicProfileDto profile) => new()
    {
        ClinicName = profile.ClinicName,
        Address = profile.Address,
        Phone = profile.Phone,
        DoctorName = profile.DoctorName,
        RegistrationNumber = profile.RegistrationNumber
    };

    public ClinicProfileInput ToInput() => new(ClinicName, Address, Phone, DoctorName, RegistrationNumber);
}
