# 16 — Work Breakdown Structure

**Source:** `15-Development-Roadmap.md`, `18-Planning-Traceability-Matrix.md`
**Size scale:** S, M, L relative to each other only. No hours or dates are set here.
**Status:** Proposed for review

## 1. WBS Tree

```
1  Governance and decisions
2  Foundation (M1)
3  Patient management (M2)
4  Appointments (M3)
5  Consultation and history (M4)
6  Medication and prescription (M5)
7  History filter and recent patients (M6)
8  Export (M7)
9  Non-functional verification (M8)
10 Phase 1 acceptance (M9)
```

## 2. Work Packages

| WP ID | Work package | Deliverable | Layer | Req IDs | Depends on | Size | Milestone |
|---|---|---|---|---|---|---|---|
| **1 Governance and decisions** | | | | | | | |
| WP-1.1 | Close planning decisions | Decision record (done 2026-11-10) | Process | All | — | S | M0 |
| WP-1.2 | Refresh Module 1 artifacts | Updated 00, 01, 05, 06, 09, 12 (see review package) | Process | All | WP-1.1 | S | M0 |
| WP-1.3 | Amend BRD where decisions change it | BRD amendment (done 2026-11-10; uncommitted) | Process | All | WP-1.1 | S | M0 |
| **2 Foundation** | | | | | | | |
| WP-2.1 | Solution skeleton | Three projects, three test projects, references per AD-02 | All | All | WP-1.3 | S | M1 |
| WP-2.2 | Database and migrations | DbContext, entities, configurations, initial migration | Data | All data IDs | WP-2.1 | M | M1 |
| WP-2.3 | Authentication shell | Cookie auth, login page, global authorise, logout | Presentation, Business, Data | NFR-SEC-001 | WP-2.2 | M | M1 |
| WP-2.4 | Seeded user | Single user seeded (PDR-05) | Data | NFR-SEC-001, CON-002 | WP-2.2 | S | M1 |
| WP-2.5 | Layout and navigation | Shared layout, top bar, quick-search box | Presentation | NFR-USB-001, NFR-CMP-001 | WP-2.3 | S | M1 |
| WP-2.6 | Shared validation and result types | Result, Error, Warning, validator base | Business | FR-VIT-001, FR-PAT-001 | WP-2.1 | S | M1 |
| WP-2.7 | Test harness | Test DB setup, WebApplicationFactory base | Test | All | WP-2.2 | S | M1 |
| **3 Patient management** | | | | | | | |
| WP-3.1 | Patient entity, repository, DTOs | Patient persistence and DTOs | Data, Business | FR-PAT-001 | WP-2.2 | S | M2 |
| WP-3.2 | Patient validator and duplicate check | Required fields, format rules (PDR-22), duplicate warning | Business | FR-PAT-001 | WP-3.1 | M | M2 |
| WP-3.3 | Register and edit | Services and SCR-05 | Business, Presentation | FR-PAT-001, FR-PAT-002 | WP-3.2 | M | M2 |
| WP-3.4 | Patient profile | SCR-06 with calculated age | Business, Presentation | FR-PAT-003, FR-SRC-003 (partial) | WP-3.3 | S | M2 |
| WP-3.5 | Search | SCR-04 and quick search in top bar | Business, Data, Presentation | FR-PAT-004, FR-SRC-001 | WP-3.1 | M | M2 |
| WP-3.6 | Patient performance check (early) | Seed 1,000 patients; time search | Test | NFR-PRF-002, SC-002 | WP-3.5 | S | M2 |
| **4 Appointments** | | | | | | | |
| WP-4.1 | Appointment entity and slot rules | Slot grid, overlap check (PDR-06, PDR-07) | Data, Business | FR-APT-001 | WP-3.1 | M | M3 |
| WP-4.2 | Schedule appointment | SCR-09 | Business, Presentation | FR-APT-001 | WP-4.1 | M | M3 |
| WP-4.3 | Daily list | SCR-10 list, date selector | Business, Presentation | FR-APT-002 | WP-4.2 | S | M3 |
| WP-4.4 | Status change and archive | Status lifecycle (`19-`, PDR-08), archive (PDR-09) | Business, Presentation | FR-APT-003 | WP-4.3 | S | M3 |
| WP-4.5 | Home dashboard (partial) | SCR-03 with daily list and quick search | Presentation | FR-APT-002, FR-SRC-001 | WP-4.3, WP-3.5 | S | M3 |
| **5 Consultation and history** | | | | | | | |
| WP-5.1 | Consultation and vitals persistence | Consultation, VitalSign entities and repositories | Data | FR-VIT-001, FR-HIS-001 | WP-2.2 | M | M4 |
| WP-5.2 | Vitals validator (warnings) | Ranges and missing-vitals notice | Business | FR-VIT-001 | WP-2.6, WP-5.1 | S | M4 |
| WP-5.3 | Start consultation (walk-in and from appointment) | `Start` action; SCR-06 and SCR-10 entry points | Business, Presentation | FR-HIS-001, FR-APT-003 | WP-5.1, WP-4.3 | M | M4 |
| WP-5.4 | Consultation screen | SCR-11 with vitals, complaints, diagnosis | Presentation, Business | FR-VIT-001, FR-CMP-001, FR-DIA-001 | WP-5.2, WP-5.3 | L | M4 |
| WP-5.5 | Visit history and detail | SCR-07, SCR-08 | Business, Presentation | FR-HIS-001, FR-HIS-002, FR-SRC-003 | WP-5.4 | M | M4 |
| **6 Medication and prescription** | | | | | | | |
| WP-6.1 | Prescription and items persistence | Entities, repositories, unique index | Data | FR-RX-001, FR-RX-002 | WP-5.1 | S | M5 |
| WP-6.2 | Medicine lines in consultation screen | Add, edit, remove lines on SCR-11 | Business, Presentation | FR-RX-001 | WP-6.1, WP-5.4 | M | M5 |
| WP-6.3 | Clinic profile (header) | SCR-14 and header data (PDR-15) | Business, Data, Presentation | FR-RX-002 | WP-2.2 | S | M5 |
| WP-6.4 | Print view and immutability | SCR-12, A4 CSS, mark printed, block edits (PDR-14, PDR-16) | Business, Presentation | FR-RX-002, SC-004 | WP-6.2, WP-6.3 | M | M5 |
| **7 History filter and recent patients** | | | | | | | |
| WP-7.1 | Date filter | SCR-07 filter (single date or range) | Business, Presentation | FR-HIS-003 | WP-5.5 | S | M6 |
| WP-7.2 | Recent patients | Five most recent by last visit (PDR-11); Home list | Business, Data, Presentation | FR-SRC-002 | WP-5.5 | S | M6 |
| **8 Export** | | | | | | | |
| WP-8.1 | Export content builder | Patient, visits, vitals, diagnosis, prescriptions (PDR-17) | Business | FR-EXP-001, FR-EXP-002 | WP-5.5, WP-6.4 | M | M7 |
| WP-8.2 | CSV export | CSV writer and SCR-13 download | Business, Presentation | FR-EXP-001 | WP-8.1 | S | M7 |
| WP-8.3 | Printable export view | HTML print view for patient export, Print button, Save as PDF (PDR-18) | Business, Presentation | FR-EXP-002 | WP-8.1 | M | M7 |
| **9 Non-functional verification** | | | | | | | |
| WP-9.1 | Performance measurement | NFR-PRF-001, NFR-PRF-002, SC-002 results | Test | NFR-PRF-001, NFR-PRF-002, SC-002 | M2 to M7 | S | M8 |
| WP-9.2 | Consultation timing | SC-001 timed rehearsal | Test | SC-001 | M4, M5 | S | M8 |
| WP-9.3 | Browser matrix | Chrome, Edge, Safari run | Test | NFR-CMP-001 | M7 | S | M8 |
| WP-9.4 | Security checks | Login, timeout, hashing, TLS and encryption checks (PDR-21) | Test | NFR-SEC-001, NFR-SEC-002 | M1 | S | M8 |
| WP-9.5 | Volume check | Full screens at 1,000 patients | Test | NFR-SCL-001 | M7 | S | M8 |
| WP-9.6 | Usability walkthrough (qualitative) | Tab order, required markers, one-screen flow | Test | NFR-USB-001 | M5 | S | M8 |
| **10 Phase 1 acceptance** | | | | | | | |
| WP-10.1 | Acceptance script run | All 31 in-scope IDs | Test | All in-scope | M8 | M | M9 |
| WP-10.2 | Acceptance record | Results and sign-off | Process | All in-scope | WP-10.1 | S | M9 |

## 3. Totals

| Measure | Count |
|---|---|
| Top-level groups | 10 |
| Work packages | 43 |
| Size S | 27 |
| Size M | 15 |
| Size L | 1 (WP-5.4) |

## 4. Requirement Coverage by Work Package

| Requirement | Work packages |
|---|---|
| FR-PAT-001 | WP-2.2, WP-3.1, WP-3.2, WP-3.3 |
| FR-PAT-002 | WP-3.3 |
| FR-PAT-003 | WP-3.4 |
| FR-PAT-004 | WP-3.5 |
| FR-APT-001 | WP-4.1, WP-4.2 |
| FR-APT-002 | WP-4.3, WP-4.5 |
| FR-APT-003 | WP-4.4, WP-5.3 |
| FR-VIT-001 | WP-5.1, WP-5.2, WP-5.4 |
| FR-CMP-001 | WP-5.4 |
| FR-DIA-001 | WP-5.4 |
| FR-RX-001 | WP-6.1, WP-6.2 |
| FR-RX-002 | WP-6.1, WP-6.3, WP-6.4 |
| FR-HIS-001 | WP-5.1, WP-5.3, WP-5.5 |
| FR-HIS-002 | WP-5.5 |
| FR-HIS-003 | WP-7.1 |
| FR-SRC-001 | WP-3.5, WP-4.5 |
| FR-SRC-002 | WP-7.2 |
| FR-SRC-003 | WP-3.4, WP-5.5 |
| FR-EXP-001 | WP-8.1, WP-8.2 |
| FR-EXP-002 | WP-8.1, WP-8.3 |
| NFR-USB-001 | WP-2.5, WP-9.6 |
| NFR-PRF-001 | WP-9.1 |
| NFR-PRF-002 | WP-3.6, WP-9.1 |
| NFR-SEC-001 | WP-2.3, WP-2.4, WP-9.4 |
| NFR-SEC-002 | WP-9.4 |
| NFR-SCL-001 | WP-9.5 |
| NFR-CMP-001 | WP-2.5, WP-9.3 |
| SC-001 | WP-9.2 |
| SC-002 | WP-3.6, WP-9.1 |
| SC-004 | WP-6.4 |
| SC-005 | WP-8.2, WP-8.3 |

Deferred and not in the WBS: NFR-REL-001, NFR-REL-002, SC-003, SC-006.
