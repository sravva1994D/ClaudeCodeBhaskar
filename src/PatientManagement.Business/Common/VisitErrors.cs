namespace PatientManagement.Business.Common;

// Error codes shared by the visit-related services (consultation, prescription), so one service does not depend on another's constants.
public static class VisitErrors
{
    public const string NotFoundCode = "Consultation.NotFound";
    public const string NotFoundMessage = "The visit was not found.";
    public const string PrintedCode = "Prescription.Printed";
    public const string PrintedVisitMessage = "This visit has a printed prescription, so it can no longer be changed.";
}
