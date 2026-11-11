# 17 — Planning Decisions and Open Items

**Purpose:** The BRD is the single source of truth. Where the planning design needs a detail the BRD does not state, the item is listed here as a decision for review. **No item below has been assumed or decided.** Recommendations are options for the reviewer, not adopted choices.
**Blocking:** `Yes` = the milestone shown cannot start until the item is decided. `No` = the design works either way and can be closed later.
**Owner:** Project Owner (STK-003) for BRD behaviour. Mentor / Lead (STK-006) for process. Developer for technology choices, subject to review.
**Status:** All 32 items **decided 2026-11-10** by the Project Owner (developer items included). The original options and recommendations are kept for the record. Amendments after review are in §6.

## 1. BRD Behaviour Gaps (Project Owner)

| PDR | Topic | Req IDs | What the BRD does not say | Options | Recommendation (for decision) | Blocking | Milestone |
|---|---|---|---|---|---|---|---|
| PDR-01 | Gender values | FR-PAT-001 | Gender is required but its allowed values are not listed | (a) Fixed list you supply; (b) free text | **DECIDED 2026-11-10 (Project Owner): Male, Female, Other** | Yes | M2 |
| PDR-02 | Password complexity and length | NFR-SEC-001 | "Complexity" is not defined. No maximum length. No reuse rule | (a) Define rules; (b) keep "8 characters with complexity" as written | **DECIDED 2026-11-10 (Project Owner): 8 to 16 characters; must include upper case, lower case, digit and symbol. No reuse rule stated.** | Yes | M1 |
| PDR-03 | Login and reset messages | NFR-SEC-001 | Wording when the reset email does not match, and session-expiry notice | Generic message or specific message | **DECIDED 2026-11-10 (Project Owner): "If the email matches an account, the password has been reset."** | No | M1 |
| PDR-05 | Creating the single user account | NFR-SEC-001, CON-002 | No sign-up or user management. How the one account is created is not stated | (a) Seed script with a one-time setup; (b) configuration file | **DECIDED 2026-11-10 (Project Owner): (a) seed script, run once. Username: bmunukuri. Email: supplied by Project Owner (not recorded in this document). Password set when the script runs.** | Yes | M1 |
| PDR-06 | Slot grid | FR-APT-001 | Start times and working hours are not stated. "30-minute slots" only | (a) Fixed day grid you supply; (b) any start time on the half hour | **DECIDED 2026-11-10 (Project Owner): 30-minute slots, 09:00–13:00 and 17:00–20:00. Last slot starts 12:30 and 19:30.** | Yes | M3 |
| PDR-07 | Does a cancelled or no-show slot free up? | FR-APT-001, FR-APT-003 | Overlap rule does not say whether Cancelled or No-show releases the slot | (a) Yes; (b) No | **DECIDED 2026-11-10 (Project Owner): (a) Yes. Cancelled and No-show appointments release the slot.** | Yes | M3 |
| PDR-08 | Status reversal | FR-APT-003 | "Can be changed back to an earlier status." Order of statuses is not defined (Cancelled and No-show have no order) | (a) Any status to any other; (b) defined transitions | **RESOLVED 2026-11-10 (Project Owner): lifecycle Scheduled → Checked-In → In Consultation → Completed; Scheduled → Cancelled; Scheduled → No-show; Checked-In → Cancelled. No reversal. See `19-Appointment-Status-Lifecycle.md`.** | Yes | M3 |
| PDR-09 | Archiving appointments | FR-APT-003 | BRD says appointments are archived. Trigger and view are not stated | (a) Manual "Archive" action; (b) automatic after a set period; (c) archive only via status change | **DECIDED 2026-11-10 (Project Owner): (a) manual Archive action. Archived appointments stay hidden everywhere; no screen shows them.** | Yes | M3 |
| PDR-10 | Duplicate-patient match rule | FR-PAT-001 | BRD: "same name, DOB or phone". Exact or similar match not stated | (a) Exact match, name case-insensitive; (b) fuzzy match | **DECIDED 2026-11-10 (Project Owner): (a) exact match; name case-insensitive.** | No | M2 |
| PDR-11 | Recent patients: what counts as a visit | FR-SRC-002 | "5 most recently visited, by last visit". Whether appointments count, and patients with no visit | (a) Consultations only; (b) appointments too | **DECIDED 2026-11-10 (Project Owner): (a) consultations only; patients with no consultation are not listed.** | Yes | M6 |
| PDR-12 | Complaints shape | FR-CMP-001 | "Enter patient symptoms (free text)". One text block or several entries? | (a) One text block per visit; (b) list of entries | **DECIDED 2026-11-10 (Project Owner): (a) one free-text block per visit.** | No | M4 |
| PDR-13 | Medicine line rules | FR-RX-001, FR-RX-002 | Which medicine fields are required. Whether a prescription may have no lines | Per field and per prescription | **DECIDED 2026-11-10 (Project Owner): (a) medicine Name required; Dosage, Frequency, Duration and Instructions optional; a prescription needs at least one medicine before printing.** | Yes | M5 |
| PDR-14 | Reprint and "printed" | FR-RX-002 | What counts as printed (print command or print preview). Can a printed prescription be printed again? | (a) Print command sets printed; reprint allowed; (b) reprint not allowed | **DECIDED 2026-11-10 (Project Owner): (a) the Print command sets the prescription as printed; reprinting is allowed; no edits after first print.** | Yes | M5 |
| PDR-15 | Clinic header data | FR-RX-002 | Header fields are required (clinic name, address, phone, doctor name, registration number, logo). Where they are entered is not stated | (a) Clinic profile screen (SCR-14); (b) configuration file | **DECIDED 2026-11-10 (Project Owner): (a) clinic profile screen (SCR-14); logo file stored on disk.** **AMENDED 2026-11-11 (D-M5-6, Project Owner approval of the Module 5 plan): the header fields may be left blank. Printing is allowed; a prescription with a blank header field prints with a notice that the clinic details are incomplete. Column lengths are still enforced.** **SUPERSEDED 2026-11-11 (D-M5-9, Project Owner instruction during the Module 5 browser checks): all five header fields are required on save, as the original rule says. A save with any field blank is refused. The print notice stays for a header that has never been saved.** | Yes | M5 |
| PDR-16 | Print method | FR-RX-002, SC-004 | "Printable on A4". Browser print or server-made PDF? | (a) Browser print with A4 CSS; (b) server PDF then print | **DECIDED 2026-11-10 (Project Owner): printable HTML view with browser print. Superseded: the earlier server-PDF answer is replaced by PDR-18 (no PDF library).** | Yes | M5 |
| PDR-17 | CSV layout | FR-EXP-001 | Column names, one file or several, date format | (a) One file, one row per visit with patient columns; (b) one file per section | **DECIDED 2026-11-10 (Project Owner): (a) one CSV file, one row per visit, patient details repeated on each row. Column names and date format still to be agreed with a sample.** | Yes | M7 |
| PDR-19 | Missing vitals confirmation | FR-VIT-001 | "Show a warning and do not block." Does the user confirm, or is a notice enough? | (a) Notice only; (b) confirm click like range warnings | **DECIDED 2026-11-10 (Project Owner): (a) notice only; the consultation saves without a confirm click.** | No | M4 |
| PDR-20 | Meaning of "24-hour idle timeout" | NFR-SEC-001 | Idle (sliding) or fixed 24 hours from sign-in? | (a) Sliding: resets on activity; (b) fixed from sign-in | **DECIDED 2026-11-10 (Project Owner): (a) sliding; the timer resets on activity and the user is signed out after 24 hours without activity.** | Yes | M1 |
| PDR-21 | Encryption mechanism and localhost HTTP | NFR-SEC-002 | "SQL Server standard storage encryption" is read as Transparent Data Encryption (TDE). Confirm. Plain HTTP on localhost is assumed allowed by "TLS only beyond localhost" | (a) TDE; (b) column encryption; plain HTTP on localhost vs HTTPS everywhere | **DECIDED 2026-11-10 (Project Owner): (a) TDE for the database; plain HTTP on localhost; HTTPS once the app runs beyond localhost.** | Yes | M1 |
| PDR-22 | Format rules for phone, email, DOB | FR-PAT-001 | BRD gives no formats. DOB not in the future is proposed | Formats per rule | **DECIDED 2026-11-10 (Project Owner): (a) standard email format; phone of 7 to 15 digits with optional spaces, hyphens and leading +; date of birth not in the future.** | Yes | M2 |
| PDR-23 | Search matching | FR-PAT-004, FR-SRC-001 | Contains or starts-with? Case? Minimum characters? | (a) Contains, case-insensitive, no minimum; (b) starts-with | **DECIDED 2026-11-10 (Project Owner): (a) contains match, case-insensitive, no minimum length.** | No | M2 |
| PDR-24 | Visit date | FR-HIS-001 | Set by the system at start, or can the physician enter a past date? | (a) System time only; (b) editable date | **DECIDED 2026-11-10 (Project Owner): (a) system sets the visit date and time at start; not editable.** | No | M4 |
| PDR-25 | Patient archive or delete | FR-PAT-002 | Archive is stated for appointments only. Confirm patients are never deleted or archived | Confirm | **DECIDED 2026-11-10 (Project Owner): (a) patients cannot be deleted or archived in Phase 1.** | No | M2 |
| PDR-26 | Consultations per appointment | FR-APT-001, FR-HIS-001 | Can one appointment produce more than one consultation? | (a) One; (b) many | **DECIDED 2026-11-10 (Project Owner): (a) one consultation per appointment.** | No | M4 |
| PDR-27 | Completed status and consultation | FR-APT-003 | Must a "Completed" appointment have a linked consultation? | (a) Yes; (b) No | **RESOLVED 2026-11-10 (Project Owner): Completed requires a consultation. An appointment reaches Completed only after In Consultation. It cannot be marked Completed from Scheduled or Checked-In. Supersedes the earlier answer (b).** | No | M3 |

## 2. Technology and Process Items (Developer, reviewed by Mentor / Lead)

| PDR | Topic | Req IDs | What is open | Options | Recommendation (for decision) | Blocking | Milestone |
|---|---|---|---|---|---|---|---|
| PDR-04 | Authentication library | NFR-SEC-001 | ASP.NET Core Identity framework, or the hasher only with custom user table | (a) Identity framework; (b) hasher only | **DECIDED 2026-11-10 (Project Owner): (b) hasher only, custom Users table** | No | M1 |
| PDR-18 | PDF generation | FR-EXP-002, FR-RX-002 | No library chosen | (a) PDF library; (b) no library: printable HTML view with browser print | **DECIDED 2026-11-10 (Project Owner): (b). No PDF library and no server-side PDF generation. The system generates printable HTML documents. A Print button calls window.print(). The browser's Save as PDF produces the PDF. No third-party dependency is added for PDF.** | Yes | M5, M7 |
| PDR-28 | Browser versions for the test matrix | NFR-CMP-001 | "Latest" is not a version. Pin per test run | Record versions on each run | **DECIDED 2026-11-10 (Project Owner): (a) record the browser version used on each test run in the test report.** | No | M8 |
| PDR-29 | Test tools | All | Unit test framework, mocking library, browser automation | Standard .NET test stack | **DECIDED 2026-11-10 (Project Owner): (a) xUnit and NSubstitute for unit and integration tests; browser testing is manual, using the acceptance script. No browser automation in Phase 1.** | No | M1 |
| PDR-30 | Performance measurement method | NFR-PRF-001, NFR-PRF-002, SC-002 | "Approximately 2 seconds" needs a method: tool, runs, median | Browser timing; scripted timing | **DECIDED 2026-11-10 (Project Owner): (a) browser timing, median of 5 runs.** | No | M8 |
| PDR-31 | Target versions | All | .NET version and SQL Server version are not stated | Latest supported versions | **DECIDED 2026-11-10 (Project Owner): (a) latest LTS .NET at M1 start; locally installed SQL Server version; both recorded in the repository README.** | Yes | M1 |
| PDR-32 | Dapper vs EF Core data access | All data IDs | Instruction of 2026-11-10 says to retrieve SQL Server data for documents "via Dapper". The approved architecture uses EF Core repositories (AD-04). Scope of Dapper not stated | (a) Dapper for document read queries only (prescription and export views); EF Core for everything else; (b) Dapper for all data access, replacing EF Core; (c) EF Core for everything, document views included | **DECIDED 2026-11-10 (Project Owner): (c) EF Core for all data access, including document views. Dapper is not used.** | Yes | M1 |

## 3. Summary

| Item | Count |
|---|---|
| Total PDR items | 32, all decided 2026-11-10 |
| Blocking = Yes | 18 (PDR-01, 02, 05, 06, 07, 08, 09, 11, 13, 14, 15, 16, 17, 18, 20, 21, 22, 31) |
| Blocking = No | 13 |
| Project Owner items | 25 |
| Developer items | 6 |

Of the blocking items, PDR-18 and PDR-31 are developer items. PDR-04 is not blocking because either option meets the BRD.

IDs PDR-01 to PDR-32 are all in use, with no gaps. Each ID is referenced from the design files listed in §4.

## 4. Effect on Design

- No screen, entity or service changes until the blocking items are decided.
- Where an option changes the design, the affected file is listed in the table below.

| PDR | Affects |
|---|---|
| PDR-01 | 06-Entity-Design (Patient.Gender), 07-Database-Design (CHECK) |
| PDR-02 | 12-Authentication-Design §5, 13-Validation-Strategy §3.7 |
| PDR-05 | 07-Database-Design §8, 11-Repository-Design §2.1 |
| PDR-06, PDR-07 | 06-Entity-Design §2.3, 10-Service-Design §2.3, 13-Validation §3.3 |
| PDR-08, PDR-09 | 10-Service-Design §2.3, 05-Screen-Catalog SCR-10 |
| PDR-11 | 11-Repository-Design §3, 14-Testing-Strategy (TC-FR-SRC-002) |
| PDR-13 | 07-Database-Design §2 (PrescriptionItems), 13-Validation §3.5 |
| PDR-14, PDR-16 | 09-Controller-Design rows 23–24, 10-Service-Design §2.6 |
| PDR-15 | 06-Entity-Design §2.8, 05-Screen-Catalog SCR-14 |
| PDR-17, PDR-18 | 10-Service-Design §2.7, 07-Database-Design (export only) |
| PDR-20, PDR-21 | 12-Authentication-Design §4, 14-Testing-Strategy §3 |
| PDR-22 | 13-Validation-Strategy §3.1 |
| PDR-31 | 01-Solution-Architecture §2 |

## 5. Items Found in Module 1 Artifacts (not planning decisions)

These are inconsistencies with the amended BRD in artifacts from Module 1. They are corrected in WP-1.2 (`15-Development-Roadmap.md`), not in this folder.

| Artifact | Issue |
|---|---|
| `09-Constraints-Register.md` CON-001 | Refers to OOS-001 to OOS-010. Should be OOS-001 to OOS-015 |
| `09-Constraints-Register.md` CON-014 | States "No data loss. Regular automated backups" as a constraint. Deferred (OOS-011) |
| `05-Functional-Requirements.md` FR-VIT-001 note | Says validation ranges are to be supplied by a clinician. Ranges are now in the BRD |
| `06-Non-Functional-Requirements.md` NFR-SCL-001 | Says "moderate" is not defined. BRD now gives up to 1,000 records |
| `06-Non-Functional-Requirements.md` NFR-PRF-001 | Measurable as written is "Yes" but conditions not defined. Conditions now in BRD |
| `12-Requirement-Traceability-Matrix.md` | Many statuses still "Blocked" although the linked open questions are answered. FR-VIT-001 is Ready in `10-Acceptance-Criteria.md` but Blocked in the matrix |
| `00-Review-Summary.md` | Says "28 (all open)" for open questions. All are now answered, deferred or excluded |
| `01-BRD-Summary.md` §5 | Lists only OOS-001 to OOS-010 |
| `11-Requirement-Catalog.md` | Priority note updated: approved by the Project Owner 2026-11-10 |
| PDR-27 (Completed) | Confirmed 2026-11-10: Completed requires In Consultation first (`19-` §4) |

## 6. Amendments and Decisions After Review (2026-11-10)

Each row amends the earlier answer for the item shown. The earlier answers stay in §1 for the record. Decision IDs are in `Documentation/01-Brainstorming/14-Decision-and-Issue-Log.md`.

| Item | Amends | Decision | Where applied |
|---|---|---|---|
| B1 | PDR-05 | Single account created by a console seed command. Password typed at the prompt; never stored; no sign-in fallback to a single account (DEC-018) | `12-` §8; `11-` §2.1; BRD |
| B2 | PDR-21 (at rest); NFR-SEC-002 | Encryption at rest is out of Phase 1 scope. No Developer Edition install for TDE. Passwords hashed. TLS beyond localhost unchanged (DEC-017) | BRD NFR-SEC-002; `07-` §7; CLAUDE.md §9 |
| Env | PDR-31 (environment) | SQL Server Express is the local edition. Committed configuration holds a neutral connection string; the local instance goes in user secrets (R-6) | `Documentation/04-Implementation/01-Setup-Guide.md` §1, §4 |
| B3 | PDR-15 | Placeholder application logo in the print view. No logo upload and no logo column (DEC-021) | `06-` §2.8; `07-` §2; `09-` note |
| B4 | PDR-17 | CSV dates in ISO format (yyyy-MM-dd). User-relevant columns only. Column set in `10-` §2.7 (DEC-020) | `10-` §2.7; BRD |
| B5 | PDR-28; NFR-CMP-001 | Chromium-based browsers (Chrome, Edge) are the primary target. No Safari-specific support in Phase 1 (DEC-019) | BRD; `14-` §3; CLAUDE.md §10 |
| B6 | PDR-08, PDR-09, PDR-27 | Archive action and Finish action are both in Phase 1. Completed = business process completed (Finish, after In Consultation). Archived = hidden from active lists, kept for history (DEC-022) | `19-` §4; `09-` rows 30 and 31; `10-` §2.4 |
| C1 | PDR-26 | One consultation per appointment in all design files (0..1 cardinality; unique filtered index) | `06-` §3; `07-` §4; `08-` |
| C2 | Implementation order | Approved 10-item order (DEC-023) | `15-` §1A; `04-Implementation/02-` §13 |
| C3 | Release trigger | Release only after all planned features are implemented, tested and validated; the roadmap is the criterion (DEC-023) | `15-` §4; `03-Worktree/05-` §2 |
| C7 | RTM | SC-006 reference removed from NFR-USB-001 | `12-RTM` (Module 1) |
| D1 | Design rule | A printed prescription freezes the consultation fields it shows (vitals, complaints, diagnosis). Derived from FR-RX-002 AC-4 | `10-` §2.4; `13-` §3.4 |
| D2 | Design rule | Systolic and diastolic are both entered or both empty; otherwise an error | `13-` §3.4 |
| D3 | Design rule | A from-date after the to-date is an error | `13-` §3.6 |
| D4 | Design rule | An empty search term is an error; any non-empty term is searched | `13-` §3.2 |
| D5 | Design rule | Home page (SCR-03) shows today's list, quick search and recent patients | `05-` SCR-03 |
| D6 | Design rule | Database-level no-overlap guard (filtered unique index on slot start) | `07-` §4 |

Note on approval: the approval reply labelled D1 to D6 as the governance model. The governance model is already in CLAUDE.md §5 and is unchanged. D1 to D6 are recorded above with the recommended defaults under DEC-024. Any of them can be reversed with a new amendment.
