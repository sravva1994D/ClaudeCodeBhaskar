# 14 — Module 7 Implementation Plan (Export: CSV and Printable View)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M7 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Work packages (`16-WBS`):** WP-8.1 (export service), WP-8.2 (CSV), WP-8.3 (printable view and PDF)
**Status:** Plan approved in chat on 2026-11-12. Decisions D-M7-1 to D-M7-8 are as recommended in §3. Implementation proceeds one step at a time on the Project Owner's go-ahead.
**Source rules:** BRD (FR-EXP-001, FR-EXP-002, SC-005); CLAUDE.md §2, §4, §7, §8, §10; PDR-17, PDR-18, DEC-020; `10-Service-Design.md` §2.7; `09-Controller-Design.md` routes 25–27

---

## 1. Scope

| In scope | Requirement IDs | Work packages |
|---|---|---|
| Export form SCR-13 for one patient, optional From and To, with an Export button on the profile SCR-06 | FR-EXP-001, FR-EXP-002 | WP-8.1 |
| CSV download: one file, one row per visit, patient details on each row, ISO dates, 16 columns | FR-EXP-001 | WP-8.2 |
| A4 printable view with a Print button; browser Save as PDF | FR-EXP-002 | WP-8.3 |
| Verification that both files open and print | SC-005 | WP-8.2, 8.3 |

Out of scope: search timing and browser matrix (M8). No BRD change. No schema change and no migration. No new library (PDR-18, CLAUDE §2).

## 2. Already decided (not re-opened)

- PDR-17 / DEC-020: one CSV file, one row per visit, patient details repeated on each row. ISO dates (yyyy-MM-dd). The 16 columns are fixed in `10-Service-Design.md` §2.7. UTF-8 with a byte-order mark, header row, quoting of values with commas or line breaks. A patient with no visits gives one row with blank visit columns.
- PDR-18: printable HTML view. The Print button calls `window.print()`. Save as PDF is done by the browser. No PDF library and no server-side PDF.
- Routes 25 to 27 in `09-Controller-Design.md`: `GET /Export/Patient/{id}` (form), `GET /Export/Patient/{id}/Csv?from&to`, `GET /Export/Patient/{id}/Print?from&to`.
- Services: `IExportService` (GetOptionsAsync, BuildCsvAsync, BuildPrintViewAsync) and `ICsvWriter`, both in Business.

## 3. Decisions (recommendations, approved with the plan)

| # | Question | Decision |
|---|---|---|
| D-M7-1 | Date range rule | Same rule as the visit filter (Module 6, D-M6-1 and D-M6-8). From alone is that single day. From and To are both inclusive. To without From is refused. From after To is refused. Both empty means all visits. One shared rule, so the filter and the export never disagree |
| D-M7-2 | Malformed dates | Refused with the Module 6 message. Never silently dropped, because a silent drop would export the whole history |
| D-M7-3 | CSV formula injection | A text cell that starts with `=`, `+`, `-`, `@`, tab or carriage return gets a leading `'`, so a spreadsheet does not run it. Numeric vitals are not text and are not changed |
| D-M7-4 | File name | `patient-PT-000123-visits.csv`: patient code only. No patient name and no date range in the name |
| D-M7-5 | Printable view layout | Reuse `_PrintLayout` and the clinic header from the prescription print. Patient block, then one section per visit, newest first: date and time, vitals, complaints, diagnosis, medicines. The Print button marks nothing as printed (PDR-14 applies to prescriptions only) |
| D-M7-6 | Empty result | A patient with no visits at all: the CSV has one row with the patient columns filled. A filtered range with no visits: refused on the form with "No visits in this date range.", so a blank file is never taken for a full export. The printable view shows the same message |
| D-M7-7 | Sort order | CSV oldest first (reads as a timeline). Print view newest first, like visit history. The form says so |
| D-M7-8 | Unfinished visits | Included, as in the visit history. An in-progress visit is exported with the data entered so far |

## 4. Steps

Each step builds with 0 warnings, passes its tests, and is committed and pushed to `development`. One step per go-ahead from the Project Owner.

| Step | Work | Tests |
|---|---|---|
| 1. Data | `ExportVisitRow` read model (visit, vitals and medicines; no patient fields, which the service adds from `IPatientRepository` so the repository stays thin). `IConsultationRepository.ListForExportAsync(patientId, from?, to?)` as an AsNoTracking projection, oldest first. Medicines come from prescription items ordered by `SortOrder`. The day filter is one shared helper (`VisitsInRange`) also used by `ListByPatientAsync`, so the `DateOnly.MaxValue` guard applies to both | Data integration: range inclusive; medicines in order; visit with no vitals and no prescription; patient with no visits |
| 2. Business | `ICsvWriter` and `CsvWriter` (quoting, byte-order mark, formula guard, CRLF rows). `IExportService` and `ExportService`: `GetOptionsAsync`, `BuildCsvAsync` (16 columns; medicine cell `Name; Dosage; Frequency; Duration; Instructions` with blank parts omitted, joined by ` | `), `BuildPrintViewAsync`. The range rule is shared with `ConsultationService`. Unknown patient gives not-found. DI registration | Unit: columns and order; ISO dates; blank vitals; commas, quotes and line breaks; formula guard; no-visit row; range refusals; medicine cell format. IDs TC-FR-EXP-001-1, -2, -3 |
| 3. CSV and form (WP-8.1, 8.2) | `ExportController` (`[Authorize]`, GET only, no state change, so no anti-forgery token). `Patient` shows the SCR-13 form with From and To inputs. `PatientCsv` returns the file as `text/csv; charset=utf-8`. Export button on the profile | Web: sign-in required; unknown patient 404; content type; file name; byte-order mark; range applied; refusals shown on the form |
| 4. Printable view (WP-8.3) | `PatientPrint` action and `Views/Export/Print.cshtml` on `_PrintLayout`: Print button, Back link, Save as PDF hint, clinic header, patient block, visit sections | Web: patient fields; only visits in range; medicines shown; empty-range message; nothing marked printed. IDs TC-FR-EXP-002-1, -2 |
| 5. Close-out | Full unit and integration run. Update `18-` (FR-EXP-001, FR-EXP-002, SC-005) and `12-`. Write `15-Module-7-Review-Package.md` with browser checks | Chrome: CSV opened in a spreadsheet (columns, ISO dates, special characters, guarded cell); printable view in A4 preview and Save as PDF; range and empty cases |

## 5. Critical files

- New: `src/PatientManagement.Business/Interfaces/IExportService.cs`, `ICsvWriter.cs`; `Services/ExportService.cs`, `CsvWriter.cs`; `DTOs/Export*.cs`; `src/PatientManagement.Web/Controllers/ExportController.cs`; `ViewModels/Export/*`; `Views/Export/Patient.cshtml`, `Print.cshtml`
- Changed: `src/PatientManagement.Data/Abstractions/IConsultationRepository.cs` and `Repositories/ConsultationRepository.cs`; `src/PatientManagement.Business/BusinessServiceCollectionExtensions.cs`; `Views/Patients/Details.cshtml` (Export button); `Services/ConsultationService.cs` (shared range rule only)
- Reuse: the day-bound logic of `ListByPatientAsync`; `Result<T>` and `ResultMessage`; `Views/Shared/_PrintLayout.cshtml`; the clinic header in `Views/Prescriptions/Print.cshtml`; `IClinicProfileRepository` and `ClinicProfileService`; `TestDatabaseFixture`, `TestSignIn`, `TestWebApplicationFactory`

## 6. Verification

- `dotnet build PatientManagement.sln --no-incremental`: 0 errors, 0 warnings, with the app stopped first.
- Unit and integration suites pass, with FR-EXP IDs in the test names.
- Chrome (browser checks in the Module 7 review package): seed visits with and without vitals and medicines, one with a comma, a quote and a line break in free text, and one text starting with `=`. Download the CSV and open it in a spreadsheet. Open the printable view, check A4 preview, and Save as PDF. Check range and empty-range behaviour.

## 7. Risks

| Risk | Handling |
|---|---|
| CSV formula injection from free text | D-M7-3 guard, with tests |
| Silent whole-history export on a bad date | D-M7-2: malformed dates are refused |
| Patient data in file names or logs | D-M7-4: patient code only; nothing logged |
| Range rule drifts from the visit filter | One shared rule (D-M7-1) |
| Long history on one printed page | Each visit is its own section; page breaks between sections |

## 8. Completion criteria

FR-EXP-001, FR-EXP-002 and SC-005 pass tests with their IDs in the names. Matrices `18-` and `12-` are updated in the same change. The Project Owner approves the Module 7 review package after the browser checks.
