namespace PatientManagement.Data.Entities;

/// <summary>
/// The one printable prescription of a consultation (FR-RX-002, PDR-14). Created when the first medicine line is saved.
/// IsPrinted and PrintedAt are set by the Print command. Once printed, the Business layer blocks edits (AD-13, D-M5-4).
/// </summary>
public class Prescription
{
    public int Id { get; set; }

    public int ConsultationId { get; set; }

    public bool IsPrinted { get; set; }

    // Local clinic time, no UTC conversion (AD-07). Set once, by the Print command.
    public DateTime? PrintedAt { get; set; }

    public List<PrescriptionItem> Items { get; set; } = [];
}
