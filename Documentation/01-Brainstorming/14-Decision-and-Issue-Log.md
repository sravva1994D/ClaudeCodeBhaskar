# 14 — Decision and Issue Log

**Project:** Patient Management System — Phase 1, Module 1
**Maintained until:** every issue is Resolved and every decision is Closed.
**Routing date:** 2026-11-10
**Note:** "Routed" means the owner is assigned in this log. Questions must be delivered to owners by the requester. No message has been sent to anyone.

---

## 1. Decisions

| DEC ID | Decision | Date | Decided By | Status | Affects | Source |
|---|---|---|---|---|---|---|
| DEC-001 | BRD (`BRD/Doc_BRD.md`) is the approved baseline (A-001) | 2026-11-10 | Requester | Accepted by requester (sole decision-maker, DEC-010) | All | 07 §1 |
| DEC-002 | This work is Module 1 (Brainstorming) of Phase 1; all artifacts align to that scope (A-002) | 2026-11-10 | Requester | Accepted by requester (sole decision-maker, DEC-010) | All | 07 §1 |
| DEC-003 | Requirement IDs follow the proposed ID scheme and are applied consistently across all artifacts (A-003) | 2026-11-10 | Requester | Accepted by requester (sole decision-maker, DEC-010) | All IDs | 07 §1 |
| DEC-004 | OQ-025 (audit trail) was first kept open pending confirmation. Resolved: excluded from Phase 1 (option 2), revisit later (OOS-013). | 2026-11-10 | Requester | Closed | OQ-025, RSK-010 | 13 |
| DEC-005 | OQ-017 (data retention and consent) was first kept open pending confirmation. Superseded by DEC-012: deferred / out of scope (OOS-015). | 2026-11-10 | Requester | Superseded by DEC-012 | OQ-017, NFR-SEC-002 | 13 |
| DEC-006 | MoSCoW priorities are added to the BRD as a proposal (A-006) | 2026-11-10 | Requester (framework); analyst (ratings) | Approved 2026-11-10 by the Project Owner | All requirements | BRD "Requirement Priorities" |
| DEC-007 | Open questions are assigned to proposed owners (ISS-001 to ISS-028) and kept in this log until resolved | 2026-11-10 | Requester | Closed: all 28 answered 2026-11-10 | OQ-001 to OQ-028 | §2 |
| DEC-008 | No Git commit, branch, or PR until the pre-commit decision summary is approved | 2026-11-10 | Requester (per project process) | Superseded 2026-11-10: `development` is the only working branch; commits need approval first (D-1; single-branch instruction) | Repository | `15-Pre-Commit-Decision-Summary.md` |
| DEC-009 | Forgot-password flow kept as: email, new password and confirm password; if the email matches, the password is updated and the user is logged in. No second verification. Risk accepted by stakeholder. Residual risk: anyone who knows the account email can take over the account. | 2026-11-10 | Requester (stakeholder decision) | Accepted (risk accepted) | NFR-SEC-001, RSK-003, OQ-015 | 13 OQ-015 |
| DEC-011 | BRD amended to match localhost-only decisions: backups and recovery deferred (OOS-011); additional vitals out of Phase 1 (OOS-012); audit trail out of Phase 1 with no audit-logging dependency for omitted vitals (OOS-013); SC-003 and SC-006 deferred / not measured (OOS-014); NFR-PRF-001 approximately 2 s under localhost conditions; NFR-PRF-002 search 2–5 s under localhost conditions. Reason: single-user, localhost-only learning project. | 2026-11-10 | Requester (stakeholder decision) | Accepted | BRD; 03, 05, 06, 10, 11, 12 | BRD Decision Log |
| DEC-012 | Data retention, deletion and consent (OQ-017) deferred / out of scope for Phase 1 (OOS-015). Reason: localhost-only learning project with no real patient data. Revisit if deployed beyond a local environment. | 2026-11-10 | Requester (stakeholder decision) | Accepted | NFR-SEC-002, FR-PAT-002, BRD OOS-015 | 13 OQ-017 |
| DEC-010 | Governance simplified: Clinic Owner = General Physician = Product Owner = Project Owner. Single decision-maker; no escalation process. | 2026-11-10 | Requester | Accepted | All approvals and routing | 13 OQ-026; 04 Governance Update |
| DEC-013 | Planning decisions PDR-01 to PDR-32 recorded: Gender values; password 8–16 characters with complexity; reset message; hasher only; seed-script account; slot grid; cancelled and no-show release slot; status moves; manual archive; exact duplicate match; recent patients from consultations; one complaints block; medicine rules; print sets printed; clinic profile screen; CSV layout; missing-vitals notice; sliding timeout; TDE and localhost HTTP; format rules; search rules; system visit date; no patient delete; one consultation per appointment; Completed without consultation; browser version per run; xUnit and NSubstitute; browser timing; latest LTS .NET. Detail: `Documentation/02-Planning/17-Planning-Decisions-and-Open-Items.md` | 2026-11-10 | Project Owner (sole decision-maker) | Closed | Module 2 and BRD | `02-Planning/17-` |
| DEC-014 | BRD amended for the decided wording and for printable HTML outputs (PDF via browser Save as PDF; no PDF library); data access is EF Core only (no Dapper) | 2026-11-10 | Project Owner | Accepted | BRD; 05, 06, 07, 09, 10, 11, 12 | BRD Decision Log |
| DEC-015 | Appointment lifecycle resolved (PDR-08): Scheduled → Checked-In → In Consultation → Completed; Scheduled → Cancelled or No-show; Checked-In → Cancelled; no reversal; Completed only after In Consultation (PDR-27 confirmed). MoSCoW priorities approved as the baseline (A-006) | 2026-11-10 | Project Owner (sole approver) | Accepted | FR-APT-003; all priorities; `Documentation/02-Planning/19-Appointment-Status-Lifecycle.md` | `02-Planning/17-`; 07 A-006 |
| DEC-016 | Governance decisions: D-4 track `Reference/` training plan (.docx) in Git; D-7 first version `v0.1.0`; D-8 no formal review SLA; D-9 no detached worktrees; D-10 urgent fixes directly on `development`, no hotfix branch; D-11 one release PR `development` → `main`, merge commit, once the application is complete and tested. D-6 no branch protection | 2026-11-10 | Project Owner (sole decision-maker) | Accepted | Git, release, Module 3 and Module 7 | `Documentation/03-Worktree/00-Worktree-Review-Package.md` |

## 2. Issues (Open Questions Routed to Owners)

Each issue maps one-to-one to an open question in `13-Open-Questions.md`.

| ISS ID | OQ | Question (short) | Owner (Proposed) | Routed | Status | Blocks |
|---|---|---|---|---|---|---|
| ISS-001 | OQ-001 | Contact detail fields | STK-002 Clinic Owner | 2026-11-10 | Answered: phone, email, address (required/optional still open under OQ-002) | FR-PAT-001, FR-PAT-004 |
| ISS-002 | OQ-002 | Mandatory patient fields; Age vs DOB | STK-002 Clinic Owner | 2026-11-10 | Answered: Name, DOB, Gender, phone, email, address all required (confirmed 2026-11-10); Age calculated from DOB and system date. | FR-PAT-001 |
| ISS-003 | OQ-003 | Duplicate patients; unique identifier | STK-001 General Physician | 2026-11-10 | Answered: unique patient identifier required. Format and block-vs-warn rule still to confirm. | FR-PAT-001, FR-PAT-004, FR-SRC-001 |
| ISS-004 | OQ-004 | Appointment slot, duration, overlap rules | STK-001 General Physician | 2026-11-10 | Answered: 30-minute slots; no overlaps. Duration = slot length (to confirm). | FR-APT-001, FR-APT-002 |
| ISS-005 | OQ-005 | Registration before appointment; walk-ins | STK-001 General Physician | 2026-11-10 | Answered: registration required before scheduling; walk-ins registered on arrival. | FR-APT-001 |
| ISS-006 | OQ-006 | Consultation created only from appointment? (core entity) | STK-001 General Physician | 2026-11-10 | Answered: consultation can exist without an appointment; walk-in/unscheduled consultations can be started directly. | FR-APT-003, FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-HIS-001, FR-SRC-003 |
| ISS-007 | OQ-007 | Vitals units, ranges, mandatory enforcement | STK-001 General Physician | 2026-11-10 | Answered: units; warn-and-allow for missing vitals; validation ranges (Temp 34.0–42.0 °C; SBP 70–250; DBP 40–150; pulse 30–220; warn and require confirmation, no block). Extra vitals and audit recording out of Phase 1. | FR-VIT-001 |
| ISS-008 | OQ-008 | Complaints/diagnosis: free text or coded | STK-001 General Physician | 2026-11-10 | Answered: free text only; no coded terms. | FR-CMP-001, FR-DIA-001 |
| ISS-009 | OQ-009 | Medicine entry format; dosage/frequency format | STK-001 General Physician | 2026-11-10 | Answered: medicine name, dosage and frequency are free text. | FR-RX-001, FR-RX-002 |
| ISS-010 | OQ-010 | Prescription header/footer content; paper size | STK-002 Clinic Owner | 2026-11-10 | Answered: header = clinic name, address, phone, doctor name, registration number, logo; footer = signature; A4. | FR-RX-002, SC-004 |
| ISS-011 | OQ-011 | Multiple or editable prescriptions | STK-001 General Physician | 2026-11-10 | Answered: one prescription per consultation; not editable after printing. | FR-RX-002, FR-HIS-002 |
| ISS-012 | OQ-012 | Date filter form | STK-001 General Physician | 2026-11-10 | Answered: single date and date range. | FR-HIS-003 |
| ISS-013 | OQ-013 | Recent patients: count and definition | STK-001 General Physician | 2026-11-10 | Answered: 5 patients; ordered by last visited. | FR-SRC-002 |
| ISS-014 | OQ-014 | Export content and scope | STK-002 Clinic Owner | 2026-11-10 | Answered: patient details + visits, vitals, diagnosis, prescriptions; single patient per export, optionally limited to a date range (corrected 2026-11-10). | FR-EXP-001, FR-EXP-002, SC-005 |
| ISS-015 | OQ-015 | Login method, password rules, session timeout | STK-002 Clinic Owner | 2026-11-10 | Answered: username + password; minimum 8 characters with complexity; 24-hour idle timeout; email-only forgot-password flow accepted as risk (DEC-009). | NFR-SEC-001 |
| ISS-016 | OQ-016 | Encryption standard | STK-003 Product Owner | 2026-11-10 | Partly answered 2026-11-10: BRD NFR-SEC-002 kept as written (option 2); localhost note recorded as context; password hashing mandatory. At-rest: SQL Server standard storage encryption, approach chosen by the team. TLS: required only beyond localhost (TLS 1.2+); not required for localhost build (leaves BRD NFR-SEC-002 in-transit unmet on localhost). Answered. | NFR-SEC-002 |
| ISS-017 | OQ-017 | Regulation, retention, deletion, consent | STK-003 Product Owner | 2026-11-10 | Closed as Deferred / Out of Scope (OOS-015), 2026-11-10 (DEC-012). | NFR-SEC-002, FR-PAT-002 |
| ISS-018 | OQ-018 | Backup frequency, retention, restore | STK-003 Product Owner | 2026-11-10 | Deferred for localhost-only learning project; BRD amended 2026-11-10 (OOS-011). | NFR-REL-002 |
| ISS-019 | OQ-019 | Acceptable data-loss window; recovery time | STK-003 Product Owner | 2026-11-10 | Deferred for localhost-only learning project (option 1); BRD amended 2026-11-10 (OOS-011). | NFR-REL-001 |
| ISS-020 | OQ-020 | Performance measurement method; "fast" | STK-003 Product Owner | 2026-11-10 | Answered: localhost conditions, 1,000-record sample, guidelines not SLAs; search 2–5 s; page load ~2 s. BRD wording updated 2026-11-10. | NFR-PRF-001, NFR-PRF-002, SC-002 |
| ISS-021 | OQ-021 | Success-criteria baselines and measures | STK-002 Clinic Owner | 2026-11-10 | Answered: SC-001 measured on localhost test data; SC-003 and SC-006 deferred/out of scope. BRD success criteria updated 2026-11-10 (SC-003 and SC-006 deferred). | SC-001, SC-003, SC-006, NFR-USB-001 |
| ISS-022 | OQ-022 | Minimum browser versions; mobile browsers | STK-003 Product Owner | 2026-11-10 | Answered: latest Chrome, Edge, Safari; desktop only. | NFR-CMP-001 |
| ISS-023 | OQ-023 | Expected patient and visit volume | STK-002 Clinic Owner | 2026-11-10 | Answered: up to 1,000 patient records (sample dataset). | NFR-SCL-001 |
| ISS-024 | OQ-024 | Delete, archive, or reverse status | STK-002 Clinic Owner | 2026-11-10 | Answered: archive (hidden but kept), no hard delete; status can be reversed. Superseded 2026-11-10 (DEC-015): no reversal. | FR-PAT-002, FR-APT-003 |
| ISS-025 | OQ-025 | Audit trail or edit history required? | STK-003 Product Owner | 2026-11-10 | Answered: excluded from Phase 1 (option 2); revisit later (DEC-004). | BO-003, RSK-010 |
| ISS-026 | OQ-026 | Stakeholder roles; decision owner | STK-003 Product Owner | 2026-11-10 | Answered: all roles are one person (Clinic Owner = General Physician = Product Owner = Project Owner); single decision-maker. | All routing |
| ISS-027 | OQ-027 | Update BRD open-questions section | STK-003 Product Owner | 2026-11-10 | Answered: BRD Open Questions section updated with decisions, deferred and still-open items. | BRD accuracy |
| ISS-028 | OQ-028 | "Consultation" and "visit" same entity? | STK-001 General Physician | 2026-11-10 | Answered: same record. | FR-VIT-001, FR-HIS-001, FR-RX-002, FR-EXP-001/002 |

**Totals:** 28 issues; 0 open; all answered, deferred or excluded (2026-11-10). Owner split: STK-001: 11 · STK-002: 8 · STK-003: 9.

## 3. Risks Raised by Unresolved Items (Cross-Reference)

See `08-Risk-Register.md`. Highest-rated risks depend on these issues: RSK-001 (ISS-006, ISS-028), RSK-002 (ISS-020, ISS-021), RSK-003 (ISS-015, ISS-016, ISS-017), RSK-004 (ISS-018, ISS-019), RSK-006 (ISS-010, ISS-011), RSK-008 (ISS-027).

## 4. Action Log

| Date | Action | By | Result |
|---|---|---|---|
| 2026-11-10 | Module 1 artifacts produced (00–13) | Analyst | Awaiting review |
| 2026-11-10 | Assumptions A-001 to A-003 accepted by requester | Requester | Recorded (DEC-001 to DEC-003) |
| 2026-11-10 | MoSCoW priorities proposed in BRD | Analyst | Approved by Project Owner 2026-11-10 (DEC-006) |
| 2026-11-10 | Open questions assigned to owners in this log | Analyst | All 28 answered 2026-11-10 (DEC-007) |
| 2026-11-10 | Planning decisions PDR-01 to PDR-32 recorded | Project Owner | Closed (DEC-013) |
| 2026-11-10 | BRD amended for decided wording | Analyst, approved by Project Owner | Done (DEC-014) |
| 2026-11-10 | Module 1 artifacts refreshed | Analyst | Done |
