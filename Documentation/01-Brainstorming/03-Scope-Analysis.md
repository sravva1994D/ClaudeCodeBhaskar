# 03 — Scope Analysis

**Source:** BRD: Scope, Out of Scope, Functional Requirements, Non-Functional Requirements
**Rule applied:** BRD is the single source of truth. Anything not traceable to the BRD is excluded and logged, not added.
**Status:** Refreshed 2026-11-10

---

## 1. In Scope (Phase 1)

| Scope Item (BRD) | Requirement IDs | Status |
|---|---|---|
| Web-based access | NFR-CMP-001 | Defined: latest Chrome, Edge, Safari; desktop only (OQ-022) |
| Patient registration and profile management | FR-PAT-001, FR-PAT-002, FR-PAT-003 | Defined (OQ-001, OQ-002, OQ-024, OQ-026) |
| Patient search | FR-PAT-004, FR-SRC-001 | Defined: name or phone; contains match (OQ-003) |
| Appointment scheduling and tracking | FR-APT-001, FR-APT-002, FR-APT-003 | Defined: 30-minute slots, no overlap (OQ-004, OQ-005, OQ-024) |
| Complaints (symptoms) | FR-CMP-001 | Defined: free text (OQ-008) |
| Diagnosis documentation | FR-DIA-001 | Defined: free text (OQ-008) |
| Medication and prescriptions | FR-RX-001 | Defined: free text fields (OQ-009) |
| Printable prescription | FR-RX-002 | Defined: A4 printable view, header and footer (OQ-010, OQ-011) |
| Mandatory vitals | FR-VIT-001 | Defined: units and ranges; warn and allow (OQ-007) |
| Visit history | FR-HIS-001, FR-HIS-002, FR-HIS-003 | Defined: single date or range filter (OQ-012) |
| Basic search (quick search, recent patients, navigation) | FR-SRC-001, FR-SRC-002, FR-SRC-003 | Defined: 5 recent patients by last visit (OQ-013) |
| Data export CSV and PDF | FR-EXP-001, FR-EXP-002 | Defined: patient, visits, vitals, diagnosis, prescriptions; single patient, optional date range (OQ-014) |
| Secure login | NFR-SEC-001 | Defined: username and password; 8–16 characters; 24-hour sliding timeout (OQ-015) |
| Encryption at rest and in transit | NFR-SEC-002 | Defined: TDE at rest; TLS 1.2+ beyond localhost (OQ-016) |
| Backups and no data loss | NFR-REL-001, NFR-REL-002 | Deferred / out of scope for Phase 1 (OOS-011) |
| Performance targets | NFR-PRF-001, NFR-PRF-002 | Defined: localhost conditions, 1,000-record sample (OQ-020) |
| Usability | NFR-USB-001 | Qualitative only: no measurable criterion in BRD (SC-006 deferred) |
| Single-clinic scale | NFR-SCL-001 | Defined: up to 1,000 patient records (OQ-023) |

**Total: 20 functional and 9 non-functional requirement IDs (7 non-functional in scope). Success criteria: 6 (4 in scope; 2 deferred).**

## 2. Out of Scope (Phase 1)

| OOS ID | Item (BRD: Out of Scope) | Boundary Note |
|---|---|---|
| OOS-001 | Receptionist or multi-user access | Excludes any second user role. Single-user login (NFR-SEC-001) is in scope. |
| OOS-002 | Billing and invoicing | |
| OOS-003 | Insurance processing | |
| OOS-004 | Integration with labs or pharmacies | |
| OOS-005 | AI-based diagnosis or recommendations | |
| OOS-006 | Offline functionality | |
| OOS-007 | Mobile application | Native app excluded. Desktop browser only (OQ-022). |
| OOS-008 | Advanced analytics and reporting | Basic CSV and printable export (FR-EXP) is in scope. Analytics and dashboards are not. |
| OOS-009 | Multi-doctor or multi-clinic support | Single clinic, single physician (NFR-SCL-001). |
| OOS-010 | Follow-up alerts and reminders | Not implied by any in-scope requirement. |
| OOS-011 | Backups and disaster recovery | Deferred (2026-11-10); localhost-only learning project. |
| OOS-012 | Additional vitals (respiratory rate, weight, height, SpO2) | Out of Phase 1 (2026-11-10). |
| OOS-013 | Audit trail and edit history | Out of Phase 1 (2026-11-10; OQ-025). |
| OOS-014 | Business KPI and usability measurement (SC-003, SC-006) | Deferred / not measured (2026-11-10). |
| OOS-015 | Data retention, deletion and consent (OQ-017) | Deferred (2026-11-10); no real patient data. |

## 3. Boundary Cases (resolved)

| Case | Risk | Resolution |
|---|---|---|
| "Recent patients" (FR-SRC-002) | Could drift toward a reminder or worklist feature | Simple list of 5 patients by last visit (OQ-013). No reminders (OOS-010) |
| "Appointment status" (FR-APT-003) | Could drift toward reminders or notifications | Status is set manually. No alerts are implied (OOS-010) |
| "Export" (FR-EXP) | Could drift toward reporting or analytics (OOS-008) | Patient data with visits, vitals, diagnosis and prescriptions for one patient (OQ-014). Not analytics |
| Audit trail / edit history | Not in BRD; relevant to "accurate history" (BO-003) | Excluded from Phase 1 (OQ-025; OOS-013). Recorded risk RSK-013 |
| Record deletion / archiving | Not in BRD | Archive, not hard delete, for appointments, visits and status (OQ-024). No patient deletion or archiving (PDR-25) |
| Data retention and patient consent | Not in BRD; a regulatory matter | Deferred (OQ-017; OOS-015). Recorded risk RSK-003 |

## 4. Dependencies Between Scope Items

```
Patient (FR-PAT-001)
 ├── Appointment (FR-APT-001) ─── Status (FR-APT-003)
 └── Consultation/Visit (defined: same record; optional appointment; OQ-006, OQ-028)
      ├── Vitals (FR-VIT-001)
      ├── Complaints (FR-CMP-001)
      ├── Diagnosis (FR-DIA-001)
      ├── Medicines (FR-RX-001) ─── Prescription print (FR-RX-002) ← needs Vitals, Diagnosis, Patient
      └── Visit History (FR-HIS-001..003)
Search (FR-PAT-004) ─── Quick search (FR-SRC-001) ─── Recent patients (FR-SRC-002)
Export (FR-EXP-001/002) ← needs Patient and Visit data
Login (NFR-SEC-001) ← gates all features
Encryption (NFR-SEC-002) ← applies to all stored and transmitted data
Backup (NFR-REL-002) ← deferred (OOS-011)
```

Full per-requirement dependency list: `11-Requirement-Catalog.md`.

## 5. Key Scope Risks (refreshed)

- Scope creep toward out-of-scope features remains the main scope risk (RSK-007). Any addition needs a BRD amendment first (CON-017).
- Appointment lifecycle resolved 2026-11-10 (PDR-08); see `Documentation/02-Planning/19-Appointment-Status-Lifecycle.md`.
- Priorities approved 2026-11-10 (A-006).
