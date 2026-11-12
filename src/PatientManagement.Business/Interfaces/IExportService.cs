using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Export rules for one patient (Documentation/02-Planning/10-Service-Design.md §2.7; FR-EXP-001, FR-EXP-002; PDR-17, PDR-18).
// The date range is the visit range rule (FR-HIS-003). A range with no visits is refused, so an empty file is never
// mistaken for a full export (D-M7-6).
public interface IExportService
{
    // The patient's details for the export form (SCR-13).
    Task<Result<ExportOptionsDto>> GetOptionsAsync(int patientId, CancellationToken cancellationToken = default);

    // One CSV file: one row per visit, patient details on each row, oldest first (PDR-17, DEC-020, D-M7-7).
    Task<Result<ExportFile>> BuildCsvAsync(
        int patientId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);

    // The printable view of the same content (FR-EXP-002, PDR-18). The browser's Save as PDF makes the PDF.
    Task<Result<ExportPrintDto>> BuildPrintViewAsync(
        int patientId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
}
