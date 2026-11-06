# Requirements Traceability Matrix

Maps each BRD requirement to its module, acceptance criteria, branch or PR, tests, and status. Source of scope: `BRD/Doc_BRD.md`. Rules: `CLAUDE.md` §4.

## Status Definitions

| Status | Meaning |
|---|---|
| Proposed | Requirement defined; no implementation yet. |
| In progress | Implementation branch open; not yet validated. |
| Implemented | Every acceptance criterion validated with evidence (see `pms-verification`). |
| Partial | Some criteria validated. The gap and reason are in the notes. |
| Deferred | Not delivered in this scope. Reason and owner approval are in the notes. |

An ID is `Implemented` only when every acceptance criterion is validated and a passing test proves it.

## Summary

| Measure | Count |
|---|---|
| Requirements in the BRD | 36 (21 functional, 9 non-functional, 6 success criteria) |
| Implemented | 0 |
| In progress | 0 |
| Proposed | 34 |
| Deferred | 2 (SC-003, SC-006; owner-approved 2026-11-06; excluded from Phase 1 implementation scope) |

## Functional Requirements

| ID | Requirement | BRD section | Module | Acceptance criteria | Branch / PR | Tests | Status | Notes |
|---|---|---|---|---|---|---|---|---|
| FR-PAT-001 | Add patient details | Functional Requirements › Patient Management | Patient management | FR-PAT-001-AC-01, -02 | — | — | Proposed | |
| FR-PAT-002 | Edit patient details | Functional Requirements › Patient Management | Patient management | FR-PAT-002-AC-01, -02 | — | — | Proposed | |
| FR-PAT-003 | View patient details | Functional Requirements › Patient Management | Patient management | FR-PAT-003-AC-01, -02 | — | — | Proposed | |
| FR-PAT-004 | Capture name, age or DOB, gender, contact details | Functional Requirements › Patient Management | Patient management | FR-PAT-004-AC-01, -02, -03 | — | — | Proposed | AC-03 proposed; owner to confirm whether age or DOB is required |
| FR-PAT-005 | Search patients by name or phone number | Functional Requirements › Patient Management | Patient management | FR-PAT-005-AC-01, -02, -03 | — | — | Proposed | |
| FR-APT-001 | Schedule appointments | Functional Requirements › Appointment Management | Appointments | FR-APT-001-AC-01, -02 | — | — | Proposed | |
| FR-APT-002 | View daily appointment list | Functional Requirements › Appointment Management | Appointments | FR-APT-002-AC-01, -02 | — | — | Proposed | AC-02 proposed; owner to confirm the default date |
| FR-APT-003 | Update appointment status (Scheduled, Completed, Cancelled, No-show) | Functional Requirements › Appointment Management | Appointments | FR-APT-003-AC-01, -02, -03 | — | — | Proposed | AC-03 proposed; owner to confirm whether a closed status can change |
| FR-VIT-001 | Record temperature, blood pressure, and pulse for every consultation | Functional Requirements › Consultation Workflow › Vitals Capture (Mandatory) | Consultation | FR-VIT-001-AC-01, -02, -03, -04 | — | — | Proposed | AC-04 proposed; owner to confirm normal ranges |
| FR-CMP-001 | Enter patient symptoms (free text) | Functional Requirements › Consultation Workflow › Complaints | Consultation | FR-CMP-001-AC-01 | — | — | Proposed | |
| FR-DIA-001 | Record diagnosis notes | Functional Requirements › Consultation Workflow › Diagnosis | Consultation | FR-DIA-001-AC-01 | — | — | Proposed | |
| FR-RX-001 | Add medicines (name, dosage, frequency, duration, instructions) | Functional Requirements › Consultation Workflow › Medication / Prescription | Prescriptions | FR-RX-001-AC-01, -02, -03 | — | — | Proposed | |
| FR-RX-002 | Generate printable prescription (header, patient details, vitals, diagnosis, medications, footer) | Functional Requirements › Consultation Workflow › Medication / Prescription | Prescriptions | FR-RX-002-AC-01, -02, -03 | — | — | Proposed | Physician sign-off on the printout required |
| FR-HIS-001 | View previous visits | Functional Requirements › Patient History | History and search | FR-HIS-001-AC-01, -02 | — | — | Proposed | |
| FR-HIS-002 | Access vitals, complaints, diagnosis, and prescriptions for each visit | Functional Requirements › Patient History | History and search | FR-HIS-002-AC-01, -02 | — | — | Proposed | |
| FR-HIS-003 | Filter history by date | Functional Requirements › Patient History | History and search | FR-HIS-003-AC-01 | — | — | Proposed | |
| FR-SRC-001 | Quick patient search | Functional Requirements › Search & Navigation | History and search | FR-SRC-001-AC-01 | — | — | Proposed | |
| FR-SRC-002 | View recent patients | Functional Requirements › Search & Navigation | History and search | FR-SRC-002-AC-01 | — | — | Proposed | |
| FR-SRC-003 | Navigate easily between patient profile and visits | Functional Requirements › Search & Navigation | History and search | FR-SRC-003-AC-01 | — | — | Proposed | |
| FR-EXP-001 | Export patient or visit data as CSV | Functional Requirements › Data Export | Data export | FR-EXP-001-AC-01 to -05 | — | — | Proposed | |
| FR-EXP-002 | Export patient or visit data as PDF | Functional Requirements › Data Export | Data export | FR-EXP-002-AC-01, -02 | — | — | Proposed | |

## Non-Functional Requirements

Non-functional requirements are validated in the verification and release stages (`pms-verification`, `pms-gap-analysis`, `pms-finishing`). They have no module-level acceptance criteria yet.

| ID | Requirement | BRD section | Module | Acceptance criteria | Branch / PR | Tests | Status | Notes |
|---|---|---|---|---|---|---|---|---|
| NFR-USB-001 | Simple, minimal UI optimised for fast data entry during consultations | Non-Functional Requirements › Usability | Cross-cutting | Verification sign-off | — | — | Proposed | Physician sign-off required |
| NFR-PRF-001 | Page load time under 2 seconds | Non-Functional Requirements › Performance | Cross-cutting | Verification measurement | — | — | Proposed | Lighthouse or equivalent |
| NFR-PRF-002 | Fast patient search and retrieval | Non-Functional Requirements › Performance | Cross-cutting | Verification measurement | — | — | Proposed | Measured against SC-002 |
| NFR-REL-001 | No data loss | Non-Functional Requirements › Reliability | Cross-cutting | Verification | — | — | Proposed | Consultation save is atomic |
| NFR-REL-002 | Regular automated backups | Non-Functional Requirements › Reliability | Cross-cutting | Release verification | — | — | Proposed | Restore verified at release |
| NFR-SEC-001 | Secure login (single-user authentication) | Non-Functional Requirements › Security | Cross-cutting | Verification | — | — | Proposed | Required before any patient data is shown |
| NFR-SEC-002 | Data encryption at rest and in transit | Non-Functional Requirements › Security | Cross-cutting | Release verification | — | — | Proposed | Checked against deployment configuration |
| NFR-SCL-001 | Designed for a single clinic with moderate patient volume | Non-Functional Requirements › Scalability | Cross-cutting | Verification | — | — | Proposed | |
| NFR-CMP-001 | Works on modern browsers (Chrome, Edge, Safari) | Non-Functional Requirements › Compatibility | Cross-cutting | Verification | — | — | Proposed | |

## Success Criteria

| ID | Criterion | BRD section | Module | Acceptance criteria | Branch / PR | Tests | Status | Notes |
|---|---|---|---|---|---|---|---|---|
| SC-001 | Consultation record completed within 2–3 minutes | Success Criteria | Consultation | SC-001-AC-01, -02 | — | — | Proposed | Timed run and owner confirmation |
| SC-002 | Patient search and history retrieval within 2–5 seconds | Success Criteria | History and search | SC-002-AC-01 | — | — | Proposed | |
| SC-003 | At least 80% reduction in paper usage | Success Criteria | Cross-cutting | — | — | — | Deferred | Owner-approved deferral; see [Deferred Requirements](#deferred-requirements) |
| SC-004 | Smooth generation and printing of prescriptions | Success Criteria | Prescriptions | SC-004-AC-01 | — | — | Proposed | |
| SC-005 | Successful export of data in CSV/PDF format | Success Criteria | Data export | SC-005-AC-01 | — | — | Proposed | |
| SC-006 | High usability with minimal training required | Success Criteria | Cross-cutting | — | — | — | Deferred | Owner-approved deferral; see [Deferred Requirements](#deferred-requirements) |

## Deferred Requirements

These requirements are excluded from the Phase 1 implementation scope. They are not built, tested, or counted in the release gate for the current scope. They stay in this matrix so they can be traced when they are taken up in a future release.

| Field | SC-003 | SC-006 |
|---|---|---|
| Requirement | At least 80% reduction in paper usage | High usability with minimal training required |
| Owner approval | Project owner, 2026-11-06 | Project owner, 2026-11-06 |
| Reason for deferral | Paper reduction depends on how the clinic uses the system day to day after go-live. It cannot be measured in development or on synthetic data. | Usability and training effort are judged by real users. They cannot be measured before the physician uses the system in the clinic. |
| Baseline needed | Count of paper documents the clinic produces each week (for example prescriptions, appointment notes, and patient records), recorded for four weeks before go-live | Not applicable |
| Measurement plan | Repeat the same paper count for four weeks after go-live. Reduction = (baseline − after) ÷ baseline. Target from BRD: at least 80%. The counting method is proposed; the owner confirms it. | With the physician as first user, record (a) time to complete the first consultation without assistance and (b) the number of help requests in the first two weeks. |
| Threshold | At least 80% (BRD) | TBD (proposed; pending business confirmation) |
| Target release | Next planned release after the current MVP / Phase 1 delivery (see [release roadmap](release-roadmap.md)) | Next planned release after the current MVP / Phase 1 delivery (see [release roadmap](release-roadmap.md)) |
| Release gate | Excluded from the current scope gate (CLAUDE.md §13) | Excluded from the current scope gate (CLAUDE.md §13) |
| Re-review | At each release, and when go-live data is available | At each release, and after the first two weeks of use |

Rules for deferred requirements:

- A deferred requirement is not implemented, tested, or reported as a gap for the current scope.
- It is re-reviewed at every release. The owner decides whether to bring it into scope, keep it deferred, or drop it.
- Moving it into scope needs a new owner decision, a `docs/brd` change if the wording changes, and new acceptance criteria.
- Its ID is never reused (CLAUDE.md §4.1).

## Update Rules

- Each feature PR updates the rows for the IDs it delivers, in the same PR (CLAUDE.md §4.2).
- `Branch / PR` records the feature branch and the PR number once a PR is opened.
- `Tests` lists the test names, which include the criterion IDs.
- Status changes to `Implemented` only after verification and gap analysis show evidence for every criterion.
- Requirement IDs are never reused or renumbered.
- Changes to the BRD go through a `docs/brd` PR first.
- Changing a `Deferred` status needs a recorded owner decision in the Deferred Requirements section.
