# 11 — Requirement Catalog

**Scope:** Master list of all requirement IDs for Phase 1, from `BRD/Doc_BRD.md`.
**Rule:** IDs are never reused or renumbered once assigned. Prefixes follow the project ID scheme (see A-003).
**Priority:** MoSCoW, proposed in `BRD/Doc_BRD.md` → "Requirement Priorities" (approved by the Project Owner 2026-11-10). See `12-Requirement-Traceability-Matrix.md` for the per-ID priority.
**Status:** Refreshed 2026-11-10

## 1. Functional Requirements (20)

| Req ID | Title | BRD Section | Dependencies | Status |
|---|---|---|---|---|
| FR-PAT-001 | Register patient | FR > Patient Management | None | Traced |
| FR-PAT-002 | Edit patient | FR > Patient Management | FR-PAT-001 | Traced |
| FR-PAT-003 | View patient | FR > Patient Management | FR-PAT-001 | Traced |
| FR-PAT-004 | Search patient by name or phone | FR > Patient Management | FR-PAT-001 | Traced |
| FR-APT-001 | Schedule appointment | FR > Appointment Management | FR-PAT-001 | Traced |
| FR-APT-002 | View daily appointment list | FR > Appointment Management | FR-APT-001 | Traced |
| FR-APT-003 | Update appointment status | FR > Appointment Management | FR-APT-001 | Traced |
| FR-VIT-001 | Record mandatory vitals | FR > Consultation Workflow > Vitals | Consultation entity | Traced |
| FR-CMP-001 | Record complaints (symptoms) | FR > Consultation Workflow > Complaints | Consultation entity | Traced |
| FR-DIA-001 | Record diagnosis notes | FR > Consultation Workflow > Diagnosis | Consultation entity | Traced |
| FR-RX-001 | Add medicines | FR > Consultation Workflow > Medication | Consultation entity | Traced |
| FR-RX-002 | Generate printable prescription | FR > Consultation Workflow > Medication | FR-PAT-001, FR-VIT-001, FR-DIA-001, FR-RX-001 | Traced |
| FR-HIS-001 | View previous visits | FR > Patient History | Consultation entity; FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001 | Traced |
| FR-HIS-002 | Access visit content | FR > Patient History | FR-HIS-001 | Traced |
| FR-HIS-003 | Filter history by date | FR > Patient History | FR-HIS-001 | Traced |
| FR-SRC-001 | Quick patient search | FR > Search & Navigation | FR-PAT-004 | Traced |
| FR-SRC-002 | View recent patients | FR > Search & Navigation | FR-PAT-003 | Traced |
| FR-SRC-003 | Navigate patient profile and visits | FR > Search & Navigation | FR-PAT-003, FR-HIS-001 | Traced |
| FR-EXP-001 | Export data as CSV | FR > Data Export | FR-PAT-001, FR-HIS-001 | Traced |
| FR-EXP-002 | Export data as PDF (printable HTML view; browser Save as PDF) | FR > Data Export | FR-PAT-001, FR-HIS-001 | Traced |

## 2. Non-Functional Requirements (9)

| Req ID | Title | BRD Section | Dependencies | Status |
|---|---|---|---|---|
| NFR-USB-001 | Simple, fast-entry UI | NFR > Usability | All UI requirements | Traced |
| NFR-PRF-001 | Page load under 2 seconds | NFR > Performance | All UI requirements | Traced |
| NFR-PRF-002 | Fast search and retrieval | NFR > Performance; SC-002 | FR-PAT-004, FR-HIS-001 | Traced |
| NFR-REL-001 | No data loss | NFR > Reliability | All data-storing requirements | Deferred (OOS-011) |
| NFR-REL-002 | Automated backups | NFR > Reliability | NFR-REL-001 | Deferred (OOS-011) |
| NFR-SEC-001 | Secure single-user login | NFR > Security | All requirements (access gate) | Traced |
| NFR-SEC-002 | Encryption at rest and in transit | NFR > Security | All data-storing requirements | Traced |
| NFR-SCL-001 | Single clinic, moderate volume | NFR > Scalability | NFR-PRF-001, NFR-PRF-002 | Traced |
| NFR-CMP-001 | Chrome, Edge, Safari support | NFR > Compatibility | All UI requirements | Traced |

## 3. Success Criteria (6)

| Ref | Criterion | BRD Section | Dependencies | Status |
|---|---|---|---|---|
| SC-001 | Consultation record in 2–3 minutes | Success Criteria | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-RX-002 | Traced |
| SC-002 | Search and history retrieval in 2–5 seconds | Success Criteria | FR-PAT-004, FR-HIS-001 | Traced |
| SC-003 | At least 80% paper reduction | Success Criteria | All in-scope FRs | Deferred (OOS-014) |
| SC-004 | Smooth prescription generation and printing | Success Criteria | FR-RX-002 | Traced |
| SC-005 | Successful CSV/PDF export | Success Criteria | FR-EXP-001, FR-EXP-002 | Traced |
| SC-006 | High usability, minimal training | Success Criteria | NFR-USB-001 | Deferred (OOS-014) |

## 4. Out-of-Scope Items (15)

| Ref | Item | BRD Section |
|---|---|---|
| OOS-001 | Receptionist or multi-user access | Out of Scope |
| OOS-002 | Billing and invoicing | Out of Scope |
| OOS-003 | Insurance processing | Out of Scope |
| OOS-004 | Lab or pharmacy integration | Out of Scope |
| OOS-005 | AI-based diagnosis or recommendations | Out of Scope |
| OOS-006 | Offline functionality | Out of Scope |
| OOS-007 | Mobile application | Out of Scope |
| OOS-008 | Advanced analytics and reporting | Out of Scope |
| OOS-009 | Multi-doctor or multi-clinic support | Out of Scope |
| OOS-010 | Follow-up alerts and reminders | Out of Scope |
| OOS-011 | Backups and disaster recovery (NFR-REL-001, NFR-REL-002) | Deferred / Out of scope for Phase 1 (amended 2026-11-10) |
| OOS-012 | Additional vitals: respiratory rate, weight, height, oxygen saturation | Out of Phase 1 (amended 2026-11-10) |
| OOS-013 | Audit trail / edit history | Out of Phase 1 (amended 2026-11-10) |
| OOS-014 | Business KPI and usability measurement (SC-003, SC-006) | Deferred / Not measured (amended 2026-11-10) |
| OOS-015 | Data retention, deletion and consent (OQ-017) | Deferred / Out of scope for Phase 1 (amended 2026-11-10) |

## 5. Totals

- Functional: 20
- Non-functional: 9
- Success criteria: 6
- Out of scope: 15
- Open questions: see `13-Open-Questions.md`
