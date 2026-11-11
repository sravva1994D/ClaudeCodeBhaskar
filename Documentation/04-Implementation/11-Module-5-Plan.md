# 11 — Module 5 Implementation Plan (Medication and Prescription)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 5 (Medication and Prescription) and the clinic header data
**Milestone:** M5 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Proposed, 2026-11-11. Waiting for the Project Owner's approval. No code is written until the owner says "go ahead with module 5". Each step then proceeds on the owner's go-ahead.
**Source rules:** BRD; CLAUDE.md §3, §4, §6, §7, §8, §9, §10; `Documentation/04-Implementation/09-Module-4-Plan.md` (pattern); PDR-13, PDR-14, PDR-15, PDR-16, PDR-18; DEC-021; `Documentation/02-Planning/06-Entity-Design.md` §2.6 to §2.8; `13-Validation-Strategy.md` §3.5

---

## 1. Scope

| In scope | Requirement IDs | Work packages (`16-WBS`) |
|---|---|---|
| Prescription, PrescriptionItem and ClinicProfile entities, configuration, migration `AddPrescriptions` | FR-RX-001, FR-RX-002 | WP-6.1 |
| Medicine lines on SCR-11: add, edit, remove. Only Name required | FR-RX-001 | WP-6.2 |
| Clinic profile screen SCR-14 (clinic name, address, phone, doctor name, registration number) | FR-RX-002 | WP-6.3 |
| Print view SCR-12: A4 CSS, placeholder logo, Print button (`window.print()`), mark printed, reprint | FR-RX-002, SC-004 | WP-6.4 |
| Printed lock: visit edits and medicine edits blocked after the first print | FR-RX-002 AC-4 | WP-6.4 |
| Visit detail SCR-08 shows medicines and a Print prescription link | FR-HIS-002 | WP-5.5 (extended) |

Out of scope, each in its later milestone: visit date filter (FR-HIS-003, M6), recent patients (FR-SRC-002, M6), export (FR-EXP-001/002, M7), search timing and browser matrix (M8).

## 2. Requirement mapping

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md`) |
|---|---|---|
| FR-RX-001 | AC-1 medicine with Name, Dosage, Frequency, Duration, Instructions; only Name required. AC-2 more than one medicine. AC-3 at least one medicine before print | TC-FR-RX-001-1 (add and list lines), TC-FR-RX-001-2 (Name required; blank lines ignored) |
| FR-RX-002 | AC-1 printable A4 HTML view with Print button. AC-2 header, patient, vitals, diagnosis, medicines, footer. AC-3 one prescription per consultation. AC-4 Print marks printed; edits blocked; reprint allowed | TC-FR-RX-002-1 (view content), TC-FR-RX-002-2 (print marks printed; reprint allowed), TC-FR-RX-002-3 (edits blocked after print; no print without a medicine) |
| SC-004 | Prescriptions generate and print without error (A4 view) | Browser check (Chrome) including browser Save as PDF |
| FR-HIS-002 | AC-1 prescriptions can be viewed from a visit | TC-FR-HIS-002-2 (medicines shown on visit detail) |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points

| Point | Design | Source |
|---|---|---|
| One prescription per consultation | Unique index on Prescriptions.ConsultationId; service returns the existing one | PDR-14; `06-` §2.6; AD-04 |
| Medicine lines | PrescriptionItem rows; saved as a set (replace) in one transaction | `06-` §2.6; FR-RX-001 AC-2 |
| Required fields | Name only. A prescription needs at least one medicine before print | PDR-13 |
| Printed lock | Business layer checks IsPrinted on every save of the visit or medicines. The flag is set once, with a conditional update (same pattern as Finish) | AD-13; PDR-14; D1 |
| Print command | POST marks printed once and returns the print model. Reprint is a GET that is read-only | PDR-14; `10-` §2.3 |
| Clinic header | Single ClinicProfile row, created blank on first read | PDR-15; `06-` §2.8 |
| Logo | Static placeholder image in `wwwroot`. No upload | DEC-021; PDR-15 |
| Print method | Printable HTML with A4 CSS and `window.print()`. No PDF library, no server PDF | PDR-16, PDR-18 |
| Layers | Controllers call services; services hold rules; repositories thin; entities stay in Data; DTOs to Web | AD-02, AD-03 |
| Clock | Injected `TimeProvider` for PrintedAt | CLAUDE §10 |
| Logging | No clinical content in logs | CLAUDE §8 |

## 4. Decisions needed from the Project Owner

Each has a recommendation. Approval of this plan accepts them unless you say otherwise.

| # | Question | Recommendation | Why it needs a decision |
|---|---|---|---|
| D-M5-1 | Where medicines are entered | A "Medicines" section on SCR-11, saved by its own "Save medicines" button. Separate from the visit Save. Lines are replaced as a set in one transaction | Two save buttons on one screen; the alternative is one combined save |
| D-M5-2 | Blank and partial lines | A line with every field empty is ignored. A line with any field filled but no Name is an error | PDR-13 requires Name only; this defines the empty line |
| D-M5-3 | Line limit and field lengths | No BRD limit. UI offers up to 20 lines per prescription. Field lengths from the entity design: Name 200, Dosage 100, Frequency 100, Duration 100, Instructions 500 | The BRD sets no limit; the 20 is a UI guard only |
| D-M5-4 | What the printed lock blocks | After the first Print: medicines, complaints, diagnosis and vitals cannot be saved (D1). Finish stays allowed (it changes no printed content). This also answers open decision 3 from Module 4: edits after Finish are allowed until the print | FR-RX-002 AC-4 freezes what the prescription shows |
| D-M5-5 | Print flow | The print view (GET) is read-only. The Print button posts to Print (POST), which marks printed once and returns the same view; the browser print dialog then opens. Reprint is the same view again | PDR-14 (print command sets printed; reprint allowed) |
| D-M5-6 | Clinic header not filled in | Print view shows a notice "Clinic details are incomplete" with a link to SCR-14. Printing is still allowed | `13-` §3.5 notice; PDR-15 leaves the rule open |
| D-M5-7 | Placeholder logo | Static image in `wwwroot`, no upload. The image file is a neutral placeholder, not a real clinic logo | DEC-021 |
| D-M5-8 | Open review finding 1 (Save is two writes) | Fix it in this module: the visit text and vitals are saved in one transaction, using the same pattern as Finish | Module 4 review §6 item 1; the printed lock depends on a single save path |

## 5. Work steps

Each step builds with 0 warnings, passes its tests, and is committed and pushed to `development` (Conventional Commits, `Requirements:` line, Co-Authored-By). Each commit is reported in chat with its files and message. Work proceeds one step at a time on the Project Owner's go-ahead.

1. **Entities, configuration and migration.** `Prescription` (unique ConsultationId; IsPrinted bool default false; PrintedAt datetime2 nullable), `PrescriptionItem` (PrescriptionId FK restrict; Name required; SortOrder smallint), `ClinicProfile` (single row, header fields). Migration `AddPrescriptions`. Persistence tests: unique ConsultationId, FK restrict, Name length, no range rules at the database.
2. **Repositories.** `IPrescriptionRepository` (GetByConsultation with items, Add, ReplaceItems in a transaction, MarkPrinted as a conditional update that changes only when IsPrinted is false). `IClinicProfileRepository` (Get, Save). Integration tests on `PatientManagement_Test`, including a second print that changes nothing.
3. **Validators and services.** `MedicineLineValidator` (D-M5-2, D-M5-3). `PrescriptionService`: SaveItemsAsync, MarkPrintedAsync (blocked with no medicine line, PDR-13), GetPrintModelAsync. `ClinicProfileService`: GetAsync (blank on first read), UpdateAsync. `ConsultationService.SaveAsync` and `FinishAsync` check the printed state (D-M5-4). Unit tests: TC-FR-RX-001-1, TC-FR-RX-001-2, TC-FR-RX-002-1 to -3; boundary lengths.
4. **Medicines section on SCR-11 and PrescriptionsController.** Add and remove rows (server-rendered rows; a small script adds rows, and the form still works without it). "Save medicines" button. Web integration tests: save several lines; empty-name line refused; medicines blocked after print.
5. **Clinic profile SCR-14.** Form, validation through the service, link from the navigation. Web tests for save and the incomplete-profile notice.
6. **Print view SCR-12 and visit detail SCR-08.** A4 print CSS; header with placeholder logo; patient details; vitals; diagnosis; medicines; footer signature. Print button marks printed. Read-only state on SCR-11 and SCR-08 once printed. Web tests: TC-FR-RX-002-1 to -3, TC-FR-HIS-002-2, and "no print without a medicine".
7. **Close-out.** Full test run. Update `18-Planning-Traceability-Matrix.md` for FR-RX-001, FR-RX-002, SC-004 and FR-HIS-002. Write `11-Module-5-Review-Package.md` with the browser checks for the Project Owner (Chrome, with the browser version recorded per PDR-28, and Save as PDF for SC-004).

## 6. Verification

- `dotnet build PatientManagement.sln --no-incremental`: 0 errors, 0 warnings.
- Unit and integration suites pass against `PatientManagement_Test`.
- Project Owner applies the migration (`dotnet ef database update`, app stopped) and checks in Chrome: add two medicines; a line with no Name is refused; fill in the clinic profile; the print view on A4 preview; Print marks the prescription printed; reprint works; visit edits and medicine edits are blocked after printing; Print with no medicine is refused; Save as PDF produces a PDF.

## 7. Risks

| Risk | Handling |
|---|---|
| Two prints at the same moment | Conditional update: one print changes the row; the other changes nothing (step 2) |
| Printed lock bypassed by a direct POST | The check lives in the service and is tested at service and page level (steps 3, 4, 6) |
| Print layout differs between browsers | Chrome is checked in this module; the browser matrix is in M8 |
| Medicine rows added by script without script support | Form works with server-rendered rows (step 4) |

## 8. Completion criteria for Module 5

- FR-RX-001, FR-RX-002, SC-004 and FR-HIS-002 (prescription part) pass their tests, with IDs in the test names.
- Printed lock works at service and page level.
- Traceability matrices updated in the same change.
- Project Owner reviews `11-Module-5-Review-Package.md` and approves.
