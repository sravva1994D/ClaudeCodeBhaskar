namespace PatientManagement.Data.Entities;

/// <summary>
/// A visit (FR-HIS-001). One consultation per appointment (PDR-26); a walk-in has no appointment (OQ-006).
/// VisitDateTime is set by the system at start and cannot be edited (PDR-24). Complaints and Diagnosis are free text (PDR-12).
/// FinishedAt is set by the Finish action and makes Finish one-time (D-M4-3). Visits are never deleted (CLAUDE §6).
/// </summary>
public class Consultation
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? AppointmentId { get; set; }

    // Local clinic time, no UTC conversion (AD-07).
    public DateTime VisitDateTime { get; set; }

    public string? Complaints { get; set; }

    public string? Diagnosis { get; set; }

    public DateTime? FinishedAt { get; set; }

    // Present when vitals were saved for this visit (1:0..1, FR-VIT-001).
    public VitalSign? Vitals { get; set; }
}
