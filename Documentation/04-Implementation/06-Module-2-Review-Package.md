# 06 — Module 2 Review Package (Patient Management)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M2 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/05-Module-2-Plan.md` (approved 2026-11-11, decisions D-M2-1 to D-M2-6 accepted)
**Status:** **Approved by the Project Owner, 2026-11-11.** Browser checks reported as completed with acceptable results (§4). The browser version and the browser timing figures were not supplied with the approval and are still to be recorded.

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 2 plan, approved with recommendations | `c69decc`, approval in `3d946d6` |
| 1 | Patient entity, configuration, persisted computed code, gender check, indexes; migration `AddPatients` | `3d946d6` |
| 2 | Patient repository: lookup, search (literal `%`, `_`, `\`), duplicate check, add, update | `c25cbb6` |
| 3 | Patient validator and search-term rule; single input type | `e37bf41` |
| 4 | Age calculator with injected clock | `da50c3d` |
| 5 | Profile and search-row DTOs; mapping | `2144707` |
| 6 | Patient service: register, edit, profile, search; duplicate confirmation | `478a36a` |
| 7 | Top-bar quick search and search page | `1c95fa5` |
| 8 | Register, profile and edit screens; links from search | `cc937ea` |
| 9 | Synthetic data generator (SQL and test), search timing check, this review | this commit |

Out of scope for Module 2, as planned: visits and last visit on the profile (Module 4), appointments link (Module 3), consultation start (Module 4), export link (Module 7), recent patients on the home page (Module 6).

## 2. Requirements Covered

| Requirement | Result | Tests |
|---|---|---|
| FR-PAT-001 Register patient | Implemented | TC-FR-PAT-001-1 (save all fields), -2 (missing field), -4 (duplicate warning and continue), -5 (age) in unit and integration tests; -3 (code format) in data tests |
| FR-PAT-002 Edit patient | Implemented | TC-FR-PAT-002-1 (edit and profile shows change) |
| FR-PAT-003 View patient | Implemented; age shown | TC-FR-PAT-003-1 (every field shown) |
| FR-PAT-004 Search by name or phone | Implemented | TC-FR-PAT-004-1 (name), -2 (phone), -3 (no match); empty search error (D4) |
| FR-SRC-001 Quick search | Implemented from the top bar | TC-FR-SRC-001-1 |
| FR-SRC-003 Navigate profile and visits (partial) | Profile to search, and search to profile. Visits link comes in Module 4 | Profile link test |
| NFR-PRF-002, SC-002 | Measured (§3) | Timing test on 1,000 synthetic patients |

Patients cannot be deleted or archived (PDR-25). The code is set by the database and is never changed by the application.

## 3. Test Results

Final run, 2026-11-11, clean build:

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings |
| Unit tests | 95 passed, 0 failed |
| Integration tests | 89 passed, 0 failed |
| Logging of patient data | None: the patient code makes no log calls (checked by search of the patient code paths) |

### 3.1 Search timing (NFR-PRF-002, SC-002)

Method: 1,000 synthetic patients in `PatientManagement_Test` (generator in the timing test; SQL version in `database/Scripts/03-generate-test-patients.sql`). Each term is sent to `/Patients/Search` through the web stack in-process. One warm-up request, then 5 timed runs. Median reported (PDR-30). Timings in milliseconds.

| Term | Matches | Median | Min | Max | Runs |
|---|---|---|---|---|---|
| Patient 0500 | 1 | 10 | 5 | 34 | 5, 6, 10, 10, 34 |
| Patient 0001 | 1 | 6 | 5 | 9 | 5, 5, 6, 6, 9 |
| Brown | 100 | 6 | 6 | 8 | 6, 6, 6, 8, 8 |
| Test | 1000 | 21 | 18 | 27 | 18, 19, 21, 23, 27 |
| 00000042 | 1 | 5 | 4 | 6 | 4, 5, 5, 6, 6 |
| 00000999 | 1 | 6 | 5 | 18 | 5, 6, 6, 10, 18 |
| Harris | 100 | 9 | 8 | 10 | 8, 9, 9, 9, 10 |
| Example | 0 | 5 | 5 | 7 | 5, 5, 5, 6, 7 |
| Clark | 100 | 8 | 7 | 12 | 7, 7, 8, 9, 12 |
| zzzz | 0 | 9 | 5 | 10 | 5, 6, 9, 9, 10 |

Result: every median is under 25 ms, well inside the 2 to 5 second guideline in the BRD. Each search returned the expected number of rows.

Limits of this measurement:
- It is in-process. It leaves out the browser's own rendering and any network delay. The browser timing run (§4) is the PDR-30 measurement.
- The search uses a contains match on name and phone, which cannot use an index (`07-Database-Design` §4). It was fast here at 1,000 rows. Full-text search is not needed at this size.
- The 5-run timing is not gated in the test. Speed is reported, not enforced, because the BRD targets are guidelines.

## 4. Manual Checks

| Check | Method | Status |
|---|---|---|
| Patients migration applied to the development database | `dotnet ef database update`; read-only query | Done. `Patients` table present, no patients yet |
| Register, edit, view, search in the browser | Development database, test patients entered by hand | Completed, reported by the Project Owner, 2026-11-11 |
| Duplicate warning in the browser | Register a patient with an existing phone number; check the warning; continue | Completed, reported by the Project Owner, 2026-11-11 |
| Quick search from the top bar | Search by name and by phone | Completed, reported by the Project Owner, 2026-11-11 |
| Browser timing (PDR-30, median of 5) | `03-generate-test-patients.sql` on `PatientManagement_Test`; time each search in the browser | Reported as acceptable. Figures and browser version not yet recorded |
| Patient data in the application log | Run the app, use the screens, read the console log | Completed, reported by the Project Owner, 2026-11-11. No issues found |

## 5. Decisions Taken

| Item | Decision | Reference |
|---|---|---|
| D-M2-1 patient code | Persisted computed column; full number from 1,000,000 | Plan §3, step 1 |
| D-M2-2 phone matching | Exact text as stored, for duplicates and search | Plan §4 |
| D-M2-3 length limits | Validator enforces column sizes | Plan §4; step 3 tests |
| D-M2-4 no prefill | "Register" link goes to a blank form | Plan §4; step 8 |
| D-M2-5 performance data | Test database only | Plan §4; step 9 |
| D-M2-6 no paging | All matches shown, ordered by name | Plan §4 |
| Single input type | `PatientInput` for register and edit | Plan step 5 (changed) |
| Search row carries Id | Only identifier exposed; needed for the profile link | Plan step 5 (changed) |
| Leap-day birthday | 28 February in non-leap years | Plan §3; step 4 |
| Nullable form fields | Service messages show instead of MVC's required message | `13-Validation-Strategy` §5 |
| Search submitted vs null | Presence of the query key decides, so an empty search shows its error | Step 7 |
| Patient code correction | First draft repeated codes beyond 999,999; corrected in step 1 | Plan §7 |

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | Browser checks and browser timing not yet run | PDR-30 timing and the UI checks are not on record | Project Owner runs §4 and reports results |
| 2 | Phone numbers match only as typed | `+91 98…` and `9198…` are different patients to the duplicate check | D-M2-2 accepted; revisit if it causes missed duplicates |
| 3 | Search has no result limit or paging | Long result lists at larger sizes | D-M2-6 accepted; add paging only if timing fails |
| 4 | Case-insensitive matching depends on the database collation | A different collation could change results | Tests run on the default collation; they would catch a change |
| 5 | HTTPS redirect log warning (from Module 1, item 4) still open | Log noise on plain HTTP | Project Owner decision still pending |
| 6 | Last visit not shown on the profile | Screen catalogue lists it | Added in Module 4 with consultations |
| 7 | Data generator exists in two forms (SQL and test code) | Could drift apart | Both are deterministic; the SQL is used for the browser run |

## 7. Commits

| Commit | Message |
|---|---|
| `c69decc` | docs: add Module 2 implementation plan |
| `3d946d6` | feat(data): add Patient entity and AddPatients migration |
| `c25cbb6` | feat(data): add patient repository with search and duplicate check |
| `e37bf41` | feat(business): add patient validator and search term rule |
| `da50c3d` | feat(business): add age calculator with injected clock |
| `2144707` | feat(business): add patient DTOs and mapping |
| `478a36a` | feat(business): add patient service |
| `1c95fa5` | feat(web): add quick search from the top bar and the search page |
| `cc937ea` | feat(web): add patient register, profile and edit screens |
| this commit | feat(tests): search timing on 1,000 synthetic patients; generator script; Module 2 review |

## 8. Approval

Module 2 approved by the Project Owner on 2026-11-11, with the manual checks in §4 reported as completed.

Still to record, when available: the browser name and version for the timing run, and the browser timing figures. Item 5 in §6 (HTTPS redirect in Development, from Module 1) remains open.
