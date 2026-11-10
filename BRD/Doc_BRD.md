# Business Requirements Document

**Version:** Phase 1 (amended 2026-11-10; planning decisions PDR-01 to PDR-32; amendments DEC-017 to DEC-022)
**Scope context:** Single-user, localhost-only learning project. Not deployed to any environment.

## Product Goal

Build a simple, web-based Patient Management Application for a general physician to efficiently manage daily clinical activities such as appointment scheduling, patient records, complaints, diagnosis, and medication.

The goal is to reduce manual paperwork, improve consultation efficiency, and maintain accurate, easily accessible patient history.

---

## Users and Stakeholders

**Primary Users:**
- General Physician (Single User)

**Secondary Users:**
- None (Receptionist access not included in Phase 1)

**Stakeholders (single decision-maker):**
- Clinic Owner = General Physician = Product Owner = Project Owner

**Development Team:** Developer (learning project)

---

## Problem Statement

General physicians in small clinics often rely on paper-based systems or fragmented tools to manage patient data and appointments. This results in:
- Slow patient lookup and history tracking
- Risk of lost or incomplete records
- Inefficient consultation workflow
- Lack of structured medical records

A lightweight, web-based solution is needed to streamline and digitize daily operations.

---

## Scope

The first version of the application will include:

- Web-based access (browser-based system, desktop only, localhost)
- Patient registration and profile management
- Appointment scheduling and tracking
- Recording patient complaints (symptoms)
- Diagnosis documentation
- Medication and prescription management
- Printable prescriptions (A4 printable view; header, footer, and content)
- Mandatory vitals capture (temperature, blood pressure, pulse)
- Patient visit history tracking
- Basic search functionality
- Data export (CSV; PDF via printable view)

---

## Out of Scope

The following will NOT be included in Phase 1:

| Ref | Item | Status | Reason |
|---|---|---|---|
| OOS-001 | Receptionist or multi-user access | Out of scope | Single-user product |
| OOS-002 | Billing and invoicing | Out of scope | Not in product goal |
| OOS-003 | Insurance processing | Out of scope | Not in product goal |
| OOS-004 | Integration with labs or pharmacies | Out of scope | Not in product goal |
| OOS-005 | AI-based diagnosis or recommendations | Out of scope | Not in product goal |
| OOS-006 | Offline functionality | Out of scope | Localhost-only; always connected |
| OOS-007 | Mobile application | Out of scope | Desktop browser only |
| OOS-008 | Advanced analytics and reporting | Out of scope | Not in product goal |
| OOS-009 | Multi-doctor or multi-clinic support | Out of scope | Single clinic, single physician |
| OOS-010 | Follow-up alerts/reminders | Out of scope | Not in product goal |
| OOS-011 | Backups and disaster recovery (NFR-REL-001, NFR-REL-002) | Deferred / Out of scope for Phase 1 | Localhost-only learning project; revisit if deployed beyond a local environment |
| OOS-012 | Additional vitals: respiratory rate, weight, height, oxygen saturation | Out of Phase 1 | Phase 1 vitals are temperature, blood pressure and pulse only |
| OOS-013 | Audit trail / edit history (including logging of omitted vitals) | Out of Phase 1 | Revisit later; no Phase 1 dependency on audit logging |
| OOS-014 | Business KPI and usability measurement (SC-003, SC-006) | Deferred / Not measured | No real clinic baseline; no formal usability study |
| OOS-015 | Data retention, deletion and consent (OQ-017) | Deferred / Out of scope for Phase 1 | Localhost-only learning project; no real patient data; revisit if deployed beyond a local environment |

---

## Success Criteria

- Doctor can complete a consultation record within 2–3 minutes (usability goal, measured on localhost test data from opening a consultation record to saving it; not an SLA)
- Patient search and history retrieval within 2–5 seconds under normal localhost development conditions
- At least 80% reduction in paper usage: **Deferred / Not measured** (no real clinic baseline)
- Smooth generation and printing of prescriptions
- Successful export of data in CSV/PDF format
- High usability with minimal training required: **Deferred / Not measured** (no formal usability study)

---

## Functional Requirements

### Patient Management
- Add, edit, and view patient details
- Capture:
  - Name (required)
  - Date of birth (required, not in the future); Age is calculated from DOB and the current system date
  - Gender (required): Male, Female or Other
  - Contact details (required): phone (7 to 15 digits; spaces, hyphens and a leading + allowed), email (standard format), address
- Each patient has a unique system-generated identifier with a prefix (for example PT-000123)
- If a suspected duplicate is found (same name, DOB or phone; exact match, name ignores upper and lower case), show a warning and let the doctor continue
- Search patients by name or phone number (contains match, ignores case, no minimum length)

---

### Appointment Management
- Schedule appointments
  - Slots are 30 minutes; each appointment takes the full slot. Clinic slots: 09:00–13:00 and 17:00–20:00 (last slot starts 12:30 and 19:30)
  - Appointments cannot overlap. Cancelled and No-show appointments release their slot
  - A patient must be registered before an appointment is scheduled
- View daily appointment list
- Update appointment status through this lifecycle:
  - Scheduled → Checked-In → In Consultation → Completed
  - Scheduled → Cancelled; Scheduled → No-show; Checked-In → Cancelled
  - No other moves. An appointment cannot be marked Completed from Scheduled or Checked-In; it reaches Completed only after In Consultation. No status is changed back to an earlier status in Phase 1
  - "Completed" means the business process for the appointment is finished. The consultation's Finish action sets it (DEC-022)
- Appointments are archived (hidden but kept), not hard-deleted. Archiving is a manual action. Archived appointments are hidden everywhere in the application

---

### Consultation Workflow

A consultation and a visit are the same record. A consultation can be started directly (walk-in or unscheduled) without an appointment. Walk-in patients are registered at arrival. Each appointment leads to at most one consultation. The visit date and time are set by the system when the consultation starts and cannot be edited.

#### Vitals Capture (Mandatory)
- Record for every consultation:
  - Temperature (°C)
  - Blood pressure (mmHg): systolic and diastolic
  - Pulse (bpm)
- Validation ranges (learning purposes, to detect likely data-entry errors):
  - Temperature: 34.0–42.0 °C
  - Systolic blood pressure: 70–250 mmHg
  - Diastolic blood pressure: 40–150 mmHg
  - Pulse: 30–220 bpm
- Values outside these ranges show a warning and require user confirmation before saving. Saving is not blocked.
- Missing vitals show a notice only and do not block consultation creation or saving; no confirm step. Vitals can be added or updated later.
- Not in Phase 1: respiratory rate, weight, height, oxygen saturation

---

#### Complaints
- Enter patient symptoms (free text only; no coded terms; one text block per visit)

---

#### Diagnosis
- Record diagnosis notes (free text only; no coded terms)

---

#### Medication / Prescription
- Add medicines with (all free text):
  - Name
  - Dosage
  - Frequency
  - Duration
  - Instructions
- Only Name is required; Dosage, Frequency, Duration and Instructions are optional. A prescription needs at least one medicine before it can be printed.

- One prescription per consultation. A prescription cannot be edited after it is printed. The Print command marks it as printed; reprinting is allowed.

- Generate **printable prescription** on A4 as a printable HTML view with a Print button. The browser's Print or Save as PDF produces the printed or PDF copy. The system does not generate PDF files itself. Content:
  - Header: clinic name, address, phone, doctor name, registration number, logo (Phase 1: a placeholder logo image; final branding not required, DEC-021)
  - Patient details
  - Vitals
  - Diagnosis
  - Medications
  - Footer: signature

---

### Patient History
- View previous visits
- Access:
  - Vitals
  - Complaints
  - Diagnosis
  - Prescriptions
- Filter by a single date or a date range

---

### Search & Navigation
- Quick patient search
- View recent patients: the 5 most recently visited patients, ordered by last visit. A visit is a consultation; patients with no consultation are not listed
- Easy navigation between patient profile and visits

---

### Data Export
- Export patient data, with its visits, vitals, diagnosis and prescriptions, as:
  - CSV: one file, one row per visit, patient details repeated on each row; dates in ISO format (yyyy-MM-dd); user-relevant columns only (DEC-020)
  - PDF: a printable HTML view with a Print button; the browser's Save as PDF produces the file. No server-side PDF library.
- Scope: a single patient, optionally limited to a date range

---

## Non-Functional Requirements

- **Usability:**
  Simple, minimal UI optimized for fast data entry during consultations

- **Performance** (under normal localhost development conditions: developer machine, local SQL Server, sample dataset of up to 1,000 patient records; targets are guidelines, not SLAs):
  - Standard application pages load in approximately 2 seconds
  - Patient search results returned within 2–5 seconds

- **Reliability:**
  - Backups and recovery: **Deferred / Out of scope for Phase 1** (see OOS-011)
  - No data loss: **Deferred / Out of scope for Phase 1** (see OOS-011)

- **Security:**
  - Secure login (single user authentication): username and password only; 8 to 16 characters, including upper-case, lower-case, digit and symbol; 24-hour idle timeout that resets on activity (sliding)
  - Forgot password: user enters email, new password and confirm password; if the email matches, the password is updated and the user is logged in. No second verification. Message shown: "If the email matches an account, the password has been reset." Risk accepted by stakeholder.
  - The single user account is created once by a console seed command, not through the application. The password is typed at a prompt and is not stored in the repository or in configuration (DEC-018)
  - Passwords are stored using secure hashing, never in plain text
  - Data encryption in transit: TLS 1.2+ required when deployed beyond localhost; plain HTTP is allowed on localhost. Passwords are stored hashed (see above). Encryption at rest is out of Phase 1 scope for this localhost-only learning project (DEC-017).

- **Scalability:**
  - Designed for a single clinic; sample volume of up to 1,000 patient records

- **Compatibility:**
  - Works on the latest versions of Chrome and Edge (desktop only). Chromium-based browsers are the primary target. No Safari-specific support is required in Phase 1 (DEC-019)

---

## Requirement Priorities

Framework: **MoSCoW** (Must Have, Should Have, Could Have, Won't Have).
Status: **Approved by the Project Owner, 2026-11-10** (sole approver). Priorities do not change any requirement wording above. Requirement IDs follow the project ID scheme (see `Documentation/01-Brainstorming/11-Requirement-Catalog.md`).

| Req ID | Requirement (short) | Priority | Rationale |
|---|---|---|---|
| FR-PAT-001 | Register patient | Must Have | Core patient registration in Phase 1 scope |
| FR-PAT-002 | Edit patient details | Must Have | "Add, edit, and view" in BRD |
| FR-PAT-003 | View patient details | Must Have | "Add, edit, and view" in BRD |
| FR-PAT-004 | Search by name or phone | Must Have | Basic search in scope; SC-002 |
| FR-APT-001 | Schedule appointments | Must Have | Appointment scheduling in scope |
| FR-APT-002 | Daily appointment list | Must Have | Named functional requirement |
| FR-APT-003 | Update appointment status | Must Have | "Tracking" in scope |
| FR-VIT-001 | Mandatory vitals | Must Have | Stated as mandatory |
| FR-CMP-001 | Record complaints | Must Have | Consultation workflow in scope |
| FR-DIA-001 | Record diagnosis | Must Have | Diagnosis documentation in scope |
| FR-RX-001 | Add medicines | Must Have | Medication management in scope |
| FR-RX-002 | Printable prescription | Must Have | In scope; SC-004 |
| FR-HIS-001 | View previous visits | Must Have | Visit history in scope |
| FR-HIS-002 | Access visit content | Must Have | Named functional requirement |
| FR-HIS-003 | Filter history by date | Should Have | Refinement of history; core history usable without it |
| FR-SRC-001 | Quick patient search | Must Have | Basic search in scope |
| FR-SRC-002 | View recent patients | Should Have | Navigation aid; not required for core workflow |
| FR-SRC-003 | Navigate profile and visits | Should Have | Navigation aid; core access exists via FR-PAT-003 and FR-HIS-001 |
| FR-EXP-001 | Export as CSV | Must Have | In scope; SC-005 |
| FR-EXP-002 | Export as PDF (printable view; browser Save as PDF) | Must Have | In scope; SC-005 |
| NFR-USB-001 | Simple, fast-entry UI | Must Have | Usability for single user |
| NFR-PRF-001 | Page load approximately 2 s (localhost) | Must Have | Stated target, localhost conditions |
| NFR-PRF-002 | Search results within 2–5 s (localhost) | Must Have | SC-002 |
| NFR-REL-001 | No data loss | Won't Have (Phase 1) | Deferred; OOS-011 |
| NFR-REL-002 | Automated backups | Won't Have (Phase 1) | Deferred; OOS-011 |
| NFR-SEC-001 | Secure single-user login | Must Have | Stated requirement |
| NFR-SEC-002 | Data in transit (TLS beyond localhost); password hashing | Must Have | Amended 2026-11-10 (DEC-017): encryption at rest is out of Phase 1 scope |
| NFR-SCL-001 | Single clinic; sample of up to 1,000 records | Must Have | Defines Phase 1 scale |
| NFR-CMP-001 | Latest Chrome and Edge (desktop) | Must Have | Amended 2026-11-10 (DEC-019): Safari not required in Phase 1 |
| SC-003 | 80% paper reduction | Won't Have (Phase 1) | Deferred; OOS-014 |
| SC-006 | High usability | Won't Have (Phase 1) | Deferred; OOS-014 |
| OOS-001 to OOS-015 | Items listed under Out of Scope | Won't Have (Phase 1) | Excluded from Phase 1 by the Out of Scope section |

---

## Decision Log (2026-11-10)

Decisions recorded from stakeholder answers. Full detail: `Documentation/01-Brainstorming/13-Open-Questions.md` and `14-Decision-and-Issue-Log.md`.

| Decision | Reason |
|---|---|
| Single decision-maker: Clinic Owner = General Physician = Product Owner = Project Owner | Single-user learning project |
| Patient fields required: Name, DOB, Gender, phone, email, address (email and address confirmed as required) | Stakeholder answer |
| Patient ID: system-generated with prefix (PT-000123); duplicates warn and allow continue | Stakeholder answer |
| Appointments: 30-minute slots, no overlap, full slot per appointment, registration required first | Stakeholder answer |
| Consultation and visit are the same record; can exist without appointment | Stakeholder answer |
| Vitals: °C, mmHg, bpm; validation warns and requires confirmation; missing vitals warn only | Stakeholder answer; ranges supplied for learning purposes |
| Additional vitals out of Phase 1 | Stakeholder answer (Phase 1 scope kept to BRD mandatory vitals) |
| Complaints, diagnosis, medicine fields are free text | Stakeholder answer |
| Prescription on A4; one per consultation; not editable after printing | Stakeholder answer |
| Visit filter: single date or range; recent patients: 5, by last visit | Stakeholder answer |
| Export: single patient, optionally date range | Stakeholder answer |
| Printable outputs (prescription, patient export) are HTML views with a Print button; PDF via browser Print or Save as PDF; no server-side PDF library; data access uses EF Core only | Project Owner decision 2026-11-10 (PDR-16, PDR-18, PDR-32) |
| Login: username and password; 8+ characters with complexity; 24-hour idle timeout; email-only password reset accepted as risk | Stakeholder answer; risk accepted |
| Encryption: TLS 1.2+ only beyond localhost; hashed passwords. Encryption at rest is out of Phase 1 scope (supersedes the earlier TDE wording) | Localhost-only learning project; DEC-017 (2026-11-10) |
| Performance: approximately 2 s page load and 2–5 s search, under localhost conditions; guidelines, not SLAs | Localhost-only learning project |
| Browsers: latest Chrome and Edge; desktop only; Safari not required (supersedes the earlier three-browser answer) | DEC-019 (2026-11-10) |
| Archive, not hard delete. Appointment status follows the lifecycle in Appointment Management; no reversal | Project Owner decision 2026-11-10 (PDR-08) |
| Backups and recovery deferred (OOS-011) | Localhost-only learning project; no deployment |
| Audit trail out of Phase 1 (OOS-013) | Stakeholder answer; revisit later |
| SC-003 and SC-006 deferred / not measured (OOS-014) | No real clinic baseline; no formal usability study |
| Data retention, deletion and consent deferred / out of scope (OOS-015, OQ-017) | Localhost-only learning project; no real patient data |
| Planning decisions PDR-01 to PDR-32 recorded; wording above reflects them. Full record: `Documentation/02-Planning/17-Planning-Decisions-and-Open-Items.md` | Project Owner decisions, 2026-11-10 |
| Patients cannot be deleted or archived in Phase 1 | Project Owner decision, 2026-11-10 (PDR-25) |
| Appointment lifecycle: Scheduled → Checked-In → In Consultation → Completed; Scheduled → Cancelled or No-show; Checked-In → Cancelled. Replaces the earlier reversal rule | Project Owner decision, 2026-11-10 (PDR-08) |
| MoSCoW priorities approved as the project baseline; Project Owner is the sole approver | Project Owner decision, 2026-11-10 (A-006) |
| Each appointment leads to at most one consultation | Project Owner decision, 2026-11-10 (PDR-26) |
| DEC-017: encryption at rest out of Phase 1 scope (localhost-only learning project); TLS beyond localhost and password hashing remain. NFR-SEC-002 amended | Project Owner decision, 2026-11-10 |
| DEC-018: the single account is created by a console seed command; the password is entered at a prompt and never stored in the repository or configuration (PDR-05 amended) | Project Owner decision, 2026-11-10 |
| DEC-019: Chromium-based browsers (Chrome, Edge) are the primary target; no Safari-specific support in Phase 1 (NFR-CMP-001 amended) | Project Owner decision, 2026-11-10 |
| DEC-020: CSV dates in ISO format (yyyy-MM-dd); user-relevant columns only; column set in `Documentation/02-Planning/10-Service-Design.md` §2.7 (PDR-17 resolved) | Project Owner decision, 2026-11-10 |
| DEC-021: header logo is a placeholder image in Phase 1; no logo upload (PDR-15 amended) | Project Owner decision, 2026-11-10 |
| DEC-022: the Archive action (hides an appointment, keeps it) and the Finish action (sets Completed after In Consultation) are both in Phase 1 | Project Owner decision, 2026-11-10 |

## Still Open

- **Data retention, deletion and consent:** Deferred / Out of scope for Phase 1 (OOS-015, OQ-017). No open items remain in this section.
- **Registration fields:** all resolved. Name, DOB, Gender, phone, email and address are required (confirmed 2026-11-10).
- **Appointment lifecycle:** resolved 2026-11-10 (PDR-08). Priorities approved 2026-11-10 (A-006).
