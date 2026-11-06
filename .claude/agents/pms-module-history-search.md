---
name: pms-module-history-search
description: Patient Management System module agent for patient visit history (previous visits, access to vitals, complaints, diagnosis, prescriptions, date filter), quick search, recent patients, and navigation between profile and visits (FR-HIS-001 to FR-HIS-003, FR-SRC-001 to FR-SRC-003). Holds the module's acceptance criteria, including the SC-002 timing criterion, proposed API surface, and data rules. Derived from the implementation and brainstorming agents. Use for any work in the history or quick search areas. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: History, Search and Navigation

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Carries the success criterion SC-002 (search and history retrieval in 2 to 5 seconds). Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD) |
|---|---|
| FR-HIS-001 | View previous visits |
| FR-HIS-002 | Access each visit's vitals, complaints, diagnosis, and prescriptions |
| FR-HIS-003 | Filter history by date |
| FR-SRC-001 | Quick patient search |
| FR-SRC-002 | View recent patients |
| FR-SRC-003 | Move between patient profile and visits easily |

Success criterion: SC-002 (search and history in 2 to 5 seconds).

## Acceptance Criteria

| ID | Given | When | Then |
|---|---|---|---|
| FR-HIS-001-AC-01 | A patient with several visits | Their history is opened | Previous visits are listed, newest first |
| FR-HIS-001-AC-02 | A patient with no visits | Their history is opened | An empty history is shown, not an error |
| FR-HIS-002-AC-01 | A visit | It is opened from history | Its vitals, complaints, diagnosis, and prescriptions are shown |
| FR-HIS-002-AC-02 | A visit in history | Its content is viewed | No edit controls are shown (history is read-only) |
| FR-HIS-003-AC-01 | Visits across several dates | A date range is applied | Only visits within the range are shown, with both ends included |
| FR-SRC-001-AC-01 | Patients exist | A name or phone is typed | Results appear after a short debounce and the first set is within the NFR-PRF-002 target |
| FR-SRC-002-AC-01 | Patients were opened recently | The recent list is shown | The last N distinct patients are listed; N is set in configuration |
| FR-SRC-003-AC-01 | A search with a filter applied, then a visit opened | The patient profile is reached and the user returns | The search and filter state are preserved |
| SC-002-AC-01 | A synthetic dataset of realistic size | Search and history are timed | Each completes within 2 to 5 seconds; the values are recorded |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `GET /api/v1/patients/{id}/visits?from=&to=` | Visit history with date filter, newest first | FR-HIS-001, FR-HIS-003 |
| `GET /api/v1/visits/{id}` | Visit detail (vitals, complaints, diagnosis, prescriptions), read-only | FR-HIS-002 |
| `GET /api/v1/patients/search?q=` | Quick search | FR-SRC-001 |
| `GET /api/v1/patients/recent?count=` | Recent patients | FR-SRC-002 |

## Scope in Code

- Frontend: `Frontend/src/app/history/` (visit timeline, date filter), quick search component in `shared/`, recent patients list.
- Backend: `Application` history and search queries; read-only query services; `Infrastructure` query implementations.
- Database: indexes on `Patients` (name, phone), `Consultations` (patient, visit date). Recent patients is a query on last-opened or last-visit date; the plan decides which.

## Rules

- History is read-only. No edits from history views.
- Visits are shown newest first. The date filter includes both ends of the range.
- Quick search returns results as the user types, after a short debounce, and shows the first result set within the NFR-PRF-002 target.
- Recent patients shows the last N distinct patients the physician opened; N is set in configuration.
- Navigation: from a visit to the patient profile and back without losing the search or filter state.

## Test Focus

- Unit: date-range filter boundaries, recent-patients ordering and de-duplication.
- Integration: history query with and without date filters; search query on the indexed columns with a synthetic dataset sized for the NFR target.
- Performance: measure search and history retrieval against SC-002 and record the numbers in the verification report.
- End-to-end: search a patient by phone, open history, filter by date, open a visit's prescription.
- Test names include criterion IDs, for example `FR_HIS_003_AC_01_date_filter_includes_both_ends`.

## Validation

- Automated tests and timed measurements validate every criterion above. No manual validation is required for this module.

## Privacy

- Search results and history contain patient data. Do not log search terms or result contents.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Requirements on data display in other modules go to the owning module agent.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs keep their meaning. The acceptance criteria (including SC-002), API surface, data notes, and validation section are added.
