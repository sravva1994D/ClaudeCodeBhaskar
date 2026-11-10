# 09 — Controller Design

**Source:** `04-Navigation-Flow.md`, `05-Screen-Catalog.md`, `03-Layer-Responsibilities.md`
**Rule:** Controllers are thin. Each action calls one service method, maps the result, and returns a view or redirect. No EF Core and no business rules in controllers.
**Status:** Proposed for review

## 1. Controller List

| Controller | Screens | Services used | Req IDs |
|---|---|---|---|
| AccountController | SCR-01, SCR-02 | IAuthService | NFR-SEC-001 |
| HomeController | SCR-03 | IAppointmentService, IPatientService | FR-APT-002, FR-SRC-001, FR-SRC-002 |
| PatientsController | SCR-04, SCR-05, SCR-06 | IPatientService, IVisitService | FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-003 |
| VisitsController | SCR-07, SCR-08 | IVisitService, IConsultationService | FR-HIS-001 to FR-HIS-003 |
| AppointmentsController | SCR-09, SCR-10 | IAppointmentService, IPatientService | FR-APT-001 to FR-APT-003 |
| ConsultationsController | SCR-11 | IConsultationService | FR-VIT-001, FR-CMP-001, FR-DIA-001 |
| PrescriptionsController | SCR-11 (medicine section), SCR-12 | IPrescriptionService | FR-RX-001, FR-RX-002 |
| ExportController | SCR-13 | IExportService | FR-EXP-001, FR-EXP-002 |
| ClinicProfileController | SCR-14 | IClinicProfileService | FR-RX-002 (PDR-15) |

## 2. Route and Action Table

Routes use conventional MVC routing (`{controller}/{action}/{id?}`). Explicit routes are shown where the design needs them.

| # | Method | Route | Controller.Action | Service call | Screen | Req IDs |
|---|---|---|---|---|---|---|
| 1 | GET | `/` | Home.Index | Appointment.GetDailyAsync(today); Patient.GetRecentAsync(5) | SCR-03 | FR-APT-002, FR-SRC-002 |
| 2 | GET | `/Account/Login` | Account.Login | — | SCR-01 | NFR-SEC-001 |
| 3 | POST | `/Account/Login` | Account.Login | Auth.SignInAsync | SCR-01 | NFR-SEC-001 |
| 4 | POST | `/Account/Logout` | Account.Logout | — (cookie sign-out) | Any | NFR-SEC-001 |
| 5 | GET | `/Account/ForgotPassword` | Account.ForgotPassword | — | SCR-02 | NFR-SEC-001 |
| 6 | POST | `/Account/ForgotPassword` | Account.ForgotPassword | Auth.ResetPasswordAsync | SCR-02 | NFR-SEC-001 |
| 7 | GET | `/Patients/Search?q=` | Patients.Search | Patient.SearchAsync(q) | SCR-04 | FR-PAT-004, FR-SRC-001 |
| 8 | GET | `/Patients/Create` | Patients.Create | — | SCR-05 | FR-PAT-001 |
| 9 | POST | `/Patients/Create` | Patients.Create | Patient.RegisterAsync(dto, confirmWarnings) | SCR-05 | FR-PAT-001 |
| 10 | GET | `/Patients/Details/{id}` | Patients.Details | Patient.GetAsync(id) | SCR-06 | FR-PAT-003, FR-SRC-003 |
| 11 | GET | `/Patients/Edit/{id}` | Patients.Edit | Patient.GetAsync(id) | SCR-05 | FR-PAT-002 |
| 12 | POST | `/Patients/Edit/{id}` | Patients.Edit | Patient.UpdateAsync(dto) | SCR-05 | FR-PAT-002 |
| 13 | GET | `/Patients/{id}/Visits?from&to` | Visits.Index | Visit.ListAsync(patientId, from, to) | SCR-07 | FR-HIS-001, FR-HIS-003 |
| 14 | GET | `/Visits/Details/{consultationId}` | Visits.Details | Consultation.GetAsync(id) | SCR-08 | FR-HIS-002 |
| 15 | GET | `/Appointments/Create?patientId&date` | Appointments.Create | Patient.GetAsync; Appointment.GetSlotsAsync(date) | SCR-09 | FR-APT-001 |
| 16 | POST | `/Appointments/Create` | Appointments.Create | Appointment.ScheduleAsync(dto) | SCR-09 | FR-APT-001 |
| 17 | GET | `/Appointments/Daily?date` | Appointments.Daily | Appointment.GetDailyAsync(date) | SCR-10 | FR-APT-002 |
| 18 | POST | `/Appointments/UpdateStatus` | Appointments.UpdateStatus | Appointment.UpdateStatusAsync(id, status) | SCR-10 | FR-APT-003 |
| 19 | POST | `/Consultations/Start` | Consultations.Start | Consultation.StartAsync(patientId, appointmentId?) | SCR-06, SCR-10 | FR-HIS-001, FR-APT-003 |
| 20 | GET | `/Consultations/Edit/{id}` | Consultations.Edit | Consultation.GetWorkingViewAsync(id) | SCR-11 | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001 |
| 21 | POST | `/Consultations/Edit/{id}` | Consultations.Edit | Consultation.SaveAsync(dto, confirmWarnings) | SCR-11 | FR-VIT-001, FR-CMP-001, FR-DIA-001 |
| 22 | POST | `/Prescriptions/SaveItems/{consultationId}` | Prescriptions.SaveItems | Prescription.SaveItemsAsync(consultationId, items) | SCR-11 | FR-RX-001 |
| 23 | POST | `/Prescriptions/Print/{consultationId}` | Prescriptions.Print | Prescription.MarkPrintedAsync(consultationId) → print model | SCR-12 | FR-RX-002, SC-004 |
| 24 | GET | `/Prescriptions/Print/{consultationId}` | Prescriptions.Print | Prescription.GetPrintModelAsync(consultationId) (read-only) | SCR-12 | FR-RX-002 |
| 25 | GET | `/Export/Patient/{id}` | Export.Patient | Export.GetOptionsAsync(id) | SCR-13 | FR-EXP-001, FR-EXP-002 |
| 26 | GET | `/Export/Patient/{id}/Csv?from&to` | Export.PatientCsv | Export.BuildCsvAsync(id, from, to) | SCR-13 | FR-EXP-001 |
| 27 | GET | `/Export/Patient/{id}/Print?from&to` | Export.PatientPrint | Export.BuildPrintViewAsync(id, from, to) | SCR-13 | FR-EXP-002 |
| 28 | GET | `/ClinicProfile/Edit` | ClinicProfile.Edit | ClinicProfile.GetAsync() | SCR-14 | FR-RX-002 |
| 29 | POST | `/ClinicProfile/Edit` | ClinicProfile.Edit | ClinicProfile.UpdateAsync(dto) | SCR-14 | FR-RX-002 |
| 30 | POST | `/Appointments/Archive/{id}` | Appointments.Archive | Appointment.ArchiveAsync(id) | SCR-10 | FR-APT-003 (DEC-022) |
| 31 | POST | `/Consultations/Finish/{id}` | Consultations.Finish | Consultation.FinishAsync(id) | SCR-11 | FR-APT-003 (DEC-022) |

Notes on the table:
- Row 19 is a POST that creates a consultation and redirects to row 20.
- Rows 18 and 19 do not set Completed or Archived. Completed is set by row 31 (Finish, after In Consultation). Archived is set by row 30 and does not change Status (DEC-022).
- Rows 28 and 29 hold header text only. There is no logo upload; the print view uses a placeholder logo (DEC-021).
- Rows 23 and 24: the first POST marks the prescription printed and renders the print view. Later GETs render the same view without changing state. Repeated printing is allowed (PDR-14).
- Row 26 returns a CSV download. Row 27 returns the printable HTML view (PDR-18); the browser's Save as PDF produces the PDF. Rows 21 to 24 check `IsPrinted` in the service.
- Route parameters are integers. Patient codes (`PT-000123`) are shown in the UI but not used in routes.

## 3. Controller Rules

| Rule | Detail |
|---|---|
| Authorisation | Global `[Authorize]` filter. `[AllowAnonymous]` only on rows 2–6 |
| Anti-forgery | `[ValidateAntiForgeryToken]` on every POST |
| Model binding | Bind to ViewModels. Map ViewModel to DTO in the controller using a mapper, not by hand inside the action |
| Warnings flow | First POST without confirmation returns the same view with warnings. Second POST sets `confirmWarnings = true` (hidden field) |
| Not-found | Unknown id returns 404 |
| Printed state | Actions that edit a printed prescription return the view with an error message from the service |
| Errors | Service errors go to `ModelState`. Unexpected exceptions go to the generic error page |
| No direct data | Controllers never reference `DbContext`, repositories or entities |

## 4. Action Results

| Outcome | Result |
|---|---|
| Success on create or update | Redirect to the relevant detail (PRG pattern) |
| Validation error | Return view with ViewModel (HTTP 200) |
| Warning needing confirmation | Return view with warning list and confirm control |
| Not found | 404 |
| Printed prescription edit attempt | Return view with error (HTTP 200) |
| Export (CSV) | File with `Content-Disposition: attachment` and content type `text/csv` |
| Export (print view) | HTML page for printing; no server-side PDF (PDR-18) |
