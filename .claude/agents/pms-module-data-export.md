---
name: pms-module-data-export
description: Patient Management System module agent for exporting patient or visit data as CSV and PDF (FR-EXP-001, FR-EXP-002). Holds the module's acceptance criteria, proposed API surface, and export safety rules. Derived from the implementation and brainstorming agents. Use for any export work. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Data Export

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Exports leave the system, so they carry the highest data-exposure risk after prescriptions. Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD, Data Export) |
|---|---|
| FR-EXP-001 | Export patient or visit data as CSV |
| FR-EXP-002 | Export patient or visit data as PDF |

Success criterion: SC-005 (successful export in CSV and PDF).

## Acceptance Criteria

| ID | Given | When | Then |
|---|---|---|---|
| FR-EXP-001-AC-01 | A patient or a visit | CSV export is requested | A UTF-8 CSV with a header row and one row per record is returned |
| FR-EXP-001-AC-02 | A value starting with `=`, `+`, `-`, or `@` | It is exported to CSV | The value is prefixed with a single quote (formula-injection guard) |
| FR-EXP-001-AC-03 | A value containing commas, quotes, or line breaks | It is exported to CSV | The value is escaped correctly |
| FR-EXP-002-AC-01 | A patient or a visit | PDF export is requested | A PDF with the same content as the screen view is returned |
| FR-EXP-002-AC-02 | A PDF export | It is generated | The header states the export date, the user, and the scope |
| FR-EXP-001-AC-04 | A request for an unbounded export | It is made | It is refused; the scope must be one patient, one visit, or a date range |
| FR-EXP-001-AC-05 | An unauthenticated request | An export is requested | It is refused |
| SC-005-AC-01 | A patient with several visits | Both CSV and PDF are exported | Both files open and the record count matches the synthetic data |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `GET /api/v1/patients/{id}/export?format=csv\|pdf&from=&to=` | Export a patient's visits in a date range | FR-EXP-001, FR-EXP-002 |
| `GET /api/v1/visits/{id}/export?format=csv\|pdf` | Export one visit | FR-EXP-001, FR-EXP-002 |

Each export operation logs an audit event (user, time, scope type, record count) and returns the file as a download.

## Scope in Code

- Frontend: export action on patient profile and visit pages (`shared/` export component).
- Backend: `Application` export service; `Infrastructure` CSV writer and PDF renderer (server-side, CLAUDE.md §2).
- No new database tables.

## Rules

- Export is server-side. The browser receives a file, not the raw data set.
- CSV: UTF-8 with a header row, one row per record, values escaped for commas, quotes, and line breaks. No formula injection: values starting with `=`, `+`, `-`, or `@` are prefixed with a single quote.
- PDF: same content as the screen view, with a header that states export date, user, and the scope of the export.
- Export scope is explicit: one patient, or one visit, or a date range. No unbounded export in Phase 1.
- Each export is logged as an event with the user, time, scope type, and record count. Do not log the record contents.
- Files are not stored on disk after download. If temporary files are needed, they are deleted in the same request.

## Test Focus

- Unit: CSV escaping and formula-injection prefixing; PDF model built from the same data as the screen.
- Integration: export endpoints return correct content type, file name, and only the requested scope; unauthenticated requests are refused.
- End-to-end: export a patient's visits as CSV and as PDF, and check the row or page count against the synthetic data.
- Test names include criterion IDs, for example `FR_EXP_001_AC_02_formula_prefix_applied`.

## Validation

- Automated tests validate every criterion above. No manual validation is required for this module.

## Privacy

- Exports contain patient data. Test exports use synthetic data only. Never commit exported files to the repository.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Security review of the export endpoints goes to `pms-code-review` as a Blocker-level check.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs keep their meaning. The acceptance criteria, API surface, audit note, and validation section are added.
