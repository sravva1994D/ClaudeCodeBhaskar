namespace PatientManagement.Data.Entities;

/// <summary>
/// One medicine line of a prescription (FR-RX-001). Only Name is required (PDR-13); the other fields are free text.
/// Field lengths follow Documentation/02-Planning/06-Entity-Design.md §2.7.
/// </summary>
public class PrescriptionItem
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }

    // Display order on the prescription.
    public short SortOrder { get; set; }

    public Prescription? Prescription { get; set; }
}
