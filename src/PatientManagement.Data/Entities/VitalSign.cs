namespace PatientManagement.Data.Entities;

/// <summary>
/// The vital signs of one consultation (FR-VIT-001). A row exists only when vitals were saved.
/// All four fields are required together. Range checks are warnings in the Business layer, never database checks (AD-14).
/// </summary>
public class VitalSign
{
    // Same key as the consultation it belongs to (1:0..1).
    public int ConsultationId { get; set; }

    // Degrees Celsius, one decimal place (decimal(4,1)).
    public decimal TemperatureC { get; set; }

    public short SystolicMmHg { get; set; }

    public short DiastolicMmHg { get; set; }

    public short PulseBpm { get; set; }

    public Consultation? Consultation { get; set; }
}
