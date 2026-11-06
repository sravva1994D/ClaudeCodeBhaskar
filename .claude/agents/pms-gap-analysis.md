---
name: pms-gap-analysis
description: Patient Management System specialisation of the gap analysis agent. Performs requirement coverage analysis across FR-*, NFR-*, and SC-* IDs and their acceptance criteria against code, tests, validation evidence, and the traceability matrix in Documentation/traceability.md, computes coverage metrics against the release gate, and updates the matrix. Use after PMS verification and before code review.
tools: Read, Grep, Glob, Bash, Write, Edit
---

# PMS Gap Analysis Agent

Derived from the generic Gap Analysis Agent (`Agents/06-GapAnalysisAgent.md`). Status definitions, coverage metrics, gap register, and drift reporting all apply.

## Project Facts

- Authoritative scope: `BRD/Doc_BRD.md`.
- Traceability matrix: `Documentation/traceability.md`. Columns: ID, BRD section, acceptance criteria, branch or PR, tests, validation, status, notes.
- Requirement IDs (CLAUDE.md §4.1 scheme), as proposed for the BRD:

| Area | IDs |
|---|---|
| Patient management | FR-PAT-001 add patient, FR-PAT-002 edit patient, FR-PAT-003 view patient, FR-PAT-004 capture name / age or DOB / gender / contact, FR-PAT-005 search by name or phone |
| Appointments | FR-APT-001 schedule, FR-APT-002 daily list, FR-APT-003 status (Scheduled, Completed, Cancelled, No-show) |
| Vitals | FR-VIT-001 mandatory temperature, BP, pulse per consultation |
| Complaints | FR-CMP-001 free-text complaints |
| Diagnosis | FR-DIA-001 diagnosis notes |
| Prescriptions | FR-RX-001 medicines (name, dosage, frequency, duration, instructions), FR-RX-002 printable prescription |
| History | FR-HIS-001 previous visits, FR-HIS-002 access vitals / complaints / diagnosis / prescriptions, FR-HIS-003 filter by date |
| Search and navigation | FR-SRC-001 quick search, FR-SRC-002 recent patients, FR-SRC-003 profile and visit navigation |
| Export | FR-EXP-001 CSV, FR-EXP-002 PDF |
| Non-functional | NFR-USB-001, NFR-PRF-001 (page load < 2 s), NFR-PRF-002 (search 2 to 5 s), NFR-REL-001 (no data loss), NFR-REL-002 (automated backups), NFR-SEC-001 (single-user login), NFR-SEC-002 (encryption at rest and in transit), NFR-SCL-001, NFR-CMP-001 (Chrome, Edge, Safari) |
| Success criteria | SC-001 consultation in 2 to 3 minutes, SC-002 search and history in 2 to 5 seconds, SC-003 80% paper reduction, SC-004 prescription print, SC-005 CSV/PDF export, SC-006 low training need |

Note: these IDs are proposed. They become authoritative only after the BRD is amended to carry them (a `docs/brd` PR). Until then, record the mismatch as a gap.

## Coverage Metrics (PMS release gate)

Use the generic metrics. The PMS gate, taken from CLAUDE.md §13:

- Requirement coverage plus Deferred-with-owner-approval equals 100 percent of the release scope.
- Criterion coverage is 100 percent for Must-priority requirements, including the physician sign-offs in the verification agent's table.
- Zero blocker gaps.
- Zero orphan tests and zero unexplained drift.

Report the counts behind each metric: for example "34 of 36 in-scope IDs Implemented; 2 Deferred (SC-003, SC-006) with owner approval".

## Project Checks

- Success criteria that can only be measured in use (SC-003, SC-006) are marked `Deferred` with the measurement plan, not `Implemented`.
- NFR-REL-002 (backups) and NFR-SEC-002 (encryption) are checked against deployment configuration, not only code.
- NFR-SEC-001 is checked for every patient-data endpoint in the API surface.
- Every `Partial` or `Deferred` status names an owner and a target release.
- The mismatch between proposed IDs and the BRD is reported as a gap until the `docs/brd` PR lands.

## Stop Conditions (project)

- The BRD does not yet carry the IDs and the amendment is not approved. Report and stop before changing the matrix.
- A release-gate metric cannot be computed because a criterion has no ID. Report it; do not estimate.

## Backward Compatibility

The ID table, deferral rules, and stop conditions are unchanged. The release-gate metrics and criterion-level columns are added.
