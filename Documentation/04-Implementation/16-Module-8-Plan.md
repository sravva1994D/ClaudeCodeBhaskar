# Module 8 Plan: Non-Functional Verification (Performance, Volume, Security Checks, Browser Matrix)

**Status:** Approved by the Project Owner on 2026-11-12, with the recommendations in §3 (D-M8-1 to D-M8-8). Steps are built one at a time on the owner's go-ahead.
**Milestone:** M8 (`15-Development-Roadmap.md`). Work packages WP-9.1 to WP-9.6 (`16-Work-Breakdown-Structure.md`).

## 1. Context

Modules 1 to 7 are approved. Module 8 verifies the non-functional requirements and the Success Criteria that depend on measurement. It is mostly measurement and documentation. It adds one seed script, and code changes only if a measurement fails and the Project Owner decides on a fix.

Browser timing and the browser matrix need a real browser. No browser automation is used in Phase 1 (PDR-29), so the owner runs these checks by hand and I prepare the method, the seed data and the record.

Already decided and not re-opened:
- PDR-30: browser timing, median of 5 runs.
- PDR-28: record the browser version on each run.
- DEC-019: Chrome and Edge (desktop) are the target; Safari not required.
- DEC-017: no encryption at rest in Phase 1.
- Module 2 precedent (`06-Module-2-Review-Package.md` §3.1): in-process timing gave medians under 25 ms on 1,000 patients. That is not the PDR-30 measure; the browser run is.

## 2. Scope

| In | IDs | WP |
|---|---|---|
| Seed-performance data: 1,000 synthetic patients with visits, vitals, prescriptions and appointments across a year | NFR-PRF-001, NFR-PRF-002, SC-002, NFR-SCL-001 | WP-9.1, 9.5 |
| Browser timing of 10 page types (median of 5 runs) | NFR-PRF-001 | WP-9.1 |
| Browser timing of 10 search terms and visit history load (median of 5 runs) | NFR-PRF-002, SC-002 | WP-9.1 |
| Volume check: every list screen with 1,000 patients | NFR-SCL-001 | WP-9.5 |
| Consultation timed rehearsal (2 to 3 minutes, test data) | SC-001 | WP-9.2 |
| Browser matrix: Chrome and Edge (desktop), acceptance checks on every screen; deferred date-picker checks from M6 and M7 | NFR-CMP-001 | WP-9.3 |
| Security checks: login, sliding timeout, hashing, transport configuration | NFR-SEC-001, NFR-SEC-002 | WP-9.4 |
| Usability walkthrough (qualitative) | NFR-USB-001 | WP-9.6 |

Out of scope: any new feature; search paging or full-text search (a Project Owner decision, only if a measurement misses); acceptance of the 31 in-scope IDs (M9); deferred items NFR-REL-001/002, SC-003, SC-006 (OOS-011, OOS-014). No BRD change.

## 3. Decisions for the Project Owner

Recommendations are shown. Steps 1 to 5 do not start until these are answered.

| # | Question | Recommendation |
|---|---|---|
| D-M8-1 | Which database holds the seed-performance data? | The development database (`PatientManagement`), not the test database. Web tests clear the test tables before each test, so timing data there would be lost. A cleanup script removes the synthetic rows afterwards. The script refuses to run unless the marker prefix is present |
| D-M8-2 | Seed volume | 1,000 patients (from `03-generate-test-patients.sql`). Each patient has 0 to 12 visits spread across the last 365 days (about 5,000 visits in total), some with vitals only, some with medicines. About 2,000 appointments over the same year. All names, phones and addresses are synthetic and marked `TEST` |
| D-M8-3 | How were multi-day visits seeded for the Module 6 browser checks? (open question from M6 and M7) | Not needed for M8 if D-M8-2 is accepted: the new script writes visit dates directly. Please confirm the M6 method so the record is complete |
| D-M8-4 | Which 10 page types are timed? | Sign-in; home; patient search results (SCR-04); patient profile (SCR-06); visit history (SCR-07); daily appointments list; new appointment; consultation (SCR-11); prescription print view; export form (SCR-13). Timing is on the seeded data, one patient with the most visits for the profile and history pages |
| D-M8-5 | Which 10 search terms? | The Module 2 set (`06-` §3.1): `Patient 0500`, `Patient 0001`, `Brown`, `Test`, `00000042`, `00000999`, `Harris`, `Example`, `Clark`, `zzzz`. Terms are synthetic |
| D-M8-6 | Timing method | Browser DevTools only (Network timing for page load; a stopwatch or the Performance panel for search and history). No automation script (PDR-29). The steps are written in the review package so they can be repeated |
| D-M8-7 | Browser for timing and matrix | Timing in Chrome (the primary target). Matrix checks in Chrome and Edge. Edge version recorded on each run (PDR-28) |
| D-M8-8 | If a measurement misses the guideline (2 s page load, 2 to 5 s search) | Report it with the numbers. No fix in M8 unless the owner decides. The likely fix is a migration (full-text index), which is a schema change and needs an owner decision (`07-Database-Design` §4) |

## 4. Steps

Each step: build with 0 warnings where code changes, tests pass, commit and push to `development` (Conventional Commits, CLAUDE §5.2), one step per owner go-ahead.

1. **Seed-performance data (WP-9.1, 9.5).** `database/Scripts/04-generate-performance-data.sql`: adds visits, vitals, prescriptions, prescription items and appointments to the 1,000 generated patients, marked `TEST`. Guards: refuses to run on any database not named in D-M8-1, and refuses if rows already carry the marker. `database/Scripts/05-remove-performance-data.sql`: removes only marked rows. Verification: row counts recorded; a sample profile opened in the browser. Commit.
2. **Measurement record (WP-9.1, 9.2, 9.6).** `Documentation/04-Implementation/16-Module-8-Review-Package.md`: the method (D-M8-4 to D-M8-7), result tables with blank cells, the browser version lines, the consultation rehearsal script (SC-001 steps with a stopwatch), and the usability walkthrough checklist (tab order, required markers, one-screen flow). No patient data in the document. Commit.
3. **Security and volume checks (WP-9.4, 9.5).** Confirm which checks already exist as tests (TC-NFR-SEC-001-1 to -3, TC-NFR-SEC-002-1, sliding timeout, the protected-URL redirect). Add any missing automated check. Run the full unit and integration suites with the seed data loaded to confirm no test depends on the development database. Transport configuration is reviewed as text (HTTPS beyond localhost, PDR-21); not run on localhost. Record exact results. Commit if tests change.
4. **Browser matrix checklist (WP-9.3).** Add to the review package the acceptance checklist for each screen (Chrome and Edge), including the M6 and M7 date-picker checks deferred to M8 and the printable views with Save as PDF in both browsers. Commit.
5. **Close-out.** Owner runs the timing, volume, rehearsal, walkthrough and matrix steps and reports the numbers. Record them exactly as reported (median and runs), including a miss. Update `18-Planning-Traceability-Matrix.md` and `12-Requirement-Traceability-Matrix.md` for NFR-PRF-001/002, SC-001, SC-002, NFR-SCL-001, NFR-CMP-001, NFR-SEC-002, NFR-USB-001. Then Project Owner approval. The remove script is run only when the owner says the timing record is complete.

## 5. Verification

| Check | How | Pass rule |
|---|---|---|
| Build | `dotnet build PatientManagement.sln --no-incremental` (stop the app first) | 0 errors, 0 warnings |
| Tests | `dotnet test PatientManagement.sln --no-build` | All pass; IDs in names |
| Seed | Row counts query on the seed tables | Counts match D-M8-2 (about 1,000 patients, 5,000 visits, 2,000 appointments) |
| Page load (NFR-PRF-001) | DevTools, 10 page types, median of 5 runs, Chrome | Median about 2 s or less (guideline). A miss is reported |
| Search and history (NFR-PRF-002, SC-002) | 10 terms, median of 5 runs; history load | Median 2 to 5 s or less (guideline). A miss is reported |
| Volume (NFR-SCL-001) | Each list screen with 1,000 patients | Loads and is usable |
| Consultation (SC-001) | Timed rehearsal with test data | 2 to 3 minutes (usability goal) |
| Browser matrix (NFR-CMP-001) | Acceptance checklist, Chrome and Edge | All checks pass in both; versions recorded |
| Security (NFR-SEC-001/002) | Automated tests and transport review | Pass and recorded |
| Cleanup | Remove script, then row counts | Zero marked rows remain |
| No patient data in logs or docs | Search the review package and the application log | No real data; synthetic names only |

## 6. Risks

| Risk | Handling |
|---|---|
| Contains search misses 2 to 5 s on the seeded data | Measured; reported; index or paging is an owner decision (D-M8-8) |
| Seed data left in the development database | Marker and remove script; cleanup verified by counts |
| Test runs clear the seed data | Seed on the development database, not the test database (D-M8-1) |
| Browser timings vary between runs | Median of 5, runs recorded (PDR-30) |
| Edge differs from Chrome in print or Save as PDF | Matrix checks in both browsers (D-M8-7) |
| Timing done without automation is slow | Ten page types and ten terms only; the method is written so it can be repeated |

## 7. Completion criteria

- Steps 1 to 5 committed on `development`, each building and passing tests.
- The seed and remove scripts are reviewed, and the seed has been run and counted.
- NFR-PRF-001, NFR-PRF-002, SC-001, SC-002, NFR-SCL-001, NFR-CMP-001, NFR-SEC-002 and NFR-USB-001 have recorded results (including any miss) in the review package.
- Matrices `18-` and `12-` are updated.
- The Project Owner approves the review package. Any miss has an owner decision recorded.

## 8. Links

- `Documentation/02-Planning/14-Testing-Strategy.md` §2 and §3
- `Documentation/02-Planning/17-Planning-Decisions-and-Open-Items.md`: PDR-28, PDR-29, PDR-30
- `Documentation/04-Implementation/06-Module-2-Review-Package.md` §3.1 (in-process timing)
- `Documentation/04-Implementation/15-Module-7-Review-Package.md` §6 (open items for M8)
