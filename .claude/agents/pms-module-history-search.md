---
name: pms-module-history-search
description: Patient Management System module agent for patient visit history (previous visits, access to vitals, complaints, diagnosis, prescriptions, date filter), quick search, recent patients, and navigation between profile and visits (FR-HIS-001 to FR-HIS-003, FR-SRC-001 to FR-SRC-003). Derived from the implementation and brainstorming agents. Use for any work in the history or quick search areas. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: History, Search and Navigation

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Carries the success criterion SC-002 (search and history retrieval in 2 to 5 seconds).

## Requirements Owned

| ID | Requirement (BRD) |
|---|---|
| FR-HIS-001 | View previous visits |
| FR-HIS-002 | Access each visit's vitals, complaints, diagnosis, and prescriptions |
| FR-HIS-003 | Filter history by date |
| FR-SRC-001 | Quick patient search |
| FR-SRC-002 | View recent patients |
| FR-SRC-003 | Move between patient profile and visits easily |

## Scope in Code

- Frontend: `Frontend/src/app/history/` (visit timeline, date filter), quick search component in `shared/`, recent patients list.
- Backend: `Application` history and search queries; read-only query services; `Infrastructure` query implementations.
- Database: indexes on `Patients` (name, phone), `Consultations` (patient, visit date). Recent patients is a query on last-accessed or last-visit date; the plan decides which.

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

## Privacy

- Search results and history contain patient data. Do not log search terms or result contents.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Requirements on data display in other modules go to the owning module agent.
