# 09 — Constraints Register

**Definition:** Limits that the solution must work within. Sourced from the BRD, the training plan, or the review instructions.
**Status:** Refreshed 2026-11-10

| Con ID | Type | Constraint | Source | Affected Req IDs | Notes |
|---|---|---|---|---|---|
| CON-001 | Scope | Phase 1 only. Items in the BRD Out of Scope list (OOS-001 to OOS-015) are excluded. | BRD: Out of Scope | All | See `03-Scope-Analysis.md` |
| CON-002 | Scope | Single user (one general physician). No receptionist or multi-user access. | BRD: Users; OOS-001 | NFR-SEC-001, all | |
| CON-003 | Scope | Single clinic, single physician. No multi-clinic or multi-doctor support. | BRD: OOS-009; NFR-SCL-001 | NFR-SCL-001 | |
| CON-004 | Platform | Browser-based only. No native or mobile app. | BRD: Scope; OOS-007 | NFR-CMP-001 | |
| CON-005 | Platform | Must work on modern browsers: Chrome, Edge, Safari. | BRD: NFR > Compatibility | NFR-CMP-001 | Versions undefined (OQ-022) |
| CON-006 | Connectivity | No offline functionality. | BRD: OOS-006 | All | Implies continuous connection; no offline behaviour is required |
| CON-007 | Mandatory data | Vitals (Temperature, BP, Pulse) must be captured for every consultation. | BRD: FR > Vitals Capture (Mandatory) | FR-VIT-001 | Warn and allow; missing vitals a notice only (OQ-007 answered) |
| CON-008 | Output | Prescriptions must be printable and include header, patient details, vitals, diagnosis, medications, and footer. | BRD: FR > Medication / Prescription | FR-RX-002 | |
| CON-009 | Output | Data export formats are CSV and PDF (PDF via printable HTML view and browser Save as PDF). | BRD: FR > Data Export; SC-005 | FR-EXP-001, FR-EXP-002 | |
| CON-010 | Performance | Page load time under 2 seconds. | BRD: NFR > Performance | NFR-PRF-001 | Localhost conditions defined (OQ-020 answered) |
| CON-011 | Performance | Patient search and history retrieval within 2–5 seconds. | BRD: SC-002 | NFR-PRF-002, FR-PAT-004, FR-HIS-001 | |
| CON-012 | Security | Encryption at rest and in transit. | BRD: NFR > Security | NFR-SEC-002 | Standard defined: TDE at rest; TLS beyond localhost (OQ-016 answered) |
| CON-013 | Security | Secure login with single-user authentication. | BRD: NFR > Security | NFR-SEC-001 | Username and password; 8–16 characters with complexity; 24-hour sliding timeout (OQ-015 answered) |
| CON-014 | Reliability | Deferred (OOS-011): not required in Phase 1. | BRD: NFR > Reliability | NFR-REL-001, NFR-REL-002 | |
| CON-015 | Usability | Minimal UI optimised for fast data entry during consultations. | BRD: NFR > Usability | NFR-USB-001 | |
| CON-016 | Time | Consultation record completable within 2–3 minutes. | BRD: SC-001 | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-RX-002 | Used as a design constraint on the consultation flow |
| CON-017 | Process | BRD is the single source of truth. Nothing is built that does not trace to it. | Project rule; training plan Module 1 | All | A requested change not in the BRD requires a BRD amendment first |
| CON-018 | Process | No assumptions. Gaps are recorded as open questions. | Review instructions | All | See `07-Assumptions-Log.md` |
| CON-019 | Process | No implementation or architecture design at Module 1. | Review instructions; training plan (Module sequence) | All | This artifact set contains requirements only |
| CON-020 | Process | Phase 1 is prompt-based. The agent-driven approach is Phase 2 and is not in scope here. | Training plan: Phase 2 | All | |
| CON-021 | Process | Module 1 exit requires requirement understanding review by mentor/lead. | Training plan: Module 1 progress measurement | All | Review gate |
| CON-022 | Deployment | Localhost-only learning project. Not deployed to any environment. | BRD: Scope context (2026-11-10) | All; NFR-SEC-002; NFR-PRF-001 | Basis for deferring backups (OOS-011) and for plain HTTP on localhost (PDR-21) |
| CON-023 | Technology | Printable outputs are HTML views (browser Print and Save as PDF); no PDF library. Data access uses EF Core only (no Dapper). | Project Owner decisions PDR-18 and PDR-32 (2026-11-10) | FR-RX-002; FR-EXP-002 | Recorded in the BRD Decision Log |

## Notes

- Technology decisions are now recorded: ASP.NET Core MVC, EF Core only (no Dapper), SQL Server, localhost hosting, no PDF library (CON-022, CON-023; details in `Documentation/02-Planning/`). CON-019 applied to Module 1 only.
- Constraints CON-016 and CON-011 are measured on localhost test data (see `10-Acceptance-Criteria.md`).
