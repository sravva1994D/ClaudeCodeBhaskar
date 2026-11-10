# 12 — Requirement Traceability Matrix (Module 1)

**Purpose:** Trace each requirement ID to its BRD section, priority, business objective, success criterion, acceptance criteria, open questions, and decision/issue log entries.
**Priority:** MoSCoW, proposed in `BRD/Doc_BRD.md` → Requirement Priorities (approved by the Project Owner 2026-11-10, A-006 / DEC-006).
**Status legend:** `Traced` = BRD source confirmed; all linked open questions answered (2026-11-10). `Deferred` = in the BRD but not in Phase 1. `Excluded` = out of scope.
**Downstream columns** (Planning, Implementation, Test, Code Review) are in `Documentation/02-Planning/18-Planning-Traceability-Matrix.md`.
**Decision/Issue refs:** see `14-Decision-and-Issue-Log.md`.
**Status:** Refreshed 2026-11-10

| Req ID | Priority | BRD Section | BRD Wording (short) | Business Objective | Success Criterion | Acceptance Ref | Open Questions (all answered) | Status |
|---|---|---|---|---|---|---|---|---|
| FR-PAT-001 | Must Have | FR > Patient Management | Add, edit, view; capture Name, DOB, Gender, Contact | BO-001, BO-004 | — | 10 §1 | OQ-001, OQ-002, OQ-003 | Traced |
| FR-PAT-002 | Must Have | FR > Patient Management | Edit patient details | BO-004 | — | 10 §1 | OQ-024 | Traced |
| FR-PAT-003 | Must Have | FR > Patient Management | View patient details | BO-003 | — | 10 §1 | — | Traced |
| FR-PAT-004 | Must Have | FR > Patient Management | Search by name or phone | BO-003 | SC-002 | 10 §1 | OQ-001, OQ-003 | Traced |
| FR-APT-001 | Must Have | FR > Appointment Management | Schedule appointments | BO-004 | — | 10 §1 | OQ-004, OQ-005 | Traced |
| FR-APT-002 | Must Have | FR > Appointment Management | View daily list | BO-004 | — | 10 §1 | OQ-004 | Traced |
| FR-APT-003 | Must Have | FR > Appointment Management | Lifecycle: Scheduled, Checked-In, In Consultation, Completed, Cancelled, No-show (no reversal; Completed only after In Consultation) | BO-004 | — | 10 §1 | OQ-006, OQ-024 | Traced |
| FR-VIT-001 | Must Have | FR > Consultation > Vitals (Mandatory) | Record temp, BP, pulse for every consultation | BO-002 | SC-001 | 10 §1 | OQ-007, OQ-028 | Traced |
| FR-CMP-001 | Must Have | FR > Consultation > Complaints | Enter symptoms (free text) | BO-002 | SC-001 | 10 §1 | OQ-006, OQ-008 | Traced |
| FR-DIA-001 | Must Have | FR > Consultation > Diagnosis | Record diagnosis notes | BO-002 | SC-001 | 10 §1 | OQ-008 | Traced |
| FR-RX-001 | Must Have | FR > Consultation > Medication | Name, dosage, frequency, duration, instructions | BO-002 | SC-001 | 10 §1 | OQ-009 | Traced |
| FR-RX-002 | Must Have | FR > Consultation > Medication | Printable A4 prescription with header, patient, vitals, diagnosis, medicines, footer | BO-001, BO-005 | SC-004 | 10 §1 | OQ-010, OQ-011 | Traced |
| FR-HIS-001 | Must Have | FR > Patient History | View previous visits | BO-003 | SC-002 | 10 §1 | OQ-006 | Traced |
| FR-HIS-002 | Must Have | FR > Patient History | Access vitals, complaints, diagnosis, prescriptions | BO-003 | — | 10 §1 | OQ-011 | Traced |
| FR-HIS-003 | Should Have | FR > Patient History | Filter by single date or range | BO-003 | — | 10 §1 | OQ-012 | Traced |
| FR-SRC-001 | Must Have | FR > Search & Navigation | Quick patient search | BO-003 | SC-002 | 10 §1 | OQ-003 | Traced |
| FR-SRC-002 | Should Have | FR > Search & Navigation | View 5 recent patients by last visit | BO-003 | — | 10 §1 | OQ-013 | Traced |
| FR-SRC-003 | Should Have | FR > Search & Navigation | Navigate profile and visits | BO-002 | — | 10 §1 | OQ-006 | Traced |
| FR-EXP-001 | Must Have | FR > Data Export | Export patient data with visits as CSV | BO-001, BO-006 | SC-005 | 10 §1 | OQ-014 | Traced |
| FR-EXP-002 | Must Have | FR > Data Export | Export as PDF (printable view; browser Save as PDF) | BO-001, BO-006 | SC-005 | 10 §1 | OQ-014 | Traced |
| NFR-USB-001 | Must Have | NFR > Usability | Simple, minimal UI for fast entry | BO-002, BO-007 | SC-006 | 10 §2 | OQ-021 | Traced (qualitative) |
| NFR-PRF-001 | Must Have | NFR > Performance | Page load approximately 2 s (localhost) | BO-003 | SC-002 | 10 §2 | OQ-020 | Traced |
| NFR-PRF-002 | Must Have | NFR > Performance | Search results within 2–5 s (localhost) | BO-003 | SC-002 | 10 §2 | OQ-020 | Traced |
| NFR-REL-001 | Won't Have (Phase 1) | NFR > Reliability | No data loss | BO-003 | — | 10 §2 | OQ-019 | Deferred |
| NFR-REL-002 | Won't Have (Phase 1) | NFR > Reliability | Automated backups | BO-003 | — | 10 §2 | OQ-018 | Deferred |
| NFR-SEC-001 | Must Have | NFR > Security | Secure login, single user | — | — | 10 §2 | OQ-015 | Traced |
| NFR-SEC-002 | Must Have | NFR > Security | Encryption at rest and in transit | — | — | 10 §2 | OQ-016, OQ-017 | Traced |
| NFR-SCL-001 | Must Have | NFR > Scalability | Single clinic, up to 1,000 records | — | — | 10 §2 | OQ-023 | Traced |
| NFR-CMP-001 | Must Have | NFR > Compatibility | Latest Chrome, Edge, Safari (desktop) | — | — | 10 §2 | OQ-022 | Traced |
| SC-001 | Must Have (verification of Must Have requirements) | Success Criteria | Consultation record in 2–3 min | BO-002 | SC-001 | 10 §3 | OQ-021 | Traced |
| SC-002 | Must Have (verification of Must Have requirements) | Success Criteria | Search/history in 2–5 s | BO-003 | SC-002 | 10 §3 | OQ-020 | Traced |
| SC-003 | Won't Have (Phase 1) | Success Criteria | ≥80% paper reduction | BO-001, BO-004 | SC-003 | 10 §3 | OQ-021 | Deferred |
| SC-004 | Must Have (verification of Must Have requirements) | Success Criteria | Smooth prescription generation and printing | BO-005 | SC-004 | 10 §3 | OQ-010 | Traced |
| SC-005 | Must Have (verification of Must Have requirements) | Success Criteria | CSV and PDF export succeeds | BO-006 | SC-005 | 10 §3 | OQ-014 | Traced |
| SC-006 | Won't Have (Phase 1) | Success Criteria | High usability, minimal training | BO-007 | SC-006 | 10 §3 | OQ-021 | Deferred |
| OOS-001 | Won't Have (Phase 1) | Out of Scope | Receptionist or multi-user access | — | — | 11 §4 | — | Excluded |
| OOS-002 | Won't Have (Phase 1) | Out of Scope | Billing and invoicing | — | — | 11 §4 | — | Excluded |
| OOS-003 | Won't Have (Phase 1) | Out of Scope | Insurance processing | — | — | 11 §4 | — | Excluded |
| OOS-004 | Won't Have (Phase 1) | Out of Scope | Lab or pharmacy integration | — | — | 11 §4 | — | Excluded |
| OOS-005 | Won't Have (Phase 1) | Out of Scope | AI-based diagnosis or recommendations | — | — | 11 §4 | — | Excluded |
| OOS-006 | Won't Have (Phase 1) | Out of Scope | Offline functionality | — | — | 11 §4 | — | Excluded |
| OOS-007 | Won't Have (Phase 1) | Out of Scope | Mobile application | — | — | 11 §4 | OQ-022 | Excluded |
| OOS-008 | Won't Have (Phase 1) | Out of Scope | Advanced analytics and reporting | — | — | 11 §4 | — | Excluded |
| OOS-009 | Won't Have (Phase 1) | Out of Scope | Multi-doctor or multi-clinic support | — | — | 11 §4 | — | Excluded |
| OOS-010 | Won't Have (Phase 1) | Out of Scope | Follow-up alerts and reminders | — | — | 11 §4 | — | Excluded |
| OOS-011 | Won't Have (Phase 1) | Out of Scope (deferred) | Backups and disaster recovery | — | — | 11 §4 | OQ-018, OQ-019 | Deferred |
| OOS-012 | Won't Have (Phase 1) | Out of Scope | Additional vitals (respiratory rate, weight, height, SpO2) | — | — | 11 §4 | OQ-007 | Excluded |
| OOS-013 | Won't Have (Phase 1) | Out of Scope | Audit trail / edit history | — | — | 11 §4 | OQ-025 | Excluded |
| OOS-014 | Won't Have (Phase 1) | Out of Scope (deferred) | Business KPI and usability measurement (SC-003, SC-006) | — | SC-003, SC-006 | 11 §4 | OQ-021 | Deferred |
| OOS-015 | Won't Have (Phase 1) | Out of Scope (deferred) | Data retention, deletion and consent | — | — | 11 §4 | OQ-017 | Deferred |

## Priority Summary

| Priority | Functional | Non-Functional | Success Criteria | Out of Scope |
|---|---|---|---|---|
| Must Have | 17 | 7 | 4 (as verification of Must Have items) | — |
| Should Have | 3 (FR-HIS-003, FR-SRC-002, FR-SRC-003) | — | — | — |
| Could Have | 0 | 0 | — | — |
| Won't Have (Phase 1) | — | 2 (NFR-REL-001, NFR-REL-002) | 2 (SC-003, SC-006) | 15 (OOS-001 to OOS-015) |

## Coverage Checks

| Check | Result |
|---|---|
| Every BRD functional bullet has at least one requirement ID | Yes |
| Every BRD non-functional bullet has at least one requirement ID | Yes |
| Every success criterion has an ID | Yes (SC-001 to SC-006) |
| Every out-of-scope item has an OOS ID and a Won't Have priority | Yes (OOS-001 to OOS-015) |
| Every business objective is served by at least one requirement | Yes (BO-001 to BO-007) |
| Every requirement has a priority | Yes (proposed; pending approval) |
| In-scope requirements with status `Traced` (no open question) | 31 of 35 (4 deferred) |

## Result

All 31 in-scope requirement IDs are traced. Their linked open questions are answered. The four deferred IDs (NFR-REL-001, NFR-REL-002, SC-003, SC-006) are recorded as deferred.

Priorities were approved by the Project Owner on 2026-11-10 (A-006). Changing any priority changes the Phase 1 scope boundary and needs Project Owner approval.
