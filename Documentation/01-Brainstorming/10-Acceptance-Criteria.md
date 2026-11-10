# 10 — Acceptance Criteria

**Format:** Each criterion is checkable against the BRD wording as amended on 2026-11-10.
**Status key:** Ready = testable as written. Qualitative = no measurable criterion in the BRD. Deferred = not in Phase 1.
**Status:** Refreshed 2026-11-10 (all referenced open questions answered, deferred or excluded)

## 1. Functional Requirements

| Req ID | Acceptance Criteria | Status |
|---|---|---|
| FR-PAT-001 | AC-1: A patient can be saved with Name, Date of birth, Gender (Male, Female or Other), phone, email and address. AC-2: The saved patient is retrievable by FR-PAT-003 and FR-PAT-004. AC-3: The patient has a system-generated code in the form PT-000123. AC-4: A suspected duplicate (exact match on name, DOB or phone; name ignores case) shows a warning and can be continued. AC-5: Age is calculated from DOB and the system date. AC-6: DOB is not in the future; phone is 7 to 15 digits; email has a standard format. | Ready (OQ-001, OQ-002, OQ-003; PDR-01, 10, 22) |
| FR-PAT-002 | AC-1: Existing patient details can be changed and saved. AC-2: Changes are visible on the patient profile. AC-3: No patient is deleted or archived in Phase 1. | Ready (OQ-024; PDR-25) |
| FR-PAT-003 | AC-1: The patient profile shows all details captured under FR-PAT-001. | Ready |
| FR-PAT-004 | AC-1: A name search returns matching patients (contains match, ignores case). AC-2: A phone search returns matching patients. | Ready (OQ-003; PDR-23) |
| FR-APT-001 | AC-1: An appointment can be scheduled for a registered patient. AC-2: The appointment appears in the daily list. AC-3: Appointments are 30-minute slots on the clinic grid (09:00–13:00 and 17:00–20:00; last slots start 12:30 and 19:30). AC-4: An overlapping booking is rejected. AC-5: A Cancelled or No-show appointment releases its slot. | Ready (OQ-004, OQ-005; PDR-06, 07) |
| FR-APT-002 | AC-1: The daily list shows the appointments for a chosen day. AC-2: Archived appointments are not shown. | Ready (OQ-004; PDR-09) |
| FR-APT-003 | AC-1: Status can be set through the lifecycle: Scheduled, Checked-In, In Consultation, Completed, Cancelled or No-show. AC-2: The new status is shown in the daily list. AC-3: Only the allowed moves are accepted: Scheduled → Checked-In; Scheduled → Cancelled or No-show; Checked-In → In Consultation or Cancelled; In Consultation → Completed. No reversal. An appointment cannot be marked Completed from Scheduled or Checked-In. AC-4: An appointment can be archived manually (Archive action) and is then hidden from active lists everywhere; it is kept for history. AC-5: The consultation Finish action sets Completed only from In Consultation (DEC-022). | Ready (OQ-024; PDR-08, 09, 27) |
| FR-VIT-001 | AC-1: Temperature, blood pressure and pulse can be recorded for each consultation. AC-2: A consultation saved without vitals shows a notice and is saved (no confirm step). AC-3: Vitals can be added or updated later. AC-4: Values are entered in °C, mmHg and bpm. AC-5: Values outside Temperature 34.0–42.0 °C, Systolic 70–250 mmHg, Diastolic 40–150 mmHg or Pulse 30–220 bpm show a warning and require confirmation before saving. Saving is not blocked. | Ready (OQ-007, OQ-028; PDR-19) |
| FR-CMP-001 | AC-1: Symptoms can be entered as free text, one block per visit. AC-2: The saved text appears on the visit and in history. | Ready (OQ-008; PDR-12) |
| FR-DIA-001 | AC-1: Diagnosis notes can be entered and saved for a consultation. AC-2: Saved notes appear on the visit and in history. | Ready (OQ-008) |
| FR-RX-001 | AC-1: A medicine can be added with Name, Dosage, Frequency, Duration and Instructions. Only Name is required. AC-2: More than one medicine can be added to one prescription. AC-3: A prescription needs at least one medicine before it can be printed. | Ready (OQ-009; PDR-13) |
| FR-RX-002 | AC-1: A prescription can be generated as a printable A4 HTML view with a Print button. AC-2: The printed view contains the header (clinic name, address, phone, doctor name, registration number, placeholder logo), patient details, vitals, diagnosis, medicines and the footer signature. AC-3: One prescription per consultation. AC-4: The Print command marks the prescription printed. Edits are blocked after printing; reprinting is allowed. | Ready (OQ-010, OQ-011; PDR-14, 15, 16, 18) |
| FR-HIS-001 | AC-1: All previous visits for a patient are listed. | Ready (OQ-006, OQ-028) |
| FR-HIS-002 | AC-1: From a visit, vitals, complaints, diagnosis and prescriptions can each be viewed. | Ready (OQ-011) |
| FR-HIS-003 | AC-1: Visits can be filtered by a single date or a date range. AC-2: The filtered list contains only visits in that range. | Ready (OQ-012) |
| FR-SRC-001 | AC-1: Quick search is available from the top bar and returns matching patients (see FR-PAT-004). | Ready (OQ-003) |
| FR-SRC-002 | AC-1: The 5 most recently visited patients are shown, ordered by last visit. A visit is a consultation; patients with no consultation are not listed. | Ready (OQ-013; PDR-11) |
| FR-SRC-003 | AC-1: The user can move from a patient profile to that patient's visits and back. | Ready |
| FR-EXP-001 | AC-1: A single patient's data, with visits, vitals, diagnosis and prescriptions, can be exported to CSV, optionally limited to a date range. AC-2: The file opens in a spreadsheet tool with one row per visit. Dates are yyyy-MM-dd; columns are user-relevant only (DEC-020). | Ready (OQ-014; PDR-17) |
| FR-EXP-002 | AC-1: The same data can be shown as a printable HTML view with a Print button. AC-2: Browser Save as PDF produces a PDF containing the data. No server-side PDF library is used. | Ready (OQ-014; PDR-18) |
| NFR-USB-001 | No measurable criterion is set by the BRD. Qualitative check only: tab order, required markers, one-screen consultation. SC-006 is deferred. | Qualitative |
| NFR-PRF-001 | AC-1: Standard application pages load in approximately 2 seconds under localhost conditions (developer machine, local SQL Server, up to 1,000 patient records). Measured by browser timing, median of 5 runs (PDR-30). | Ready (OQ-020) |
| NFR-PRF-002 | AC-1: Patient search results are returned within 2–5 seconds under localhost conditions. | Ready (OQ-020) |
| NFR-REL-001 | Deferred (OOS-011). | Deferred |
| NFR-REL-002 | Deferred (OOS-011). | Deferred |
| NFR-SEC-001 | AC-1: The system cannot be accessed without logging in. AC-2: One user account is supported. AC-3: Passwords are 8 to 16 characters and include upper case, lower case, digit and symbol. AC-4: The session ends after 24 hours of inactivity (sliding). AC-5: Forgot password follows the BRD flow; the message is "If the email matches an account, the password has been reset." | Ready (OQ-015; PDR-02, 03, 05, 20) |
| NFR-SEC-002 | AC-1: Data is encrypted in transit beyond localhost (TLS 1.2+). AC-2: Encryption at rest is out of Phase 1 scope (DEC-017). AC-3: Passwords are stored hashed, never in plain text. | Ready (OQ-016; PDR-21 as amended by DEC-017). Localhost in-transit exception recorded in RSK-015 |
| NFR-SCL-001 | AC-1: The system supports up to 1,000 patient records. | Ready (OQ-023) |
| NFR-CMP-001 | AC-1: All functions work in the latest Chrome and Edge on desktop. The browser version is recorded for each test run. Safari is not required (DEC-019). | Ready (OQ-022; PDR-28; DEC-019) |

## 2. Success Criteria

| Ref | Acceptance Criteria | Status |
|---|---|---|
| SC-001 | Active. Measured from opening a consultation record to saving it, using test data on localhost. 2–3 minutes is a usability goal, not an SLA. | Ready (OQ-021) |
| SC-002 | Patient search and history retrieval complete in 2–5 seconds on localhost. | Ready (OQ-020) |
| SC-003 | Deferred / not measured (no real clinic baseline). | Deferred (OOS-014) |
| SC-004 | Prescriptions generate and print without error (printable A4 view). | Ready (OQ-010) |
| SC-005 | CSV and printable PDF (browser Save as PDF) exports complete and open without error. | Ready (OQ-014) |
| SC-006 | Deferred / not measured (no formal usability study). | Deferred (OOS-014) |
