using PatientManagement.Data.Entities;

namespace PatientManagement.Business.DTOs;

// Maps patient entities to DTOs and back to entity values. Entities never leave the Business layer (AD-03).
public static class PatientMapper
{
    public static PatientDto ToDto(Patient patient, int age) => new(
        patient.Id,
        patient.PatientCode,
        patient.FullName,
        patient.DateOfBirth,
        age,
        patient.Gender,
        patient.Phone,
        patient.Email,
        patient.Address);

    public static PatientSummaryDto ToSummary(Patient patient) => new(
        patient.Id,
        patient.PatientCode,
        patient.FullName,
        patient.DateOfBirth,
        patient.Phone);

    // Copies the entered values onto an entity. The identity and the patient code are never changed here.
    // Callers have validated the input first, so the nullable values are present.
    public static void ApplyInput(PatientInput input, Patient patient)
    {
        patient.FullName = input.FullName!.Trim();
        patient.DateOfBirth = input.DateOfBirth!.Value;
        patient.Gender = input.Gender!;
        patient.Phone = input.Phone!.Trim();
        patient.Email = input.Email!.Trim();
        patient.Address = input.Address!.Trim();
    }

    public static Patient ToNewEntity(PatientInput input)
    {
        var patient = new Patient();
        ApplyInput(input, patient);
        return patient;
    }
}
