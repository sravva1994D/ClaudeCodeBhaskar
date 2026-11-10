# 05 — Screen Catalog

**Source:** BRD: Functional Requirements, Non-Functional Requirements
**Rule:** Each screen exists only to serve a BRD requirement. No screen is added for a feature the BRD does not describe.
**Status:** Proposed for review

| Screen ID | Name | Purpose | Req IDs | Primary actions | Notes |
|---|---|---|---|---|---|
| SCR-01 | Login | Sign in with username and password | NFR-SEC-001 | Sign in; go to Forgot password | Only public page with SCR-02 |
| SCR-02 | Forgot Password | Reset password by email | NFR-SEC-001 | Submit email, new password, confirm; sign in | Fields per BRD: email, new password, confirm password. No second verification (DEC-009, RSK-003) |
| SCR-03 | Home | Daily starting point | FR-APT-002, FR-SRC-001, FR-SRC-002 | Quick search; open today's appointments; open recent patients; New patient | Composes three requirements. No extra widgets |
| SCR-04 | Patient Search Results | Find patients by name or phone | FR-PAT-004, FR-SRC-001 | Open profile; Register if no match | Shows name, patient code, DOB, phone |
| SCR-05 | Register / Edit Patient | Add or change patient details | FR-PAT-001, FR-PAT-002 | Save; Continue despite possible duplicate | One form, two modes. Duplicate warning shown here |
| SCR-06 | Patient Profile | View all patient details and entry points | FR-PAT-003, FR-SRC-003 | Edit; Schedule; Start consultation; Visit history; Export | Shows age (calculated) and last visit |
| SCR-07 | Visit History | List visits for one patient | FR-HIS-001, FR-HIS-003 | Filter by single date or range; open visit | Newest first |
| SCR-08 | Visit Detail | Read a visit's content | FR-HIS-002 | Edit (if not printed); Print prescription; Back | Vitals, complaints, diagnosis, medicines |
| SCR-09 | Schedule Appointment | Book a 30-minute slot for a registered patient | FR-APT-001 | Choose patient; choose date and slot; Save | Slot rules per PDR-06, PDR-07 |
| SCR-10 | Daily Appointments | List appointments for one day | FR-APT-002, FR-APT-003 | Change date; change status; Start consultation | Status change is inline (no separate page) |
| SCR-11 | Consultation | Record vitals, complaints, diagnosis and medicines in one screen | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001 | Save; Print prescription; Back to patient | Single page for the SC-001 workflow (CON-016). Vitals section shows warnings |
| SCR-12 | Prescription (Print) | A4 printable prescription | FR-RX-002, SC-004 | Print button (window.print()); browser Save as PDF | Header, patient details, vitals, diagnosis, medicines, footer signature. PDR-16, PDR-18 |
| SCR-13 | Export Patient Data | Choose format and optional date range | FR-EXP-001, FR-EXP-002 | Download CSV; Download PDF | Single patient only (BRD: Export) |
| SCR-14 | Clinic Profile | Maintain prescription header data | FR-RX-002 | Save | Supports the header in SCR-12. Entry method per PDR-15 (clinic profile screen) |

## Screen Groups

| Group | Screens |
|---|---|
| Access | SCR-01, SCR-02 |
| Patients | SCR-03, SCR-04, SCR-05, SCR-06 |
| History | SCR-07, SCR-08 |
| Appointments | SCR-09, SCR-10 |
| Consultation and prescription | SCR-11, SCR-12 |
| Export | SCR-13 |
| Setup | SCR-14 |

## Cross-Cutting Screen Requirements

| Requirement | Applies to | Design rule |
|---|---|---|
| NFR-USB-001 (fast entry) | All data-entry screens | Keyboard tab order; required fields marked; no nested dialogs |
| NFR-PRF-001 (page load about 2 s) | All screens | Lean pages; no heavy client libraries |
| NFR-CMP-001 (Chrome, Edge) | All screens | Standard controls only |
| Desktop only (BRD: Scope) | All screens | Fixed desktop layout; no mobile breakpoints required |
| Warnings vs errors (FR-VIT-001, FR-PAT-001) | SCR-05, SCR-09, SCR-11 | Errors block save. Warnings show a message and require confirmation where the BRD says so |
| Printed prescriptions read-only (FR-RX-002) | SCR-08, SCR-11, SCR-12 | Edit controls hidden or disabled once printed |

## Not Screens (on purpose)

- No receptionist or user-management screens (OOS-001).
- No billing, reporting dashboards, analytics or reminders (OOS-002, OOS-008, OOS-010).
- No backup or audit-trail screens (OOS-011, OOS-013).
- No change-password screen. The BRD describes login and forgot password only.
