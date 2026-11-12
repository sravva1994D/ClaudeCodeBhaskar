namespace PatientManagement.Web.Presentation;

// One colour per appointment status, used by every list that shows it. The text is always shown too,
// so colour is never the only signal (accessibility).
public static class StatusBadge
{
    public static string ClassFor(string status) => status switch
    {
        "Completed" => "text-bg-success",
        "In Consultation" => "text-bg-warning",
        "Checked-In" => "text-bg-info",
        "Scheduled" => "text-bg-primary",
        _ => "text-bg-secondary"
    };
}
