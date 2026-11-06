---
name: pms-module-patient-management
description: Patient Management System module agent for patient registration, profile view and edit, and patient search by name or phone (FR-PAT-001 to FR-PAT-005). Holds the module's acceptance criteria, proposed API surface, and data rules. Derived from the implementation and brainstorming agents. Use for any work in the patients feature. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Patient Management

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD, Patient Management) |
|---|---|
| FR-PAT-001 | Add patient details |
| FR-PAT-002 | Edit patient details |
| FR-PAT-003 | View patient details |
| FR-PAT-004 | Capture name, age or DOB, gender, contact details |
| FR-PAT-005 | Search patients by name or phone number |

Related: FR-SRC-001 (quick search) is owned by the history-search module, but FR-PAT-005 must be consistent with it.

## Acceptance Criteria

Criteria are derived from the BRD wording. Items marked *proposed* need owner confirmation.

| ID | Given | When | Then |
|---|---|---|---|
| FR-PAT-001-AC-01 | Valid required details | The patient is saved | A patient record is created with an ID and audit fields |
| FR-PAT-001-AC-02 | A required field is missing | The patient is saved | The save is rejected with a validation error; no record is created |
| FR-PAT-002-AC-01 | An existing patient | A field is changed and saved | The change is stored and the update timestamp changes |
| FR-PAT-002-AC-02 | An edit that fails validation | It is saved | The previous values remain unchanged |
| FR-PAT-003-AC-01 | An existing patient | Their details are opened | All captured details are shown |
| FR-PAT-003-AC-02 | An unknown patient ID | Details are requested | A not-found response is returned |
| FR-PAT-004-AC-01 | A date of birth is known | The patient is shown | Age is derived from the date of birth, not stored as the only value |
| FR-PAT-004-AC-02 | A patient is captured | Name, age or DOB, gender, and contact are entered | All are stored; gender comes from a fixed list |
| FR-PAT-004-AC-03 | Age or DOB is required (*proposed, owner to confirm*) | Neither is entered | The save is rejected |
| FR-PAT-005-AC-01 | Patients exist | A name fragment is searched in any letter case | Matching patients are returned |
| FR-PAT-005-AC-02 | Patients exist | A phone number is searched with spaces or dashes | Matches are found on the digits |
| FR-PAT-005-AC-03 | No patient matches | A search is run | An empty result is returned, not an error |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `POST /api/v1/patients` | Add patient | FR-PAT-001 |
| `GET /api/v1/patients/{id}` | View patient | FR-PAT-003 |
| `PUT /api/v1/patients/{id}` | Edit patient | FR-PAT-002 |
| `GET /api/v1/patients?name=&phone=&page=` | Search by name or phone | FR-PAT-005 |

Each operation returns the standard error model and the status codes in the planning agent's conventions.

## Data (planning input)

- `Patients`: `Id`, name, date of birth (nullable only if the owner confirms FR-PAT-004-AC-03 as optional), gender (fixed list), contact fields, `CreatedAtUtc`, `UpdatedAtUtc`.
- Indexes: name, phone digits.

## Scope in Code

- Frontend: `Frontend/src/app/patients/` (list, detail, form, search box).
- Backend: `Application` patient services and DTOs; `Domain` patient entity; `Infrastructure` patient repository.
- Database: `Patients` table as above.

## Rules

- Store DOB when known. Do not store age as the only value. Derive age on display.
- Gender values come from a fixed list in the Domain layer, not free text, unless the plan records a reason.
- Phone search matches digits only, so spaces and dashes in input do not break matching.
- Name search is case-insensitive and matches partial names.
- Patient records are not hard-deleted (CLAUDE.md §10).

## Test Focus

- Unit: validators for required fields, DOB and age consistency, phone normalisation.
- Integration: create, read, update, and search endpoints with `WebApplicationFactory`. Search returns within the NFR-PRF-002 target on a synthetic dataset.
- End-to-end: register a patient, then find them by name and by phone.
- Test names include criterion IDs, for example `FR_PAT_005_AC_01_search_by_partial_name_is_case_insensitive`.

## Validation

- Automated tests validate every criterion above. No manual validation is required for this module.

## Privacy

- Use synthetic names and phone numbers in every fixture and screenshot.
- Patient details never appear in logs or error messages.

## Handoffs

Output goes to `pms-verification` and then `pms-gap-analysis`. Any change to scheduling or consultation links goes back to `pms-planning`.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs are unchanged. The acceptance criteria, API surface, data notes, and validation section are added.
