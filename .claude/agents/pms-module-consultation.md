---
name: pms-module-consultation
description: Patient Management System module agent for the consultation workflow: mandatory vitals (temperature, BP, pulse), complaints, and diagnosis notes (FR-VIT-001, FR-CMP-001, FR-DIA-001). Holds the module's acceptance criteria, including the SC-001 timing criterion, proposed API surface, and data rules. Derived from the implementation and brainstorming agents. Use for any work in the consultations feature. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Consultation

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. This module carries the success criterion SC-001 (a consultation record in 2 to 3 minutes), so UI speed is a requirement here. Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD, Consultation Workflow) |
|---|---|
| FR-VIT-001 | Record temperature, blood pressure, and pulse for every consultation (mandatory) |
| FR-CMP-001 | Enter patient complaints as free text |
| FR-DIA-001 | Record diagnosis notes |

Success criterion: SC-001 (consultation record in 2 to 3 minutes).

## Acceptance Criteria

| ID | Given | When | Then |
|---|---|---|---|
| FR-VIT-001-AC-01 | A consultation with no vitals | Save is attempted | Save is blocked on the UI and rejected on the server |
| FR-VIT-001-AC-02 | Temperature, systolic and diastolic BP, and pulse are missing one at a time | Save is attempted | Each missing value blocks the save and is named in the error |
| FR-VIT-001-AC-03 | All four vitals are entered | Save is attempted | The consultation is saved with the vitals |
| FR-VIT-001-AC-04 | A vital outside the normal range (*range table to be confirmed by the owner*) | It is entered | A confirmation prompt is shown; the value is not blocked |
| FR-CMP-001-AC-01 | A consultation | Complaints are entered as free text and saved | The text is stored with the consultation and shown in its history |
| FR-DIA-001-AC-01 | A consultation | Diagnosis notes are entered and saved | The notes are stored with the consultation and shown on the visit |
| SC-001-AC-01 | A complete consultation entered with the keyboard | The timed run is measured end to end | Elapsed time is within 2 to 3 minutes; the measurement is recorded |
| SC-001-AC-02 | The consultation form | Only the keyboard is used | Every required field and the save action are reachable and operable |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `POST /api/v1/appointments/{id}/consultation` | Create a consultation with vitals, complaints, and diagnosis in one request | FR-VIT-001, FR-CMP-001, FR-DIA-001, NFR-REL-001 |
| `PUT /api/v1/consultations/{id}` | Update a saved consultation | FR-CMP-001, FR-DIA-001 |
| `GET /api/v1/consultations/{id}` | View a consultation | FR-HIS-002 |

## Data (planning input)

- `Consultations`: `Id`, `PatientId`, `AppointmentId`, visit date (UTC), complaints text, diagnosis text, audit fields.
- Vitals: columns on `Consultations` or `ConsultationVitals`, decided in the plan. Temperature, systolic, diastolic, and pulse are all required on save.

## Scope in Code

- Frontend: `Frontend/src/app/consultations/` (consultation page with vitals, complaints, diagnosis sections; keyboard-first entry).
- Backend: `Application` consultation service; `Domain` consultation and vitals value objects; `Infrastructure` repository.
- Database: `Consultations` and vitals storage as above, with audit columns.

## Rules

- Vitals are mandatory. Save is blocked until temperature, systolic and diastolic BP, and pulse are entered. Enforce on the server as well as the UI.
- Validate ranges for vitals to catch entry errors. Show a confirmation prompt for values outside a normal range rather than blocking. The range table is recorded in the plan and reviewed by the physician.
- Complaints and diagnosis are free text. Do not auto-suggest diagnoses (AI diagnosis is out of scope).
- Each consultation is saved as a visit that stays in the patient history (FR-HIS).
- The save is one request that stores the whole consultation or nothing (NFR-REL-001).
- Tab order and keyboard shortcuts for vitals, complaint, and diagnosis are tested for SC-001.

## Test Focus

- Unit: vitals validation (required, numeric, range warnings), save guard.
- Integration: create consultation endpoint rejects missing vitals with `ProblemDetails`; a failed save leaves no partial record.
- End-to-end: full consultation timed against SC-001, recorded in the verification report.
- Accessibility: keyboard-only completion of the consultation form.
- Test names include criterion IDs, for example `FR_VIT_001_AC_02_missing_bp_blocks_save`.

## Validation

- SC-001-AC-01 needs the owner's confirmation that the timed run reflects real use. Record the sign-off in the verification report.
- Automated tests validate the other criteria.

## Privacy

- Complaints and diagnosis are clinical free text. Never copy them into logs, analytics events, or test output.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Prescription links go to `pms-module-prescriptions`. History display goes to `pms-module-history-search`.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs keep their meaning. The acceptance criteria (including SC-001), API surface, data notes, and validation section are added.
