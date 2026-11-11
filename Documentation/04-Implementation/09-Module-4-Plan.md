# 09 — Module 4 Implementation Plan (Consultation Workflow)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 4 (Consultation Workflow) and the visit parts of the patient profile
**Milestone:** M4 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Proposed. Waiting for Project Owner approval. No code until approved.
**Source rules:** BRD (amended 2026-11-10); CLAUDE.md §3, §4, §6, §7, §8, §9, §10; `Documentation/04-Implementation/07-Module-3-Plan.md` (pattern); `Documentation/02-Planning/19-Appointment-Status-Lifecycle.md` §4 and §6

---

## 1. Scope

| In scope | Requirement IDs | Work packages (`16-WBS`) |
|---|---|---|
| Consultation and VitalSign entities, configuration, migration `AddConsultations` | FR-VIT-001, FR-HIS-001 | WP-5.1 |
| Vitals validator: range warnings, missing-vitals notice, pair rule | FR-VIT-001 | WP-5.2 |
| Start consultation from an appointment (Checked-In) and walk-in (SCR-06, SCR-10) | FR-HIS-001, FR-APT-003 | WP-5.3 |
| Finish consultation (route 31): In Consultation → Completed for appointments | FR-APT-003 | WP-5.3 |
| Consultation screen SCR-11: vitals, complaints, diagnosis, Save, Finish | FR-VIT-001, FR-CMP-001, FR-DIA-001 | WP-5.4 |
| Visit history SCR-07 (all visits, newest first) and visit detail SCR-08 | FR-HIS-001, FR-HIS-002 | WP-5.5 |
| Profile: last visit shown; links to visit history and Start consultation; back link from visits | FR-SRC-003, FR-PAT-003 | WP-3.4 (completed), WP-5.5 |

Out of scope, each in its later milestone: medicine lines and prescription (M5), visit date filter (FR-HIS-003, M6), recent patients on the home page (FR-SRC-002, M6), export (M7), print (M5). No placeholder sections for them are shown.

## 2. Requirement mapping

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md` §4) |
|---|---|---|
| FR-VIT-001 | AC-1 record vitals; AC-2 missing → warning, saved; AC-3 add later; AC-4 units; AC-5 range warnings with confirm | TC-FR-VIT-001-1 to -5 (one per AC), boundary tests at 33.9, 34.0, 42.0, 42.1 °C; systolic 69, 70, 250, 251; diastolic 39, 40, 150, 151; pulse 29, 30, 220, 221 |
| FR-CMP-001 | AC-1 free text; AC-2 shown on consultation and history | TC-FR-CMP-001-1 (save and reload); TC-FR-CMP-001-2 (appears in visit detail) |
| FR-DIA-001 | AC-1 save notes; AC-2 shown on consultation and history | TC-FR-DIA-001-1; TC-FR-DIA-001-2 |
| FR-HIS-001 | AC-1 all previous visits listed | TC-FR-HIS-001-1 (visit count matches seeded visits) |
| FR-HIS-002 | AC-1 vitals, complaints, diagnosis viewable | TC-FR-HIS-002-1 |
| FR-APT-003 (M4 part) | Start sets In Consultation; Finish sets Completed | TC-FR-APT-003-3 (start sets In Consultation); TC-FR-APT-003-5 (Finish from In Consultation only) |
| FR-SRC-003 | AC-1 move from profile to visits and back | TC-FR-SRC-003-1 |

FR-HIS-003 (date filter) is not in this module. Its test ID stays open until M6.

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points applied

| Point | Design | Source |
|---|---|---|
| Consultation is the visit | One table for a visit. Can exist without an appointment (walk-in) | `06-` §2.4; OQ-006 |
| One consultation per appointment | Unique filtered index on AppointmentId (not null). A second start is rejected | PDR-26; `07-` §4 |
| Visit date | Set by the system at start, not editable | PDR-24; AD-07 |
| Start from appointment | Appointment must be Checked-In and belong to the patient. Sets In Consultation | `19-` §2, §4 |
| Finish | Only action that sets Completed. Walk-in: no status change | PDR-27; DEC-022; `10-` §2.4 |
| Vitals storage | VitalSign row only when vitals are saved; one row per consultation (1:0..1). No database range check | AD-14; `06-` §2.5 |
| Vitals warnings | Out of range shows a warning and needs "Confirm and save". Missing vitals show a notice and save without confirm | FR-VIT-001 AC-5; PDR-19; AD-11; `13-` §2 |
| Complaints and diagnosis | Free text, one block each per visit; no length rule beyond the column | PDR-12; `13-` §3.4 |
| Layers | Controller calls one service; the service holds the lifecycle and vitals rules; repository is thin; entities stay in Data; DTOs to Web | AD-02, AD-03; `09-` §3 |
| Clock | Injected `TimeProvider` for the visit time | CLAUDE §10 |
| Logging | No clinical content in logs | CLAUDE §8 |

## 4. Decisions needed from the Project Owner

Each has a recommendation. Approval of this plan accepts them unless you say otherwise.

| # | Question | Recommendation | Why it needs a decision |
|---|---|---|---|
| D-M4-1 | Start from an appointment | Allowed only when the appointment is Checked-In, belongs to the patient, is not archived, and has no consultation yet. Sets the appointment to In Consultation. Visit time is now | `19-` §4; PDR-26 |
| D-M4-2 | Walk-in start | Allowed from the patient profile with no appointment. No status change. The rules do not limit how many walk-ins a patient can have open | OQ-006; BRD is silent on several open walk-ins |
| D-M4-3 | What Finish does and whether it is one-time | Finish is on SCR-11 only. It sets `FinishedAt` (new column). A second Finish is rejected. For an appointment, the status must be In Consultation, and it becomes Completed. Finish does not lock editing; editing stops only when printed (M5, PDR-14) | Needs a column for "finished"; without it a walk-in could be finished twice |
| D-M4-4 | Vitals: partial entry and precision | All four vitals or none. Systolic and diastolic both or neither (error otherwise). Temperature takes at most one decimal place (error otherwise, because the column stores one decimal) | `13-` §3.4 pair rule; `06-` §2.5 decimal(4,1) |
| D-M4-5 | Adding vitals later | Allowed: the consultation can be edited until printed, so vitals can be added on a later save (AC-3) | FR-VIT-001 AC-3 |
| D-M4-6 | Complaints and diagnosis | Optional at save. No length rule beyond the database column | PDR-12; `13-` §3.4 |
| D-M4-7 | Visit history and detail | SCR-07 lists all visits, newest first, with no date filter (M6). SCR-08 shows vitals, complaints and diagnosis, and an Edit link. No medicines section until M5 | Keeps screens to what is built |
| D-M4-8 | Profile additions | "Last visit" date (latest visit, from consultations only, PDR-11). "Start consultation" button and "Visit history" link. Closes the M2 item "last visit not shown" | `05-` SCR-06; M2 review §6 item 6 |
| D-M4-9 | Start on the daily list | A "Start consultation" action on Checked-In rows of SCR-10 (WP-5.3 entry point) | `09-` row 19; WP-5.3 |

## 5. Work steps

Each step builds with 0 warnings, passes its tests, and is committed and pushed to `development`. Each step is reported in chat with its files and message. Work proceeds one step at a time on the Project Owner's go-ahead.

1. **Entities, configuration and migration.** `Consultation` (FK to Patients restricted; FK to Appointments restricted; unique filtered index on AppointmentId; index on PatientId and VisitDateTime descending; `FinishedAt` nullable) and `VitalSign` (ConsultationId primary key and foreign key; decimal(4,1) and smallint columns; no range check). Migration `AddConsultations`. Persistence tests: unique appointment link, one vitals row per consultation, no range check at the database, FK restrict.
2. **Repositories.** `IConsultationRepository` (GetById with vitals, ListByPatient newest first, GetByAppointmentId, Add, Update) and `IVitalSignRepository` (GetByConsultation, Upsert). A second consultation for the same appointment raises a typed exception, mapped to a message as in Module 3. Integration tests on `PatientManagement_Test`.
3. **Vitals validator and consultation validator.** Range warnings (TC-FR-VIT-001-1 to -5; boundary sets), missing-vitals notice, pair rule, precision rule (D-M4-4), free-text fields with no rule beyond the column. Unit tests for each boundary on both sides.
4. **Lifecycle and service.** `ConsultationService`: StartAsync (walk-in and from appointment, D-M4-1 and D-M4-2), GetWorkingViewAsync, SaveAsync (warnings with confirm, D-M4-5), FinishAsync (D-M4-3), GetDetailAsync, ListByPatient. Unit tests with substitute repositories and a fixed clock: TC-FR-APT-003-3 (start sets In Consultation), TC-FR-APT-003-5 (Finish from In Consultation only), TC-FR-HIS-001-1 (visit list), TC-FR-CMP-001-1, TC-FR-DIA-001-1.
5. **Consultation screen (SCR-11) and start entry points.** Start from the profile (SCR-06) and from Checked-In rows on SCR-10 (D-M4-9). The Finish button on SCR-11. Warning panel with "Confirm and save" (AD-11). Web integration tests: TC-FR-VIT-001-1 to -5 at the page level, Finish rules, start rules.
6. **Visit history (SCR-07), visit detail (SCR-08) and profile.** Lists, detail, last visit on the profile, back and forward links (D-M4-7, D-M4-8). Web tests: TC-FR-HIS-002-1, TC-FR-CMP-001-2, TC-FR-DIA-001-2, TC-FR-SRC-003-1.
7. **Close-out.** Full test run; update `18-Planning-Traceability-Matrix.md` for FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001, FR-HIS-002, FR-SRC-003 and the M4 part of FR-APT-003; write `09-Module-4-Review-Package.md` with the browser checks for the Project Owner.

## 6. Verification

- `dotnet build PatientManagement.sln --no-incremental`: 0 errors, 0 warnings.
- Unit and integration suites pass against `PatientManagement_Test`.
- Project Owner applies the migration to the development database (`dotnet ef database update`, app stopped) and checks in Chrome: start from a Checked-In appointment; start a walk-in; enter vitals with an out-of-range value and confirm; enter vitals only partly (error); save with no vitals (notice, no confirm); edit later to add vitals; Finish an appointment consultation (appointment becomes Completed, then can be archived); try Finish twice (rejected); visit history and detail; profile last visit and links.

## 7. Risks

| Risk | Handling |
|---|---|
| Two starts for the same appointment at the same moment | Unique filtered index is the backstop (step 1); translated to a message (step 2) |
| Start or Finish changes two tables (consultation and appointment) | One DbContext and one SaveChanges, so both change or neither does (AD-04 unit of work) |
| Precision of temperature input | Rule in step 3 (D-M4-4) |
| "Finished" needs a column | Migration in step 1 (D-M4-3) |
| Module 5 will add editing rules after print | Printed check is added in M5 and tested there; M4 leaves the hook in the service |

## 8. Completion criteria for Module 4

- FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001, FR-HIS-002, FR-SRC-003 and the M4 part of FR-APT-003 pass their tests, with IDs in the test names.
- Start and Finish follow the lifecycle in `19-`.
- Traceability matrices updated in the same change.
- Project Owner reviews `09-Module-4-Review-Package.md` and approves.
