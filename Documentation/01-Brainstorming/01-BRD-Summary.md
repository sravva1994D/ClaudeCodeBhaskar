# 01 — BRD Summary

**Source:** `BRD/Doc_BRD.md` (single source of truth; amended 2026-11-10)
**Project:** Patient Management System — Phase 1 (localhost learning project)
**Module:** 1 — Brainstorming (`Reference/AI Training & Progress Tracking Plan.docx`)
**Status:** Refreshed 2026-11-10

---

## 1. Product Goal (BRD: Product Goal)

Build a simple, web-based Patient Management Application for a general physician to manage daily clinical activities: appointment scheduling, patient records, complaints, diagnosis, and medication.

Goal: reduce manual paperwork, improve consultation efficiency, and keep patient history accurate and easy to access.

## 2. Problem Statement (BRD: Problem Statement)

Small-clinic general physicians rely on paper or fragmented tools, causing:
- Slow patient lookup and history tracking
- Risk of lost or incomplete records
- Inefficient consultation workflow
- No structured medical records

## 3. Users (BRD: Users and Stakeholders)

| Type | Who | Note |
|---|---|---|
| Primary | General Physician (single user) | Also Clinic Owner, Product Owner and Project Owner (single decision-maker) |
| Secondary | None | Receptionist access excluded from Phase 1 |

## 4. Phase 1 Scope (BRD: Scope)

1. Web-based access (browser; desktop only; localhost)
2. Patient registration and profile management
3. Appointment scheduling and tracking
4. Recording patient complaints (symptoms)
5. Diagnosis documentation
6. Medication and prescription management
7. Printable prescriptions (A4 printable view: header, footer, content)
8. Mandatory vitals capture (temperature, blood pressure, pulse)
9. Patient visit history tracking
10. Basic search
11. Data export (CSV; PDF via printable view and browser Save as PDF)

## 5. Out of Scope (BRD: Out of Scope)

| Ref | Item | Status |
|---|---|---|
| OOS-001 | Receptionist or multi-user access | Out of scope |
| OOS-002 | Billing and invoicing | Out of scope |
| OOS-003 | Insurance processing | Out of scope |
| OOS-004 | Integration with labs or pharmacies | Out of scope |
| OOS-005 | AI-based diagnosis or recommendations | Out of scope |
| OOS-006 | Offline functionality | Out of scope |
| OOS-007 | Mobile application | Out of scope |
| OOS-008 | Advanced analytics and reporting | Out of scope |
| OOS-009 | Multi-doctor or multi-clinic support | Out of scope |
| OOS-010 | Follow-up alerts and reminders | Out of scope |
| OOS-011 | Backups and disaster recovery | Deferred (2026-11-10) |
| OOS-012 | Additional vitals (respiratory rate, weight, height, oxygen saturation) | Out of Phase 1 (2026-11-10) |
| OOS-013 | Audit trail and edit history | Out of Phase 1 (2026-11-10) |
| OOS-014 | Business KPI and usability measurement (SC-003, SC-006) | Deferred / not measured (2026-11-10) |
| OOS-015 | Data retention, deletion and consent (OQ-017) | Deferred (2026-11-10) |

## 6. Success Criteria (BRD: Success Criteria)

| Ref | Criterion (as written in BRD) | Status |
|---|---|---|
| SC-001 | Consultation record within 2–3 minutes (usability goal, localhost test data; not an SLA) | Active |
| SC-002 | Patient search and history retrieval within 2–5 seconds (localhost) | Active |
| SC-003 | At least 80% reduction in paper usage | Deferred / not measured |
| SC-004 | Smooth generation and printing of prescriptions | Active |
| SC-005 | Successful export of data in CSV and PDF format | Active |
| SC-006 | High usability with minimal training | Deferred / not measured |

## 7. Requirement Count (BRD as written, refreshed)

| Area | Count |
|---|---|
| Functional requirements (BRD bullets, grouped) | 9 areas → 20 requirement IDs (all in scope) |
| Non-functional requirements | 6 categories → 9 requirement IDs (7 in scope; 2 deferred) |
| Success criteria | 6 (4 active; 2 deferred) |
| Out-of-scope items | 15 (OOS-001 to OOS-015) |

Requirement IDs are assigned in `11-Requirement-Catalog.md`. The BRD itself uses IDs only in its Requirement Priorities table.

## 8. BRD Observations (refreshed)

- **Requirement IDs:** assigned in this analysis (see `11-Requirement-Catalog.md`). The BRD's Requirement Priorities table now uses them.
- **Priorities:** MoSCoW is in the BRD and was approved by the Project Owner on 2026-11-10 (A-006).
- **Open Questions:** the BRD section is updated. No questions remain open. Four items are deferred or excluded (OQ-017, OQ-018, OQ-019, OQ-025).
- **Terminology:** consultation and visit are the same record (OQ-028), as stated in the BRD.
- **Appointment lifecycle:** Scheduled → Checked-In → In Consultation → Completed, with Cancelled and No-show branches. No reversal (PDR-08, resolved 2026-11-10).

## 9. Related Artifacts

- `02-Business-Objectives.md`
- `03-Scope-Analysis.md`
- `04-Stakeholder-Analysis.md`
- `05-Functional-Requirements.md`
- `06-Non-Functional-Requirements.md`
- `07-Assumptions-Log.md`
- `08-Risk-Register.md`
- `09-Constraints-Register.md`
- `10-Acceptance-Criteria.md`
- `11-Requirement-Catalog.md`
- `12-Requirement-Traceability-Matrix.md`
- `13-Open-Questions.md`
- `14-Decision-and-Issue-Log.md`
- `15-Pre-Commit-Decision-Summary.md`
- `00-Review-Summary.md`
- `../02-Planning/` (Module 2)
