# 18 — Planning Traceability Matrix

**Purpose:** Extends `Documentation/01-Brainstorming/12-Requirement-Traceability-Matrix.md` into planning. Each requirement ID is traced to its screens, controller actions, services, repositories, tables, tests, work packages and milestone.
**Source of IDs:** `01-Brainstorming/11-Requirement-Catalog.md` (35 IDs: 31 in scope, 4 deferred)
**Priority:** MoSCoW, proposed in `BRD/Doc_BRD.md` → Requirement Priorities (approved by Project Owner 2026-11-10, A-006 / DEC-006)
**Status:** Proposed for review

Route numbers (#) refer to `09-Controller-Design.md` §2. Test IDs follow `14-Testing-Strategy.md` §4.

## 1. Functional Requirements

| Req ID | Priority | Screens | Controller.Action (route #) | Service.Method | Repository | Tables | Tests | WBS | Milestone | PDR |
|---|---|---|---|---|---|---|---|---|---|---|
| FR-PAT-001 | Must | SCR-05 | Patients.Create (8, 9) | PatientService.RegisterAsync | IPatientRepository.AddAsync, FindPossibleDuplicatesAsync | Patients | TC-FR-PAT-001-1 to -5 | WP-2.2, 3.1, 3.2, 3.3 | M2 | 01, 10, 22 |
| FR-PAT-002 | Must | SCR-05 | Patients.Edit (11, 12) | PatientService.UpdateAsync | IPatientRepository.GetByIdAsync | Patients | TC-FR-PAT-002-1 | WP-3.3 | M2 | 25 |
| FR-PAT-003 | Must | SCR-06 | Patients.Details (10) | PatientService.GetAsync | IPatientRepository.GetByIdAsync | Patients | TC-FR-PAT-003-1 | WP-3.4 | M2 | — |
| FR-PAT-004 | Must | SCR-04 | Patients.Search (7) | PatientService.SearchAsync | IPatientRepository.SearchAsync | Patients | TC-FR-PAT-004-1 to -3 | WP-3.5 | M2 | 23 |
| FR-APT-001 | Must | SCR-09 | Appointments.Create (15, 16) | AppointmentService.ScheduleAsync, GetSlotsAsync | IAppointmentRepository.HasOverlapAsync, AddAsync | Appointments | TC-FR-APT-001-1 to -3 (implemented, M3; test names in `04-Implementation/08-Module-3-Review-Package.md` §2) | WP-4.1, 4.2 | M3 | 06, 07, 26 |
| FR-APT-002 | Must | SCR-03, SCR-10 | Home.Index (1); Appointments.Daily (17) | AppointmentService.GetDailyAsync | IAppointmentRepository.GetDailyAsync | Appointments | TC-FR-APT-002-1, -2 (implemented, M3) | WP-4.3, 4.5 | M3 | 09 |
| FR-APT-003 | Must | SCR-10 | Appointments.UpdateStatus (18); Consultations.Start (19); Consultations.Finish (31); Appointments.Archive (30); lifecycle in `19-` | AppointmentService.UpdateStatusAsync, ArchiveAsync; ConsultationService.FinishAsync | IAppointmentRepository.GetByIdAsync | Appointments | TC-FR-APT-003-1, -2, -4 (implemented, M3); -3, -5 (implemented, M4) | WP-4.4, 5.3 | M3 | 07, 08, 09, 27 |
| FR-VIT-001 | Must | SCR-11 | Consultations.Edit (20, 21) | ConsultationService.SaveAsync; vitals validator | IVitalSignRepository.UpsertAsync | VitalSigns | TC-FR-VIT-001-1 to -5, boundary set (implemented, M4) | WP-5.1, 5.2, 5.4 | M4 | 19 |
| FR-CMP-001 | Must | SCR-11, SCR-08 | Consultations.Edit (20, 21) | ConsultationService.SaveAsync | IConsultationRepository.GetByIdAsync | Consultations | TC-FR-CMP-001-1, -2 (implemented, M4) | WP-5.4 | M4 | 12 |
| FR-DIA-001 | Must | SCR-11, SCR-08 | Consultations.Edit (20, 21) | ConsultationService.SaveAsync | IConsultationRepository.GetByIdAsync | Consultations | TC-FR-DIA-001-1, -2 (implemented, M4) | WP-5.4 | M4 | — |
| FR-RX-001 | Must | SCR-11 (medicine section) | Prescriptions.SaveItems (22) | PrescriptionService.SaveItemsAsync | IPrescriptionRepository.ReplaceItemsAsync | Prescriptions, PrescriptionItems | TC-FR-RX-001-1, -2 (implemented and approved, M5; see review package §2) | WP-6.1, 6.2 | M5 | 13 |
| FR-RX-002 | Must | SCR-12 | Prescriptions.Print (23, 24) | PrescriptionService.MarkPrintedAsync, GetPrintModelAsync | IPrescriptionRepository.GetByConsultationAsync; IClinicProfileRepository.GetAsync | Prescriptions, PrescriptionItems, ClinicProfile, VitalSigns | TC-FR-RX-002-1 to -3 (implemented and approved, M5; see review package §2) | WP-6.1, 6.3, 6.4 | M5 | 14, 15, 16 |
| FR-HIS-001 | Must | SCR-07 | Visits.Index (13) | ConsultationService.ListVisitsAsync | IConsultationRepository.ListByPatientAsync | Consultations | TC-FR-HIS-001-1 (implemented, M4) | WP-5.1, 5.3, 5.5 | M4 | 11, 24, 26 |
| FR-HIS-002 | Must | SCR-08 | Visits.Details (14) | ConsultationService.GetAsync | IConsultationRepository.GetByIdAsync (with vitals) | Consultations, VitalSigns | TC-FR-HIS-002-1 (implemented, M4); TC-FR-HIS-002-2 medicines and print link (implemented and approved, M5) | WP-5.5, 6.4 | M4, M5 | — |
| FR-HIS-003 | Should | SCR-07 | Visits.Index (13, from and to) | ConsultationService.ListVisitsAsync | IConsultationRepository.ListByPatientAsync | Consultations | TC-FR-HIS-003-1, -2 (implemented and approved, M6; browser Chrome 157.0.8103.51, see `13-` §4) | WP-7.1 | M6 | — |
| FR-SRC-001 | Must | Top bar; SCR-03; SCR-04 | Patients.Search (7) | PatientService.SearchAsync | IPatientRepository.SearchAsync | Patients | TC-FR-SRC-001-1 | WP-3.5, 4.5 | M2, M3 | 23 |
| FR-SRC-002 | Should | SCR-03 | Home.Index (1) | PatientService.GetRecentAsync | IConsultationRepository.GetRecentPatientVisitsAsync; IPatientRepository.GetByIdsAsync | Consultations, Patients | TC-FR-SRC-002-1, -2 (implemented and approved, M6; browser Chrome 157.0.8103.51, see `13-` §4) | WP-7.2 | M6 | 11 |
| FR-SRC-003 | Should | SCR-06, SCR-07, SCR-08 | Patients.Details (10); Visits.Index (13) | PatientService.GetAsync; ConsultationService.ListVisitsAsync | IPatientRepository, IConsultationRepository | Patients, Consultations | TC-FR-SRC-003-1 (implemented, M4) | WP-3.4, 5.5 | M2, M4 | — |
| FR-EXP-001 | Must | SCR-13 | Export.PatientCsv (26) | ExportService.BuildCsvAsync | IConsultationRepository.ListForExportAsync | Patients, Consultations, VitalSigns, Prescriptions, PrescriptionItems | TC-FR-EXP-001-1, -2, -3 (implemented, M7; browser checks pending, review package `15-`) | WP-8.1, 8.2 | M7 | 17 |
| FR-EXP-002 | Must | SCR-13 | Export.PatientPrint (27) | ExportService.BuildPrintViewAsync | IConsultationRepository.ListForExportAsync | Patients, Consultations, VitalSigns, Prescriptions, PrescriptionItems | TC-FR-EXP-002-1, -2 (implemented, M7; browser checks pending, review package `15-`) | WP-8.1, 8.3 | M7 | 17, 18 |

## 2. Non-Functional Requirements

| Req ID | Priority | Screens / scope | Controller.Action (route #) | Service / component | Repository / config | Tables / store | Tests | WBS | Milestone | PDR |
|---|---|---|---|---|---|---|---|---|---|---|
| NFR-USB-001 | Must | All data-entry screens | Layout (all controllers) | Layout, CSS, tab order | — | — | Qualitative (TC-NFR-USB-001) | WP-2.5, 9.6 | M1, M8 | — |
| NFR-PRF-001 | Must | All pages | — | — | — | — | Timed page load, 10 pages | WP-9.1 | M8 | 30 |
| NFR-PRF-002 | Must | SCR-04, SCR-07 | Patients.Search (7); Visits.Index (13) | PatientService.SearchAsync; VisitService.ListAsync | Index strategy (07 §4) | Patients, Consultations | Timed search, 10 terms | WP-3.6, 9.1 | M2, M8 | 23, 30 |
| NFR-SEC-001 | Must | SCR-01, SCR-02 | Account.* (2 to 6); global Authorize | AuthService.SignInAsync, ResetPasswordAsync | IUserRepository | Users | TC-NFR-SEC-001-1, -2 | WP-2.3, 2.4, 9.4 | M1, M8 | 02, 03, 04, 05, 20 |
| NFR-SEC-002 | Must | All (transport) | — | Password hash; HTTPS config | None at rest (DEC-017) | Users (hash) | Hash check; transport configuration review | WP-9.4 | M1, M8 | 21, DEC-017 |
| NFR-SCL-001 | Must | All list screens | — | — | — | Patients (1,000 records) | Volume check | WP-9.5 | M8 | — |
| NFR-CMP-001 | Must | All screens | — | — | — | — | Browser matrix (Chrome, Edge; DEC-019) | WP-2.5, 9.3 | M1, M8 | 28, DEC-019 |

## 3. Success Criteria

| Ref | Priority | Screens | Controller.Action | Service / component | Tables | Tests | WBS | Milestone | PDR |
|---|---|---|---|---|---|---|---|---|---|
| SC-001 | Must (verification) | SCR-11 | Consultations.Edit (20, 21) | ConsultationService.SaveAsync | Consultations, VitalSigns, Prescriptions | Timed rehearsal | WP-9.2 | M8 | — |
| SC-002 | Must (verification) | SCR-04, SCR-07 | Patients.Search (7); Visits.Index (13) | PatientService, VisitService | Patients, Consultations | Timed search and history | WP-3.6, 9.1 | M2, M8 | 30 |
| SC-004 | Must (verification) | SCR-12 | Prescriptions.Print (23, 24) | PrescriptionService.MarkPrintedAsync | Prescriptions | Browser checks 7 to 11 passed (Chrome 157.0.8103.51, A4 print and Save as PDF), review package §4; approved M5 | WP-6.4 | M5 | 16 |
| SC-005 | Must (verification) | SCR-13 | Export.PatientCsv (26); Export.PatientPrint (27) | ExportService | — | Automated content and refusal checks done (M7); file open and Save as PDF checks pending (review package `15-` §4, checks 3 to 9, 13 to 15) | WP-8.2, 8.3 | M7 | 17, 18 |

## 4. Deferred (not designed in Phase 1)

| Req ID | Priority | Reason | Planning treatment |
|---|---|---|---|
| NFR-REL-001 | Won't Have (Phase 1) | OOS-011 | No design, test or WBS item |
| NFR-REL-002 | Won't Have (Phase 1) | OOS-011 | No design, test or WBS item |
| SC-003 | Won't Have (Phase 1) | OOS-014 | No measurement |
| SC-006 | Won't Have (Phase 1) | OOS-014 | Qualitative check only under NFR-USB-001 |

## 5. Coverage Checks

| Check | Result |
|---|---|
| Every in-scope requirement ID (31) has at least one screen or component | Yes |
| Every in-scope requirement ID has at least one test. Functional IDs have TC IDs; NFR and SC rows name the test type, with TC IDs to be assigned in the test plan | Yes |
| Every in-scope requirement ID has at least one WBS work package | Yes |
| Every in-scope requirement ID has a milestone | Yes |
| Every screen (SCR-01 to SCR-14) traces to at least one requirement ID | Yes (see `05-Screen-Catalog.md`) |
| Every route (1 to 29) traces to at least one requirement ID | Yes (see `09-Controller-Design.md`) |
| Every table traces to at least one requirement ID | Yes (see `08-ERD.md` §3) |
| Design element without a requirement ID (added functionality) | None identified. Design elements that exist only to support a requirement are listed in §6 |
| Deferred IDs have no design work | Yes (§4) |

## 6. Supporting Design Elements (no separate requirement ID)

These are needed to implement a requirement. Each is traced to the requirement it supports.

| Element | Supports | Why it exists |
|---|---|---|
| Home page composition (SCR-03) | FR-APT-002, FR-SRC-001, FR-SRC-002 | Landing page that brings the three together |
| Top-bar quick search | FR-SRC-001 | Quick search needs a persistent entry point |
| Clinic profile (SCR-14, ClinicProfile) | FR-RX-002 | The BRD requires header fields but not where they are entered (PDR-15) |
| Users table | NFR-SEC-001 | Stores the single login account |
| Anti-forgery tokens | NFR-SEC-001 | Protects form submissions on the single account |
| Seed command (console) | NFR-SEC-001, CON-002 | Creates the single account; password typed at a prompt and never stored (DEC-018) |
| Placeholder logo | FR-RX-002 | Logo image in the print view; no upload (DEC-021) |
| Seed data (test set) | NFR-PRF-001, NFR-PRF-002, NFR-SCL-001 | Needed to measure performance at 1,000 records |

## 7. Changes Compared with Module 1 RTM

| Item | Module 1 (`12-RTM`) | Planning (this file) | Reason |
|---|---|---|---|
| Blocked status | Most rows "Blocked" | Not used. Open items are listed as PDR in `17-` | Open questions are answered; all planning decisions are closed (2026-11-10) |
| FR-SRC-001 | Must | Must | Unchanged |
| Downstream columns | "Added in later modules" | Added here | Module 2 scope |
