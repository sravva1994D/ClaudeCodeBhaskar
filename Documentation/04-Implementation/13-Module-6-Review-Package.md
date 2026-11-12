# 13 — Module 6 Review Package (Visit Date Filter and Recent Patients)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M6 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/12-Module-6-Plan.md` (plan approved in chat 2026-11-12; decisions D-M6-1 to D-M6-7 accepted as recommended)
**Status:** **Approved by the Project Owner on 2026-11-12.** Browser checks passed (§4, Chrome 157.0.8103.51).

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 6 plan (`12-`) | `b228961` |
| 1 | Repositories: visit list by from and to day (inclusive); recent patients by last visit; patients by ids | `59a3ce1` |
| 2 | Services: range rules for the visit list (single day, refusals); recent patients service | `248a9c9` |
| 3 | Visit history screen: From and To inputs, Show visits, Clear filter, error and empty messages | `8d57fd3` |
| 4 | Home page: Recent patients section (five latest, linked to the profile) | `668135d` |
| 5 | Close-out: this review package; traceability matrices `18-` and `12-`; Module 4 review §6 items 3 and 4 | `c287f1b` |
| Review fixes | Code review (Module 6 scope): a range ending on the last day the type holds no longer fails; a malformed date is refused, not dropped | this commit |

Out of scope for Module 6, as planned: export (FR-EXP-001/002, M7); search timing and browser matrix (M8). No schema change and no migration in this module.

## 2. Requirements Covered

| Requirement | Result | Tests (named in the test code) |
|---|---|---|
| FR-HIS-003 Filter visit history by date | Implemented. Browser check pending (§4) | TC-FR-HIS-003-1 (single date): `ConsultationServiceTests.FR_HIS_003_a_single_day_filter_returns_only_that_day`; `ConsultationRepositoryTests.FR_HIS_003_single_day_filter_returns_only_visits_on_that_day`; `VisitPagesTests.FR_HIS_003_a_single_date_shows_only_visits_on_that_day`. TC-FR-HIS-003-2 (range, boundaries inclusive): `ConsultationServiceTests.FR_HIS_003_a_range_is_passed_to_the_repository_as_given`; `ConsultationRepositoryTests.FR_HIS_003_range_includes_both_end_days_whole_and_excludes_the_day_after`; `VisitPagesTests.FR_HIS_003_a_range_shows_only_visits_inside_it_with_both_ends_inclusive`. Refusals and empty range: `ConsultationServiceTests.FR_HIS_003_a_from_date_after_the_to_date_is_refused_and_nothing_is_read`; `ConsultationServiceTests.FR_HIS_003_a_to_date_without_a_from_date_is_refused`; `VisitPagesTests.FR_HIS_003_a_from_date_after_the_to_date_shows_an_error_and_no_visits`; `VisitPagesTests.FR_HIS_003_a_range_with_no_visits_says_so_and_offers_to_clear_it` |
| FR-SRC-002 View recent patients (five, by last visit, consultations only) | Implemented. Browser check pending (§4) | TC-FR-SRC-002-1 (five, latest visit first): `PatientServiceTests.FR_SRC_002_recent_patients_keep_the_visit_order_with_their_last_visit`; `ConsultationRepositoryTests.FR_SRC_002_recent_patients_are_the_five_latest_by_last_visit`; `HomeDailyListTests.FR_SRC_002_home_lists_the_five_latest_visited_patients_in_visit_order`. TC-FR-SRC-002-2 (no-visit patients excluded, PDR-11): `PatientServiceTests.FR_SRC_002_patients_with_no_visit_are_not_listed`; `PatientServiceTests.FR_SRC_002_a_patient_outside_the_visit_list_is_not_added_to_it`; `ConsultationRepositoryTests.FR_SRC_002_patient_with_no_visit_is_not_in_the_recent_list`; `HomeDailyListTests.FR_SRC_002_home_leaves_out_patients_with_no_visit`. Supporting: `PatientRepositoryTests.FR_SRC_002_get_by_ids_returns_only_the_requested_patients`; `HomeDailyListTests.FR_SRC_002_home_says_so_when_no_patient_has_a_visit`; `HomeDailyListTests.FR_SRC_002_recent_patient_name_links_to_the_profile` |

FR-HIS-001, FR-HIS-002, FR-SRC-003 and FR-APT-002 were not changed in behaviour. The visit history page, the visit detail and the home page keep their existing tests, and they still pass.

## 3. Test Results

Run after the review fixes (this commit). Before those fixes, the same suites passed at step 4 (`668135d`): 345 unit and 271 integration.

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings |
| Unit tests | 345 passed, 0 failed (Module 5 end: 338; Module 6 added 7) |
| Integration tests | 274 passed, 0 failed (Module 5 end: 258; Module 6 added 16, including 3 regression tests for the review fixes) |

## 4. Browser Checks (for the Project Owner)

Stop the app if it is running, then start it again. Module 6 adds no migration. Use a patient with visits on several different days. Record the browser and version for each run (PDR-28).

| # | Check | Expected | Result |
|---|---|---|---|
| 1 | Open a patient with visits on several days, then Visit history | All visits listed, newest first (unchanged) | |
| 2 | Enter From only (a day with a visit), Show visits | Only visits on that day are listed | |
| 3 | Enter From and To covering two visit days, Show visits | Visits on both the From day and the To day are listed; visits outside are not | |
| 4 | Enter From later than To, Show visits | Red message "From date must not be after To date."; no list | |
| 5 | Enter only To, Show visits | Message "Enter a From date to filter by a range."; no list | |
| 6 | Enter a range with no visits | "No visits in this date range." with a Clear filter link; Clear filter shows all visits again | |
| 7 | Open a patient with no visits, then Visit history | "No visits yet." (unchanged); no filter results | |
| 8 | Home page, with at least one patient who has visits | Recent patients lists up to five, latest visit first, with code and last visit time | |
| 9 | Start and finish a consultation for a patient who is not at the top of Recent patients | That patient moves to the top of Recent patients | |
| 10 | Home page, with a patient who has never had a visit | That patient is not in Recent patients | |
| 11 | Click a name in Recent patients | Opens that patient's profile | |
| 12 | Home page with no visits in the database (or a new database) | "No recent patients yet." | |

Owner report: all checks in this table passed, in Chrome 157.0.8103.51 (reported in chat). Per-check results were not supplied separately.

Browser used: Chrome 157.0.8103.51. Result: passed. Date of approval: 2026-11-12.

## 5. Decisions Made

The plan (`12-`) recorded D-M6-1 to D-M6-7 with recommendations. They were accepted with the plan. One point was added during the build and is not in the plan:

| # | Point | Decision |
|---|---|---|
| D-M6-8 | A To date with no From date | Refused with "Enter a From date to filter by a range." It is not ignored, so the user is told what is missing. Accepted with the Project Owner's approval on 2026-11-12 |

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | Date inputs look different in each browser | The date picker and its display format are set by the browser. The page always sends and reads yyyy-MM-dd | Check the picker in Chrome (§4); Edge in M8 |
| 2 | Recent patients counts visits by their start time | A visit started earlier and finished later still counts from its start time | Acceptable; the BRD asks for "last visit", which is the visit time (PDR-11) |
| 3 | Module 4 review §6 items 3 and 4 | Closed by this module | Recorded in `10-Module-4-Review-Package.md` |
| 4 | Module 5 review §6 items 2 and 8 | Still open, not part of Module 6 | Project Owner decision (see `11-Module-5-Review-Package.md` §6) |
| 5 | Export | Not built | Module 7 |
| 6 | Search timing and browser matrix | Not measured | Module 8 |

## 7. Approval

**Approved by the Project Owner on 2026-11-12.** Browser checks passed in Chrome 157.0.8103.51 (§4). D-M6-8 was accepted with the approval. Module 6 is complete, pending the final `development` → `main` PR after Modules 7 and 8.
