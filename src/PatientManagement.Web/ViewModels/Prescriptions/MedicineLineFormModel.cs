namespace PatientManagement.Web.ViewModels.Prescriptions;

// One medicine row on the consultation screen (FR-RX-001). Every field is free text; only Name is required (PDR-13).
public sealed class MedicineLineFormModel
{
    public string? Name { get; set; }

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }
}

// The inputs for one medicine row partial: its position in the list, its values, and whether the page is read-only.
public sealed record MedicineRowViewModel(int Index, MedicineLineFormModel Line, bool Locked);
