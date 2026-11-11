using PatientManagement.Data.Entities;

namespace PatientManagement.Business.DTOs;

// Maps visit entities to DTOs. Entities never leave the Business layer (AD-03).
public static class ConsultationMapper
{
    // The vitals are mapped only when the entity has them loaded and saved.
    public static ConsultationDto ToDto(Consultation consultation, Patient patient) => new(
        consultation.Id,
        consultation.PatientId,
        patient.FullName,
        patient.PatientCode,
        consultation.AppointmentId,
        consultation.VisitDateTime,
        consultation.Complaints,
        consultation.Diagnosis,
        consultation.FinishedAt,
        consultation.Vitals is null
            ? null
            : new VitalsDto(
                consultation.Vitals.TemperatureC,
                consultation.Vitals.SystolicMmHg,
                consultation.Vitals.DiastolicMmHg,
                consultation.Vitals.PulseBpm));

    public static VisitSummaryDto ToSummary(Consultation consultation) => new(
        consultation.Id,
        consultation.VisitDateTime,
        consultation.AppointmentId,
        consultation.FinishedAt is not null);
}
