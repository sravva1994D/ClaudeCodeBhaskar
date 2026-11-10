# 05 — Functional Requirements

**Source:** BRD: Functional Requirements (and Scope, Success Criteria)
**Rule:** Statements below use BRD wording. Where the BRD is silent on a detail, the item is marked with an Open Question and not filled in.
**Priority:** MoSCoW in the BRD, approved by the Project Owner on 2026-11-10 (see `12-Requirement-Traceability-Matrix.md`).
**Status:** Refreshed 2026-11-10. All referenced open questions are answered, deferred or excluded (`13-Open-Questions.md`). The OQ column is kept for traceability.

| Req ID | Requirement | BRD Section | Dependencies | Open Questions |
|---|---|---|---|---|
| FR-PAT-001 | Register a new patient, capturing Name, Age/DOB, Gender, and Contact details | Functional Requirements > Patient Management | None | OQ-001, OQ-002, OQ-003 |
| FR-PAT-002 | Edit patient details | Functional Requirements > Patient Management ("Add, edit, and view") | FR-PAT-001 | OQ-024 |
| FR-PAT-003 | View patient details | Functional Requirements > Patient Management | FR-PAT-001 | — |
| FR-PAT-004 | Search patients by name or phone number | Functional Requirements > Patient Management | FR-PAT-001 | OQ-001, OQ-003 |
| FR-APT-001 | Schedule an appointment | Functional Requirements > Appointment Management | FR-PAT-001 | OQ-004, OQ-005 |
| FR-APT-002 | View the daily appointment list | Functional Requirements > Appointment Management | FR-APT-001 | OQ-004 |
| FR-APT-003 | Update appointment status through the lifecycle: Scheduled, Checked-In, In Consultation, Completed, Cancelled, No-show (no reversal) | Functional Requirements > Appointment Management | FR-APT-001 | OQ-006, OQ-024 |
| FR-VIT-001 | Record vitals for every consultation: Temperature, Blood Pressure, Pulse (mandatory) | Functional Requirements > Consultation Workflow > Vitals Capture (Mandatory) | Consultation entity (OQ-006, OQ-028) | OQ-007 (partly answered 2026-11-10), OQ-028 |

> **FR-VIT-001 update (2026-11-10):** Units: Temperature °C; Blood Pressure mmHg; Pulse bpm. Missing vitals produce a warning and do not block consultation creation or saving; vitals can be updated later. Validation ranges are now in the BRD (learning purposes). Respiratory rate, weight, height and oxygen saturation are out of Phase 1. Recording the omission in an audit trail is out of Phase 1 (OQ-025, 2026-11-10); this conflicts with the earlier instruction, so that recording is not included.
| FR-CMP-001 | Enter patient symptoms (complaints) as free text | Functional Requirements > Consultation Workflow > Complaints | Consultation entity (OQ-006) | OQ-008 |
| FR-DIA-001 | Record diagnosis notes | Functional Requirements > Consultation Workflow > Diagnosis | Consultation entity (OQ-006) | OQ-008 |
| FR-RX-001 | Add medicines with Name, Dosage, Frequency, Duration, Instructions | Functional Requirements > Consultation Workflow > Medication / Prescription | Consultation entity (OQ-006) | OQ-009 |
| FR-RX-002 | Generate a printable prescription (A4 printable HTML view with a Print button) containing: clinic/doctor header, patient details, vitals, diagnosis, medications, footer (notes/signature area) | Functional Requirements > Consultation Workflow > Medication / Prescription | FR-PAT-001, FR-VIT-001, FR-DIA-001, FR-RX-001 | OQ-010, OQ-011 |
| FR-HIS-001 | View previous visits | Functional Requirements > Patient History | Consultation entity (OQ-006); FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001 | OQ-006 |
| FR-HIS-002 | Access vitals, complaints, diagnosis, and prescriptions from a visit | Functional Requirements > Patient History | FR-HIS-001 | OQ-011 |
| FR-HIS-003 | Filter visit history by date | Functional Requirements > Patient History | FR-HIS-001 | OQ-012 |
| FR-SRC-001 | Quick patient search | Functional Requirements > Search & Navigation | FR-PAT-004 | OQ-003 |
| FR-SRC-002 | View recent patients | Functional Requirements > Search & Navigation | FR-PAT-003 | OQ-013 |
| FR-SRC-003 | Navigate easily between patient profile and visits | Functional Requirements > Search & Navigation | FR-PAT-003, FR-HIS-001 | — |
| FR-EXP-001 | Export patient or visit data as CSV | Functional Requirements > Data Export | FR-PAT-001 (data), FR-HIS-001 (visit data) | OQ-014 |
| FR-EXP-002 | Export patient or visit data as PDF: printable HTML view; browser Save as PDF (no PDF library) | Functional Requirements > Data Export | FR-PAT-001 (data), FR-HIS-001 (visit data) | OQ-014 |

## Notes

- **Consultation entity:** The BRD uses "consultation", "visit", and "patient history" for overlapping things but never defines the entity. Six requirements depend on it. Raised as OQ-006 and OQ-028. Resolved: consultation and visit are the same record, which can exist without an appointment.
- **Mandatory vitals (FR-VIT-001):** The BRD says "record for every consultation". Resolved (OQ-007): a consultation can be saved without vitals, with a notice.
- **Search overlap:** FR-PAT-004 (capability) and FR-SRC-001 (quick access) are kept separate so each can be tested on its own.
- Requirements are functional statements only. No screens, data models, or APIs are specified (out of scope for this module).
