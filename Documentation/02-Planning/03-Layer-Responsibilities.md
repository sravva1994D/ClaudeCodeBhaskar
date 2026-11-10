# 03 — Layer Responsibilities

**Source:** `01-Solution-Architecture.md` (AD-01 to AD-04)
**Status:** Proposed for review

## 1. Responsibilities by Layer

| Layer | Owns | Must do | Must not do |
|---|---|---|---|
| **Presentation** `PatientManagement.Web` | Controllers, Razor Views, ViewModels, route definitions, auth configuration, input binding, anti-forgery, layout and CSS | Bind input to ViewModels; call one service per action; map results to view or redirect; show errors and warnings | Contain business rules (age calculation, overlap, immutability, vitals ranges). Reference EF Core types. Query the database |
| **Business** `PatientManagement.Business` | Service interfaces and implementations, DTOs, validators, mappers, business rules, transaction boundaries, result types | Enforce every BRD rule that is not a database rule. Return `Result` objects with errors and warnings. Map entities to DTOs | Know about HTTP, views or cookies. Return entities. Format HTML or PDF output |
| **Data** `PatientManagement.Data` | EF Core `DbContext`, entities, EF configurations, migrations, repositories, seed data | Persist and query. Enforce structural constraints (not null, unique, lengths) | Enforce warn-and-allow rules. Know about DTOs or ViewModels |

## 2. Responsibilities by Component

| Component | Layer | Responsibility | Req IDs |
|---|---|---|---|
| AccountController | Presentation | Login, logout, forgot-password forms | NFR-SEC-001 |
| HomeController | Presentation | Landing page composition | FR-APT-002, FR-SRC-001, FR-SRC-002 |
| PatientsController | Presentation | Search, register, edit, profile | FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-003 |
| VisitsController | Presentation | Visit history list and visit detail | FR-HIS-001 to FR-HIS-003 |
| AppointmentsController | Presentation | Schedule, daily list, status change | FR-APT-001 to FR-APT-003 |
| ConsultationsController | Presentation | Start a consultation; vitals, complaints, diagnosis | FR-VIT-001, FR-CMP-001, FR-DIA-001 |
| PrescriptionsController | Presentation | Medicine lines; print view | FR-RX-001, FR-RX-002 |
| ExportController | Presentation | CSV and PDF download | FR-EXP-001, FR-EXP-002 |
| ClinicProfileController | Presentation | Header data for prescription (PDR-15) | FR-RX-002 |
| IAuthService / AuthService | Business | Credential check, forgot-password reset | NFR-SEC-001 |
| IPatientService | Business | Register, edit, view, search, recent patients, duplicate warning | FR-PAT-001 to FR-PAT-004, FR-SRC-001, FR-SRC-002 |
| IAppointmentService | Business | Schedule (slot and overlap rules), daily list, status change, archive | FR-APT-001 to FR-APT-003 |
| IConsultationService | Business | Start consultation (with or without appointment), save vitals with warnings, save complaints and diagnosis, visit history and filter | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001 to FR-HIS-003 |
| IPrescriptionService | Business | Add or edit medicine lines until printed; mark printed; build print model | FR-RX-001, FR-RX-002 |
| IExportService | Business | Build CSV and PDF content for one patient and optional date range | FR-EXP-001, FR-EXP-002 |
| IClinicProfileService | Business | Read and update header data | FR-RX-002 |
| Validators | Business | Errors and warnings per DTO | FR-PAT-001, FR-APT-001, FR-VIT-001, FR-RX-001 |
| Repositories | Data | Query and persist each aggregate | All data-storing requirements |
| DbContext and migrations | Data | Schema and relationships | All data-storing requirements |

## 3. What Belongs Where: Quick Rules

| Question | Answer |
|---|---|
| Is the age calculated? | Business (from DOB and system date). Not stored. |
| Is the patient code generated? | Business (after insert, from identity). Stored in Data. |
| Is the vitals range check a rule? | Business validator returns a **warning**. Data has no range check. |
| Is "one prescription per consultation" enforced? | Data (unique index on `ConsultationId`) and Business (returns existing prescription). |
| Is "cannot edit after printing" enforced? | Business (blocks update when `IsPrinted`). Data stores the flag. |
| Is the 24-hour idle timeout enforced? | Presentation configuration (cookie `ExpireTimeSpan`, sliding). |
| Is the print layout built? | Presentation (Razor print view, A4 CSS, window.print()). Data from Business. PDR-16 and PDR-18. |
