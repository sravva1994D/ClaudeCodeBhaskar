namespace PatientManagement.Data.Entities;

/// <summary>
/// A 30-minute appointment for a registered patient (FR-APT-001 to FR-APT-003; PDR-06, PDR-07, PDR-09).
/// Status values and transitions are enforced in the Business layer; the database only checks the allowed list.
/// Archived rows are kept and hidden, never deleted (CLAUDE §6).
/// </summary>
public class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    // Local clinic time, no UTC conversion (CLAUDE §6). Must sit on the slot grid (PDR-06), checked in the Business layer.
    public DateTime StartTime { get; set; }

    public short DurationMinutes { get; set; } = 30;

    public string Status { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
}
