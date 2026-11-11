using PatientManagement.Data.Entities;

namespace PatientManagement.Business.DTOs;

// Maps prescription entities to DTOs and back. Entities never leave the Business layer (AD-03).
public static class PrescriptionMapper
{
    public static PrescriptionDto ToDto(Prescription prescription) => new(
        prescription.Id,
        prescription.ConsultationId,
        prescription.IsPrinted,
        prescription.PrintedAt,
        prescription.Items.OrderBy(item => item.SortOrder).Select(ToLine).ToList());

    // The print view needs the visit with its vitals loaded (ConsultationMapper.ToDto reads them).
    public static PrescriptionPrintDto ToPrint(Consultation visit, Patient patient, Prescription prescription, ClinicProfileDto clinic) => new(
        visit.Id,
        prescription.Id,
        prescription.IsPrinted,
        prescription.PrintedAt,
        clinic,
        patient.FullName,
        patient.PatientCode,
        patient.DateOfBirth,
        patient.Gender,
        visit.VisitDateTime,
        ConsultationMapper.ToDto(visit, patient).Vitals,
        visit.Diagnosis,
        prescription.Items.OrderBy(item => item.SortOrder).Select(ToLine).ToList());

    // The sort order is the line's place in the cleaned list, starting at 1.
    public static PrescriptionItem ToItem(MedicineLineInput line, int sortOrder) => new()
    {
        Name = line.Name ?? string.Empty,
        Dosage = line.Dosage,
        Frequency = line.Frequency,
        Duration = line.Duration,
        Instructions = line.Instructions,
        SortOrder = (short)sortOrder
    };

    private static MedicineLineDto ToLine(PrescriptionItem item) => new(
        item.Name,
        item.Dosage,
        item.Frequency,
        item.Duration,
        item.Instructions);
}
