# 14 — Testing Strategy

**Source:** `10-Acceptance-Criteria.md` (Module 1), BRD Success Criteria and NFRs
**Rule:** Every acceptance criterion maps to at least one test. Deferred items (SC-003, SC-006, NFR-REL-001, NFR-REL-002) have no test in Phase 1.
**Status:** Proposed for review. Test tools (PDR-29) and browser version recording (PDR-28) decided 2026-11-10.

## 1. Test Levels

| Level | Scope | Tool (proposed) | Where |
|---|---|---|---|
| Unit | Services, validators, mappers, age calculation, vitals ranges | xUnit with NSubstitute (PDR-29) | `PatientManagement.UnitTests` |
| Integration (data) | Repositories and EF Core mappings, constraints, indexes, migrations | xUnit against a real SQL Server test database | `PatientManagement.IntegrationTests` |
| Integration (web) | Controllers with real service and test database; authentication; anti-forgery | ASP.NET Core `WebApplicationFactory` | `PatientManagement.IntegrationTests` |
| UI and acceptance | End-to-end scenarios per acceptance criterion | Manual script, no browser automation (PDR-29) | Test scripts in `Documentation/` (later module) |
| Non-functional | Page load, search time, consultation time, browser matrix, encryption and login checks | Browser developer tools; stopwatch script; manual matrix | Test report (later module) |

No in-memory database provider is used for data tests. SQL-specific behaviour (unique indexes, filtered indexes, check constraints, collation) must be tested on SQL Server.

## 2. Test Data

| Set | Content | Use |
|---|---|---|
| Seed-minimal | 1 user, blank clinic profile, 3 patients, 1 appointment per day for 5 days | Functional tests |
| Seed-performance | Up to 1,000 patients, visits and appointments spread across the year | NFR-PRF-001, NFR-PRF-002, SC-002 |
| Boundary | Vitals at 33.9, 34.0, 42.0, 42.1 °C; systolic 69, 70, 250, 251; diastolic 39, 40, 150, 151; pulse 29, 30, 220, 221 | FR-VIT-001 warnings |

Test data is generated and marked as test data. It is never clinical data (BRD: no real patient data is held in Phase 1, see OOS-015 note).

## 3. Non-Functional Test Plan

| Req ID | Test | Method | Pass rule (BRD) | Status |
|---|---|---|---|---|
| NFR-PRF-001 | Standard page load | Browser timing on 10 page types, median of 5 runs, seed-performance data | About 2 s on localhost (guideline, not SLA) | Planned |
| NFR-PRF-002 | Patient search | Timed search on seed-performance data, 10 terms | 2–5 s on localhost | Planned |
| SC-002 | Search and history retrieval | Same as NFR-PRF-002 plus visit history load | 2–5 s | Planned |
| SC-001 | Consultation record | Timed rehearsal: open consultation, enter vitals, complaints, diagnosis, 2 medicines, save | 2–3 min, usability goal, localhost, test data | Planned |
| SC-004 | Prescription print | Print preview and A4 print to PDF; check all header and body elements | Generates and prints without error | Planned |
| SC-005 | Export | Export CSV and PDF for one patient with and without date range; open both | Completes and opens without error | Planned |
| NFR-CMP-001 | Browser matrix | Latest Chrome, Edge and Safari (desktop) on each release candidate; run the acceptance script | Works in all three (browser version recorded per run, PDR-28) | Planned |
| NFR-SEC-001 | Login and access | Access protected URL unauthenticated; wrong password; 24-hour expiry test by setting clock or cookie expiry | Redirect to login; generic error; expiry honoured | Planned |
| NFR-SEC-002 | Encryption | Verify hashed password in DB (not plain text); verify TLS when run beyond localhost; verify storage encryption setting on DB (PDR-21) | Per PDR-21 decision | Planned |
| NFR-USB-001 | Usability | No measurable criterion is defined in BRD. Qualitative check only: tab order, required markers, one-screen consultation (CON-016) | No pass/fail threshold (see SC-006 deferred) | Qualitative only |
| NFR-SCL-001 | Volume | Seed-performance data loaded and all list screens used | Works with up to 1,000 patients | Planned |

Deferred, no test in Phase 1: NFR-REL-001 and NFR-REL-002 (OOS-011), SC-003 and SC-006 (OOS-014).

## 4. Acceptance Test Map (functional)

Each row is one test scenario. The test ID format is `TC-<Req ID>-<n>`.

| Req ID | Acceptance criterion (from Module 1) | Test scenarios |
|---|---|---|
| FR-PAT-001 | AC-1 save with all fields; AC-2 retrievable | TC-FR-PAT-001-1: save with all six required fields; TC-FR-PAT-001-2: missing each required field gives an error; TC-FR-PAT-001-3: patient code format PT-000123; TC-FR-PAT-001-4: duplicate warning and continue; TC-FR-PAT-001-5: age calculated from DOB |
| FR-PAT-002 | AC-1 edit and save; AC-2 visible on profile | TC-FR-PAT-002-1: edit each field and see it on profile |
| FR-PAT-003 | AC-1 profile shows all captured details | TC-FR-PAT-003-1: compare profile to saved data |
| FR-PAT-004 | AC-1 name search; AC-2 phone search | TC-FR-PAT-004-1: search by name part; TC-FR-PAT-004-2: search by phone part; TC-FR-PAT-004-3: no match message |
| FR-APT-001 | AC-1 schedule; AC-2 appears in schedule | TC-FR-APT-001-1: schedule for registered patient; TC-FR-APT-001-2: overlap rejected; TC-FR-APT-001-3: unregistered patient rejected |
| FR-APT-002 | AC-1 daily list for chosen day | TC-FR-APT-002-1: list shows only that day; TC-FR-APT-002-2: archived rows hidden |
| FR-APT-003 | AC-1 lifecycle statuses; AC-2 shown in list; AC-3 allowed moves only; AC-4 manual archive | TC-FR-APT-003-1: each allowed move; TC-FR-APT-003-2: each disallowed move, including reversal and Scheduled or Checked-In → Completed; TC-FR-APT-003-3: consultation start sets In Consultation; TC-FR-APT-003-4: archive hides appointment |
| FR-VIT-001 | AC-1 record vitals; AC-2 missing → warning, saved; AC-3 add later; AC-4 units; AC-5 range warnings with confirm | TC-FR-VIT-001-1 to -5 mapped to each AC; boundary tests in §2 |
| FR-CMP-001 | AC-1 free text; AC-2 shown on consultation and history | TC-FR-CMP-001-1: save and reload; TC-FR-CMP-001-2: appears in visit detail |
| FR-DIA-001 | AC-1 save notes; AC-2 shown on consultation and history | TC-FR-DIA-001-1; TC-FR-DIA-001-2 |
| FR-RX-001 | AC-1 five fields; AC-2 more than one medicine | TC-FR-RX-001-1: add one medicine with all fields; TC-FR-RX-001-2: add two medicines |
| FR-RX-002 | AC-1 generate and print; AC-2 all elements | TC-FR-RX-002-1: print view contains header, patient, vitals, diagnosis, medicines, footer; TC-FR-RX-002-2: edit after print blocked; TC-FR-RX-002-3: second prescription for the same consultation not created |
| FR-HIS-001 | AC-1 all previous visits listed | TC-FR-HIS-001-1: visit count matches seeded visits |
| FR-HIS-002 | AC-1 vitals, complaints, diagnosis, prescriptions viewable | TC-FR-HIS-002-1 |
| FR-HIS-003 | AC-1 filter by date; AC-2 only that range | TC-FR-HIS-003-1: single date; TC-FR-HIS-003-2: range boundaries inclusive |
| FR-SRC-001 | AC-1 quick search returns matches | TC-FR-SRC-001-1 (uses FR-PAT-004 rules) |
| FR-SRC-002 | AC-1 recently accessed patients shown | TC-FR-SRC-002-1: five patients, latest visit first; TC-FR-SRC-002-2: patients with no visit excluded (PDR-11) |
| FR-SRC-003 | AC-1 move between profile and visits and back | TC-FR-SRC-003-1 |
| FR-EXP-001 | AC-1 export CSV; AC-2 opens with expected columns | TC-FR-EXP-001-1: open in spreadsheet tool; TC-FR-EXP-001-2: date range applied |
| FR-EXP-002 | AC-1 export PDF; AC-2 contains data | TC-FR-EXP-002-1: open the printable view and Save as PDF; TC-FR-EXP-002-2: date range applied |
| NFR-SEC-001 | AC-1 no access without login; AC-2 one user | TC-NFR-SEC-001-1; TC-NFR-SEC-001-2 |

Acceptance criteria that are blocked in Module 1 (`10-Acceptance-Criteria.md`) are now unblocked by the answered open questions. All PDR items are decided (2026-11-10), so the test scenarios are final.

## 5. Entry and Exit Criteria

| Gate | Entry | Exit |
|---|---|---|
| Unit | Feature code written | All unit tests pass; business rules covered |
| Integration | Migration applied to test DB | Repository and constraint tests pass |
| Work package (on `development`) | Acceptance tests for the work package's requirement IDs written | All pass before the commit is pushed; no open defects of severity high |
| Phase 1 release candidate | All features merged to development | All functional tests pass; NFR tests run and recorded; browser matrix run |

## 6. Defect Handling (local)

- Defects are recorded in a local log with: requirement ID, test ID, steps, expected, actual.
- Severity: High (blocks a requirement), Medium (workaround exists), Low (cosmetic).
- High defects block the feature gate.

## 7. Traceability

Each test ID is linked to its requirement ID in `18-Planning-Traceability-Matrix.md`. Coverage is checked against `11-Requirement-Catalog.md` (35 IDs: 31 in scope, 4 deferred).
