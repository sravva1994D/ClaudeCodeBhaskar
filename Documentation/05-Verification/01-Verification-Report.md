# Verification Report: Phase 1 Implementation

**Date:** 2026-11-12
**Status:** For Project Owner review. Not approved.
**Verified code:** `development` at `38e73e4` (merge of PR #13, tree identical to `cfa612f` on `pms-Implementation`).
**Scope:** functional requirements, non-functional requirements, acceptance criteria (`../01-Brainstorming/10-Acceptance-Criteria.md`), requirement traceability (RTM), and the three layers (Presentation, Business, Data) against the architecture rules in CLAUDE.md §6 and `../02-Planning/01-Solution-Architecture.md`.

## 1. Summary

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 warnings, 0 errors |
| Unit tests | 385 passed, 0 failed, 0 skipped |
| Integration tests | 304 passed, 0 failed, 0 skipped |
| In-scope requirement IDs with automated tests | 23 of 26 (the other 3 are browser-verified: NFR-PRF-001, SC-004, SC-005) |
| In-scope IDs with a manual Chrome check (M1 to M15) | 26 of 26 (15 of 15 checks Pass, Chrome 157.0.8103.79) |
| Architecture rules (§6) | All checked rules pass. Some items were not checked in this pass and are marked in §6 |
| RTM coverage | 26 in-scope, 6 removed (DEC-025), 4 deferred. Consistent across the matrices after the corrections in §8 |
| Excluded checks | 4 items, excluded by owner decision (§7) |

Verdict: the implementation meets the in-scope requirements on the evidence available.

## 2. Functional requirements

Priorities from `../01-Brainstorming/12-Requirement-Traceability-Matrix.md`. Test counts are the number of test files that carry the ID, from the search of `tests/`. Manual steps are from `../04-Implementation/19-Module-9-Acceptance-Record.md` §3.

| Req ID | Priority | Automated evidence | Manual step | Verdict |
|---|---|---|---|---|
| FR-PAT-001 | Must | 6 files (PT code format, duplicate warning, required fields) | M2 | Verified |
| FR-PAT-002 | Must | 4 files (edit and save; no delete or archive path for patients, see §4) | M3 | Verified |
| FR-PAT-003 | Must | 4 files (profile shows every field and the age) | M3 | Verified |
| FR-PAT-004 | Must | 6 files (name and phone contains-match, case-insensitive, no-match message) | M3 | Verified |
| FR-APT-001 | Must | 7 files (grid, overlap refused, unregistered patient, past date) | M5 | Verified |
| FR-APT-002 | Must | 4 files (daily list for the day; archived hidden) | M7 | Verified |
| FR-APT-003 | Must | 10 files (allowed and disallowed moves; archive keeps status) | M7 | Verified |
| FR-APT-004 | Should | 2 files (reschedule to a free slot; old slot freed; taken slot refused; same-day rule) | M6 | Verified |
| FR-VIT-001 | Must | 6 files (vitals saved; out-of-range confirmation; missing vitals notice; boundaries) | M8 | Verified |
| FR-CMP-001 | Must | 2 files (complaints saved and shown) | M8, M10 | Verified |
| FR-DIA-001 | Must | 2 files (diagnosis saved and shown) | M8, M10 | Verified |
| FR-RX-001 | Must | 5 files (medicine name required; optional fields; 200-character limit) | M8 | Verified |
| FR-RX-002 | Must | 11 files (print view content; print marks printed; reprint keeps first time; anti-forgery on print) | M11, M14 | Verified. Save as PDF is browser-only |
| FR-HIS-001 | Must | 6 files (visits listed newest first; visit without vitals) | M8, M9 | Verified |
| FR-HIS-002 | Must | 5 files (vitals, complaints, diagnosis and medicines in visit detail) | M10 | Verified |
| FR-HIS-003 | Should | 3 files (single day; range inclusive; malformed date refused) | M9 | Verified |
| FR-SRC-001 | Must | 2 files (top-bar quick search) | M3 | Verified |
| FR-SRC-002 | Should | 4 files (five most recent by last visit; no-visit patients excluded) | M4 | Verified |
| FR-SRC-003 | Should | 2 files (profile to history and back) | M9 | Verified |
| FR-EXP-001 | Must | 3 files (one row per visit; ISO dates; range; patient with no visits) | M12 | Verified. Spreadsheet open is browser-only |
| FR-EXP-002 | Must | 2 files (printable view content; range; Print button and Save as PDF hint) | M13 | Verified. Save as PDF is browser-only |

## 3. Non-functional requirements

| Req ID | Priority | Evidence | Verdict |
|---|---|---|---|
| NFR-SEC-001 | Must | Redirect to sign-in for protected pages; cookie settings test (HttpOnly, SameSite Lax, 24-hour sliding, login path); generic error for wrong password and unknown user; password policy (8 to 16 characters with upper, lower, digit and symbol); one account created by console seed, password typed at the prompt; global authorization fallback. Manual M1 | Verified (session expiry by configuration, see §7) |
| NFR-SEC-002 | Must | Hash tests (stored value is a hash; verifies with the original password; does not verify with another); HTTPS redirection and HSTS in the non-development pipeline (`Program.cs`). Encryption at rest out of scope (DEC-017) | Verified in tests. Transport by configuration review |
| NFR-PRF-001 | Must | Browser timing, median of 5 runs, ten page types, all under 2 s (`../04-Implementation/16-Module-8-Review-Package.md`, Chrome 157.0.8103.52) | Verified by the M8 record. Not automated (PDR-29) |
| NFR-USB-001 | Won't (DEC-025) | Removed from Phase 1 scope | Not verified, out of scope |
| NFR-PRF-002 | Won't (DEC-025) | Removed from Phase 1 scope. A test exists (`SearchPerformanceTests.cs`) and passes, but it is not acceptance evidence | Not verified, out of scope |
| NFR-SCL-001 | Won't (DEC-025) | Removed from Phase 1 scope. Volume check skipped by owner decision | Not verified, out of scope |
| NFR-CMP-001 | Won't (DEC-025) | Removed from Phase 1 scope. Chrome only; Edge not checked (DEC-019) | Not verified, out of scope |
| NFR-REL-001, NFR-REL-002 | Deferred (OOS-011) | No design or test | Not in scope |

## 4. Acceptance criteria

`10-Acceptance-Criteria.md` lists AC-1 to AC-n per requirement. The M9 acceptance record maps each in-scope requirement to its tests and its manual step. This report re-checked the criteria that a code search can settle:

| AC | Check | Result |
|---|---|---|
| FR-PAT-002 AC-3: no patient is deleted or archived | No delete or archive path for patients exists in `src/` (search for remove, delete and archive on patients). The Patient entity has no archive flag | Verified |
| FR-APT-003 AC-3: only allowed moves accepted | Lifecycle rules in `AppointmentLifecycle` and `AppointmentValidator`; unit and integration tests cover each allowed and disallowed move | Verified |
| FR-APT-001 AC-3: 30-minute slots on the clinic grid | `SlotGrid`; database filtered unique index on StartTime for active, non-Cancelled, non-No-show rows | Verified |
| FR-RX-002 AC-1, AC-2: A4 printable view with header, patient, vitals, diagnosis, medicines and footer | Print view tests and manual M11 | Verified. A4 print preview and Save as PDF are browser-only |
| NFR-SEC-001 AC-3: password 8 to 16 characters with four character types | `PasswordPolicyValidator` (length and character classes); tests in account, persistence, authorization and seed tests | Verified |
| NFR-SEC-001 AC-4: 24-hour sliding session | Cookie settings test asserts 24 hours and sliding. No test runs an actual expiry | Verified by configuration |
| NFR-SEC-002 AC-1: TLS 1.2+ beyond localhost | HTTPS redirection and HSTS are configured. TLS version is not set explicitly in the app | Verified by configuration review |
| NFR-SEC-002 AC-2, AC-3: no encryption at rest; passwords hashed | DEC-017 scope; hash tests | Verified |
| SC-004, SC-005 | Print and export open without error | Manual M11, M12, M13 |

## 5. Requirement traceability (RTM)

| Check | Result |
|---|---|
| In-scope IDs | 26: 17 Must functional, 4 Should functional (FR-HIS-003, FR-SRC-002, FR-SRC-003, FR-APT-004), 3 Must NFR, 2 Must success criteria |
| Removed from Phase 1 (DEC-025) | 6: NFR-USB-001, NFR-PRF-002, NFR-SCL-001, NFR-CMP-001, SC-001, SC-002 |
| Deferred | 4: NFR-REL-001, NFR-REL-002, SC-003, SC-006 |
| `12-Requirement-Traceability-Matrix.md` | All 26 in-scope rows carry the M9 status. The Coverage Checks row reads 26 of 26 |
| `18-Planning-Traceability-Matrix.md` | Each in-scope row has screens, routes, services, repositories, tables, tests and milestone. Section 8 lists the M9 results for all 26 |
| `15-Development-Roadmap.md`, `16-Work-Breakdown-Structure.md` | M9 and WP-10.1, WP-10.2 are listed. These files have no status column, so nothing changed |
| Corrections made in this verification | Two stale FR-APT-004 status cells ("awaiting owner approval") and one stale status line in `17-UI-Modernization-Review-Package.md` ("not yet committed") were updated. See §8 |

## 6. Architecture verification by layer

### Presentation (`PatientManagement.Web`)

| Rule | Check | Result |
|---|---|---|
| Web references Data only for DI registration (CLAUDE §6) | `Program.cs` calls `AddDataLayer`; no controller or view uses EF Core or the Data namespaces | Pass |
| Controllers do not use EF Core | No DbContext outside `Program.cs`. Business-rule placement checked for the reschedule and booking day rule (moved to services in `9e7f0c3`); other controllers not re-reviewed in this pass | Pass (EF Core check); rule placement partly checked |
| Anti-forgery on every POST (§9) | 14 `[HttpPost]` actions; every one carries `[ValidateAntiForgeryToken]` | Pass |
| Authentication required before patient data (NFR-SEC-001) | Global fallback policy requires an authenticated user | Pass |
| Cookie: HttpOnly, SameSite Lax, 24-hour sliding (§9) | `SecurityServiceCollectionExtensions.cs`; asserted by a test | Pass |
| Dates: dd-MMM-yyyy on screen and in print (DEC-026); CSV ISO (DEC-020) | Date format present in 10 views; CSV uses yyyy-MM-dd in `ExportService` | Pass |
| Confirmation and double-submit guard | `site.js`; 15-second reset added in `cfa612f`. No automated test; not checked in Chrome (excluded, §7) | Pass by review |

### Business (`PatientManagement.Business`)

| Rule | Check | Result |
|---|---|---|
| Services hold the rules; validation returns errors and warnings (AD-11) | Validators per area (`PasswordPolicyValidator`, `AppointmentValidator`, `VitalsValidator`, and others) | Pass |
| Services return DTOs, not entities (AD, §6) | No public service method returns an entity type (search of `Services/`) | Pass |
| Business may reference Data (dependency direction Web → Business → Data) | `PatientManagement.Business.csproj` references Data; Data references nothing | Pass |
| Dates are local clinic time; no UTC conversion (§6) | No `UtcNow`, `ToUniversalTime` or UTC conversion in `src/` | Pass |
| Passwords hashed with the ASP.NET Core hasher (§8) | `AspNetPasswordHasher` (in Web, behind `IPasswordHasher` in Business) | Pass |
| No hard deletes; appointments archived (§6, PDR-09) | Archive sets a flag; no remove calls in Data | Pass |
| Single clinic, no tenant columns (§6) | No `ClinicId` in `src/` | Pass |

### Data (`PatientManagement.Data`)

| Rule | Check | Result |
|---|---|---|
| EF Core only; no Dapper (PDR-32) | No Dapper reference in `src` or `tests` | Pass |
| Repositories thin, one per aggregate; no generic repository (§6) | Seven repository files, one per aggregate, plus the unique-violation helper. No `IRepository<T>` | Pass |
| SaveChangesAsync is the unit of work | Not checked in this pass | Not checked |
| Schema changes only through migrations (§6) | Five migrations (`AddUsers`, `AddPatients`, `AddAppointments`, `AddConsultations`, `AddPrescriptions`) plus the model snapshot | Pass |
| Slot uniqueness enforced by the database (PDR-07) | Filtered unique index on `StartTime` with `IsArchived = 0`, `Status` not Cancelled or No-show. Covered by data tests on SQL Server | Pass |
| No check constraints on vital-sign ranges (§6) | Check constraints only on Gender, DurationMinutes, and ClinicProfile Id = 1. None on vitals | Pass |
| Integration tests use a real SQL Server database, not in-memory (§10) | No `UseInMemory` in `tests/` | Pass |
| Name guard on the test database (`_Test`) | Not checked in this pass. Recorded in the Module 1 plan | Not checked |

## 7. Excluded checks

The Project Owner excluded these four items from this verification on 2026-11-12. They are not verified in this report, and they are not open gaps.

| # | Item | Status in this report |
|---|---|---|
| G1 | Run of the 24-hour session expiry (`14-Testing-Strategy.md` §3, NFR-SEC-001). Expiry is verified by its configuration only | Excluded |
| G2 | Explicit TLS 1.2+ setting beyond localhost. HTTPS redirection and HSTS are configured | Excluded |
| G3 | Automated test and Chrome check of the 15-second busy-state reset (`cfa612f`) | Excluded |
| G4 | Browser-only checks beyond the M-pass: Edge coverage, and print and export checks outside the Chrome pass | Excluded |

Known limits L1 to L7 from the acceptance record remain accepted (`19-Module-9-Acceptance-Record.md` §5).

## 8. Changes made to documentation for this verification

| File | Change |
|---|---|
| `Documentation/01-Brainstorming/12-Requirement-Traceability-Matrix.md` | FR-APT-004 status: "Accepted (M9, 2026-11-12); implemented DEC-027" (was "awaiting owner approval") |
| `Documentation/02-Planning/18-Planning-Traceability-Matrix.md` | FR-APT-004 tests cell: "accepted in M9" (was "awaiting owner approval") |
| `Documentation/04-Implementation/17-UI-Modernization-Review-Package.md` | Status line: "Implemented and committed (cb4f917, 11f5fe4). Browser checks and the Module 9 acceptance passed (M15)." (was "not yet committed") |
| `Documentation/05-Verification/01-Verification-Report.md` | This report (new) |

No code changed in this verification.

## 9. Review requested

1. Confirm the verdict in §1.
2. Approve the documentation changes in §8.

After approval, these files go onto a branch and a PR to `development`, following CLAUDE.md §5.4. The release PR to `main` follows separately, with its own approval.
