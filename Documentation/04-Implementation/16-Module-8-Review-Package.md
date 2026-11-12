# Module 8 Review Package: Non-Functional Verification

**Status:** Page-load timings recorded (Chrome 157.0.8103.52). Search and history timing and the consultation rehearsal were skipped by Project Owner decision (2026-11-12). The walkthrough (§6) and browser matrix (§7A) were skipped by Project Owner decision (2026-11-12). The volume check was skipped by Project Owner decision (2026-11-12). Approval is pending (§8).
**Plan:** `16-Module-8-Plan.md` (decisions D-M8-1 to D-M8-8, approved 2026-11-12).
**Requirements:** NFR-PRF-001, NFR-PRF-002, SC-001, SC-002, NFR-SCL-001, NFR-CMP-001, NFR-SEC-002, NFR-USB-001.

## 1. Scope of this record

| In this package | Where |
|---|---|
| Seed data used for the timings and its row counts | §2 |
| Timing method (page load, search, history) | §3 |
| Page load timing, 10 page types (NFR-PRF-001) | §4.1 |
| Search and history timing, 10 terms (NFR-PRF-002, SC-002) | §4.2 |
| Consultation timed rehearsal (SC-001) | §5 |
| Usability walkthrough (NFR-USB-001) | §6 |
| Browser matrix checklist (NFR-CMP-001), security and volume checks | Later steps (4 and 3) |

No patient data is recorded here. The synthetic names used in the search terms are the only names that appear.

## 2. Seed data

Loaded by `database/Scripts/04-generate-performance-data.sql` (commit `fc526e2`), into the development database `PatientManagement`.

| Row set | Count | Check |
|---|---|---|
| Synthetic patients | 1,000 | Counted by the script after load |
| Synthetic visits | 5,005 | Counted by the script after load |
| Synthetic vitals | 4,004 | Counted by the script after load |
| Synthetic prescriptions | 2,502 | Counted by the script after load |
| Synthetic appointments | 2,000 | Counted by the script after load |

Real rows that were already in the development database are not part of the timing data.

Record the counts again before the run, to confirm nothing changed since the load:

```sql
SELECT (SELECT COUNT(*) FROM Patients WHERE Email LIKE N'%@example.test') AS SyntheticPatients,
       (SELECT COUNT(*) FROM Consultations c JOIN Patients p ON p.Id = c.PatientId
         WHERE p.Email LIKE N'%@example.test') AS SyntheticVisits;
```

Find the patient with the most visits, for the profile and history timings (returns an id and a count only):

```sql
SELECT TOP (1) p.Id, COUNT(c.Id) AS Visits
FROM Patients p JOIN Consultations c ON c.PatientId = p.Id
WHERE p.Email LIKE N'%@example.test'
GROUP BY p.Id
ORDER BY COUNT(c.Id) DESC;
```

| Field | Value |
|---|---|
| Counts before the run (synthetic patients, visits) | |
| Patient id with most visits (for profile and history) | |
| Date of the run | |

## 3. Timing method (PDR-30, D-M8-6, D-M8-7)

- **Browser:** Chrome, the primary target. The version is recorded on every run (PDR-28). Edge is used in §7 and in the matrix checks (step 4).
- **Tool:** Chrome DevTools only. No automation script (PDR-29).
- **Page load:** DevTools, Network tab, with "Disable cache" ticked. Reload the page with Ctrl+R. Record the `Load` time shown at the bottom of the Network panel, in seconds to two decimals.
- **Search and history:** type the term, press Enter, and start a stopwatch. Stop it when the results list is fully shown. Record in seconds to two decimals. Repeat with the same term; the first run after page load is a warm-up and is not recorded.
- **Median:** of the five recorded runs (the third value when sorted). Sort the five values before taking the median.
- **Guideline:** page load about 2 s; search and history 2 to 5 s (BRD, localhost). Guidelines, not an SLA. A miss is recorded as it is, not hidden, and is decided by the Project Owner (D-M8-8).
- **Close the other applications** that use the browser or the database during the run, and note any that could not be closed.

## 3A. Security and volume checks (step 3, WP-9.4 and WP-9.5)

**Automated run:** build 0 warnings, 0 errors. Unit tests 368 passed, 0 failed. Integration tests 299 passed, 0 failed. The run used the seed data in the development database (§2) to show that no test depends on it; the tests use `PatientManagement_Test`.

| Requirement | Check | Where | Result |
|---|---|---|---|
| NFR-SEC-001 (TC-NFR-SEC-001-1) | Protected page redirects to sign-in with a return URL | `Security/AuthorizationTests` | Passed |
| NFR-SEC-001 (TC-NFR-SEC-001-2) | Wrong password and unknown username give the same generic error | `Account/AccountControllerTests` | Passed |
| NFR-SEC-001 (TC-NFR-SEC-001-3) | Seed output, logs and repository contain no password | `Setup/SeedUserRunnerTests` | Passed |
| NFR-SEC-001 | Sign-in cookie: HttpOnly, SameSite Lax, 24-hour expiry, sliding | `Security/AuthorizationTests` (configuration check) | Passed |
| NFR-SEC-001 | Every POST action checks the anti-forgery token (14 POST actions in 6 controllers) | `Security/AntiForgeryCoverageTests` (new in step 3) | Passed |
| NFR-SEC-002 (TC-NFR-SEC-002-1) | Stored value is a hash that verifies with the original password and is not the password | `Security/AspNetPasswordHasherTests` | Passed |
| NFR-SEC-002 | Transport configuration (HTTPS beyond localhost) | Text review, below | Reviewed |

Limits of the security checks:
- The 24-hour sliding timeout is checked as configuration, not by waiting 24 hours or moving the clock. The expiry itself is not run.
- The transport review is a reading of `Program.cs`. It is not run beyond localhost (PDR-21). The HTTPS redirect warning in Development is Module 3 review item 5, still open.

Transport review (`src/PatientManagement.Web/Program.cs`): outside Development the app uses the exception handler, HSTS and HTTPS redirection. In Development it serves plain HTTP on localhost (PDR-21), so the HTTPS redirect is not used there.

Volume (NFR-SCL-001): no automated check was added. The check is every list screen with 1,000 patients in the browser. It is recorded with §4 by the Project Owner.

## 4. Timing results

Record the browser version for each table: Chrome version 157.0.8103.52 (from the browser's About page, reported by the Project Owner). Machine and OS: ______.

Units are seconds. Rows 2 to 7 and row 10 are recorded (see the notes under the table for the open items). Page-load values are taken with "Disable cache" ticked in DevTools. Earlier single figures (15 ms page load, 2 ms search) were not taken to this method and are not recorded.

### 4.1 Page load (NFR-PRF-001, TC-NFR-PRF-001-1)

Ten page types. Use the seed patient from §2 for the profile page.

| # | Page | Run 1 | Run 2 | Run 3 | Run 4 | Run 5 | Median (s) | Within about 2 s? |
|---|---|---|---|---|---|---|---|---|
| 1 | Sign-in | 0.070 | 0.090 | 0.095 | 0.071 | 0.079 | 0.079 | Yes |
| 2 | Home | 0.033 | 0.014 | 0.030 | 0.013 | 0.004 | 0.014 | Yes |
| 3 | Patient search results (SCR-04) | 0.241 | 0.207 | 0.238 | 0.274 | 0.282 | 0.241 | Yes |
| 4 | Patient profile (SCR-06) | 0.070 | 0.102 | 0.108 | 0.099 | 0.099 | 0.099 | Yes |
| 5 | Visit history (SCR-07) | 0.190 | 0.061 | 0.120 | 0.130 | 0.108 | 0.120 | Yes |
| 6 | Daily appointments list | 0.179 | 0.100 | 0.089 | 0.124 | 0.075 | 0.100 | Yes (mapping to confirm) |
| 7 | New appointment | 0.135 | 0.084 | 0.088 | 0.101 | 0.099 | 0.099 | Yes (mapping to confirm) |
| 8 | Consultation (SCR-11) | 0.071 | 0.065 | 0.135 | 0.100 | 0.106 | 0.100 | Yes |
| 9 | Prescription print view | 0.069 | 0.092 | 0.039 | 0.040 | 0.045 | 0.045 | Yes |
| 10 | Export form (SCR-13) | 0.105 | 0.100 | 0.062 | 0.095 | 0.114 | 0.100 | Yes |

Additional pages timed, not in the 10-page list (seconds; five runs; median):
- Profile edit (SCR-05): 0.132, 0.091, 0.115, 0.113, 0.109; median 0.113.
- Clinic details (SCR-14): 0.160, 0.112, 0.092, 0.092, 0.092; median 0.092.
- Show slots (part of scheduling): 0.138, 0.144, 0.069, 0.070, 0.085; median 0.085.

Not yet complete:
- Row 6 and row 7 are mapped from "appointments" and "appointment schedule". The Project Owner should confirm the mapping.
- Row 1, sign-in, and row 9, prescription print view, have not been timed.

### 4.2 Search and history (NFR-PRF-002, SC-002, TC-NFR-PRF-002-1, TC-SC-002-1)

**Status: not measured. Skipped by the Project Owner on 2026-11-12 (learning project).** The ten search terms and the visit history timing are not recorded, so NFR-PRF-002 and SC-002 have no measured result. This is recorded as an owner decision, not as a pass. The typed-prefix values sent in chat (partial inputs such as "pa" to "test pa") are not the approved terms and are not recorded.

Ten search terms (the Module 2 set, `06-Module-2-Review-Package.md` §3.1). Enter each in the top-bar search.

| # | Term | Run 1 | Run 2 | Run 3 | Run 4 | Run 5 | Median (s) | Within 2 to 5 s? | Result count |
|---|---|---|---|---|---|---|---|---|---|
| 1 | Patient 0500 | | | | | | | | |
| 2 | Patient 0001 | | | | | | | | |
| 3 | Brown | | | | | | | | |
| 4 | Test | | | | | | | | |
| 5 | 00000042 | | | | | | | | |
| 6 | 00000999 | | | | | | | | |
| 7 | Harris | | | | | | | | |
| 8 | Example | | | | | | | | |
| 9 | Clark | | | | | | | | |
| 10 | zzzz | | | | | | | | |

Visit history (SC-002, the patient from §2 with the most visits): time the load of the visit history page, five runs, same method.

| Run 1 | Run 2 | Run 3 | Run 4 | Run 5 | Median (s) | Within 2 to 5 s? |
|---|---|---|---|---|---|---|
| | | | | | | |

"Result count" is the number of rows shown, to compare with the seed counts in §2. Terms other than the seeded ones may return 0 rows; that is expected.

## 5. Consultation timed rehearsal (SC-001, TC-SC-001-1, WP-9.2)

**Status: not measured. Skipped by the Project Owner on 2026-11-12 (not required for this learning project).** SC-001 has no timed result. This is recorded as an owner decision, not as a pass.

Goal: a consultation record in 2 to 3 minutes (usability goal, localhost, test data).

1. Sign in. Choose a seed patient from §2 (a different one from the profile patient, so the history timing is not disturbed).
2. Start the timer. Open a new consultation for that patient (no appointment).
3. Enter vitals: temperature, systolic and diastolic blood pressure, pulse.
4. Enter complaints and diagnosis (any short text).
5. Add two medicines, each with a name, dosage, frequency and duration.
6. Save the consultation and save the medicines.
7. Stop the timer when both saves are complete and confirmed on screen. Do not click Finish, and do not print.

| Field | Value |
|---|---|
| Elapsed time (mm:ss) | |
| Any warning or error shown | |
| Browser and version | |

The rehearsal adds one visit to a seed patient. The remove script (step 1) deletes it with the other synthetic rows.

## 6. Usability walkthrough (NFR-USB-001, WP-9.6, TC-NFR-USB-001)

**Status: not run. Skipped by the Project Owner on 2026-11-12.** NFR-USB-001 has no walkthrough result. This is recorded as an owner decision, not as a pass.

Qualitative check. No pass or fail threshold is defined in the BRD (SC-006 is deferred). Record what is seen.

| # | Check | Result | Notes |
|---|---|---|---|
| 1 | Tab order on the consultation screen goes vitals, complaints, diagnosis, medicines, in reading order | | |
| 2 | Required fields are marked the same way on every data-entry screen | | |
| 3 | The consultation screen fits one screen without scrolling at the owner's usual window size (CON-016) | | |
| 4 | Enter moves to the next field on the patient search and the appointment form | | |
| 5 | Error messages appear next to the field they concern | | |
| 6 | The Save and Back actions are where a user expects them on each screen | | |
| 7 | Nothing on a data-entry screen uses colour alone to show a warning | | |

Screen size used: ______ x ______ px. Browser and version: ______.

## 7. Browser notes (for the matrix in step 4)

| Item | Chrome | Edge |
|---|---|---|
| Version used for §4 to §6 | | |
| Date input display (M6 and M7 open item: pickers differ per browser; the page always sends yyyy-MM-dd) | | |
| Print preview and Save as PDF on the prescription and export views | | |

Edge is not used for the timings in §4 (D-M8-7). Its checks are in step 4.

## 7A. Browser matrix checklist (step 4, WP-9.3, NFR-CMP-001)

**Status: not run. Skipped by the Project Owner on 2026-11-12.** No Edge check has been run for Module 8, so NFR-CMP-001 (latest Chrome and Edge, desktop) is not verified in Edge. Chrome results are the earlier module browser checks only. This is recorded as an owner decision.

Run the checks below in both Chrome and Edge (latest desktop versions, PDR-28). Mark each cell Pass, Fail or n/a and add a note for any Fail. The test IDs are the acceptance scenarios in `Documentation/02-Planning/14-Testing-Strategy.md` §4.

Browser run record:

| Item | Chrome | Edge |
|---|---|---|
| Version (from the About page) | | |
| Operating system and build | | |
| Date of run | | |
| Screen size and zoom | | |

| # | Screen | What to check | Test IDs | Chrome | Edge |
|---|---|---|---|---|---|
| 1 | SCR-01 Login | Correct sign-in opens home. Wrong password shows the generic message. Sign out returns to sign-in | TC-NFR-SEC-001-1, -2 | | |
| 2 | SCR-02 Forgot password | Matching email and new password reset the password. No match shows the PDR-03 message | NFR-SEC-001 | | |
| 3 | SCR-03 Home | Quick search, today's appointments link and recent patients (five, latest visit first) all work | FR-SRC-001, FR-SRC-002 | | |
| 4 | SCR-04 Search results | Name part finds; phone part finds; no match shows the message | TC-FR-PAT-004-1, -2, -3 | | |
| 5 | SCR-05 Register | Save with all fields. Missing field shows an error. Duplicate warning allows continue | TC-FR-PAT-001-1, -2, -4 | | |
| 6 | SCR-05 Edit | Change a field, save, and see it on the profile | TC-FR-PAT-002-1 | | |
| 7 | SCR-06 Profile | All details and age shown. Edit, Schedule, Start consultation, Visit history and Export buttons work | TC-FR-PAT-003-1, TC-FR-SRC-003-1 | | |
| 8 | SCR-07 Visit history | Newest first. Single-date and range filters work with the date inputs (M6 deferred check, §7) | TC-FR-HIS-001-1, TC-FR-HIS-003-1, -2 | | |
| 9 | SCR-08 Visit detail | Vitals, complaints, diagnosis and medicines shown. Print prescription opens | TC-FR-HIS-002-1, TC-FR-CMP-001-2 | | |
| 10 | SCR-09 Schedule | Book a slot. Overlap is rejected. Unregistered patient is rejected | TC-FR-APT-001-1, -2, -3 | | |
| 11 | SCR-10 Daily list | Change the date. Status change is inline. Archived rows are hidden | TC-FR-APT-002-1, -2; TC-FR-APT-003-4 | | |
| 12 | SCR-11 Consultation | Enter vitals, complaints, diagnosis and two medicines. Out-of-range vitals show a warning and a confirm step. Save works | TC-FR-VIT-001-1 to -5; TC-FR-CMP-001-1; TC-FR-DIA-001-1; TC-FR-RX-001-1, -2 | | |
| 13 | SCR-12 Prescription (print) | A4 preview shows header, patient, vitals, diagnosis, medicines and footer. Print opens the dialog. Save as PDF makes a file | TC-FR-RX-002-1; SC-004 | | |
| 14 | SCR-12 After printing | Edit is blocked. Reprint works. A second prescription for the same visit is not created | TC-FR-RX-002-2, -3 | | |
| 15 | SCR-13 Export form | Date inputs with a range (M7 deferred check, §7). Download CSV opens in a spreadsheet with ISO dates and the 16 columns | TC-FR-EXP-001-1, -2, -3 | | |
| 16 | SCR-13 Printable view | A4 preview. Print opens the dialog. Save as PDF makes a file. Range shows only those visits | TC-FR-EXP-002-1, -2 | | |
| 17 | SCR-14 Clinic profile | Save the header. The header appears on the prescription print view (SCR-12) | FR-RX-002 | | |
| 18 | Layout (all screens) | No horizontal scroll at the run's screen size. Required markers and tab order as in §6 | NFR-USB-001, NFR-CMP-001 | | |

Notes on the run:
- Volume (NFR-SCL-001): run the list screens (SCR-04, SCR-07, SCR-10) on the seeded development data (§2) in each browser, and record the result in §3A. This is part of the owner's browser run.
- Timing (§4) is in Chrome only (D-M8-7). Edge timings are not required by the plan.
- A difference between browsers is not a Fail unless the check does not work. Describe the difference in the note. A Fail is reported to the Project Owner before approval.

## 8. Approval

| Item | Status |
|---|---|
| Page load in §4.1 (NFR-PRF-001) | Recorded: all ten page types under 2 s (median) |
| Search and history in §4.2 (NFR-PRF-002, SC-002) | Not measured (owner decision, 2026-11-12) |
| Rehearsal in §5 (SC-001) | Not measured (owner decision, 2026-11-12) |
| Walkthrough in §6 (NFR-USB-001) | Not run (owner decision, 2026-11-12) |
| Browser matrix in §7A (NFR-CMP-001, Chrome and Edge) | Not run (owner decision, 2026-11-12). Edge not verified |
| Volume on list screens (NFR-SCL-001) | Not run (owner decision, 2026-11-12) |
| Remove seed data (`05-remove-performance-data.sql`) | Run on 2026-11-12 at the owner's request. Result: 0 synthetic patients left; 2 patients, 4 visits and 5 appointments remain (the real rows that were there before the seed) |
| Project Owner approval | Pending |

Approval of this package is given after §4 to §6 are recorded. Misses need an owner decision recorded here before approval.
