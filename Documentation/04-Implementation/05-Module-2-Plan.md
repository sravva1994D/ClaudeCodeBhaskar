# 05 — Module 2 Implementation Plan (Patient Management)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 2 (Patient Management) and FR-SRC-001 (quick search from the top bar)
**Milestone:** M2 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Plan for review, 2026-11-11. No code written. Implementation starts on the Project Owner's approval.
**Source rules:** BRD (amended 2026-11-10); CLAUDE.md §3, §4, §6, §8, §9, §10; `Documentation/04-Implementation/03-Module-1-Plan.md` (pattern)

---

## 1. Scope

| In scope | Requirement IDs | Work packages (`16-WBS`) |
|---|---|---|
| Patient entity, configuration and migration `AddPatients` | FR-PAT-001 to FR-PAT-004 | WP-3.1 |
| Patient validator (required fields, formats, date of birth not in future, length limits) | FR-PAT-001 (PDR-22) | WP-3.2 |
| Duplicate check, returned as a warning that can be confirmed | FR-PAT-001 (PDR-10) | WP-3.2 |
| Register and edit services and screen (SCR-05) | FR-PAT-001, FR-PAT-002 | WP-3.3 |
| Patient profile with calculated age (SCR-06) | FR-PAT-003 | WP-3.4 |
| Search results (SCR-04) and top-bar quick search | FR-PAT-004, FR-SRC-001 | WP-3.5 |
| Link from profile back to search (partial FR-SRC-003) | FR-SRC-003 (partial) | WP-3.4 |
| Early search performance check (1,000 synthetic patients) | NFR-PRF-002, SC-002 | WP-3.6 |

Out of scope for this module, each in its later milestone: visits and last-visit on the profile (M4), appointments link (M3), consultation start (M4), export link (M7), recent patients on the home page (M6). No placeholder links to unbuilt features are shown.

## 2. Requirement mapping

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md` §4) |
|---|---|---|
| FR-PAT-001 | AC-1 saved with all required fields; AC-2 retrievable; AC-3 code PT-000123 form; AC-4 duplicate warning can be continued; AC-5 age from DOB; AC-6 DOB not in future, phone 7 to 15 digits, email standard | TC-FR-PAT-001-1 (save with all six required fields), -2 (each missing field gives an error), -3 (code format), -4 (duplicate warning and continue), -5 (age from DOB) |
| FR-PAT-002 | AC-1 edit and save; AC-2 visible on profile; AC-3 no deletion or archiving | TC-FR-PAT-002-1 |
| FR-PAT-003 | AC-1 profile shows all captured details | TC-FR-PAT-003-1 |
| FR-PAT-004 | AC-1 name search (contains, case-insensitive); AC-2 phone search | TC-FR-PAT-004-1 (name), -2 (phone), -3 (no match message) |
| FR-SRC-001 | AC-1 quick search from the top bar returns matches | TC-FR-SRC-001-1 |
| FR-SRC-003 (partial) | AC-1 move from profile to visits and back | TC-FR-SRC-003-1 is completed in M4; this module covers the profile-to-search link only |
| NFR-PRF-002, SC-002 | Search results within 2 to 5 s on localhost, 1,000 records; median of 5 runs (PDR-30) | Timed search, 10 terms (`14-` §3); recorded in the review package |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points applied

| Point | Design | Source |
|---|---|---|
| Patient code | `PT-` followed by the identity, zero-padded to six digits (for example `PT-000123`). Stored in a persisted computed column with a unique index, so it is set by the database at insert and cannot be changed | AD-09; `06-` §2.2; `07-` §2 (see decision D-M2-1) |
| Required fields | Full name, date of birth, gender, phone, email, address. Errors block the save | BRD Patient Management; PDR-01; `13-` §3.1 |
| Gender | One of Male, Female or Other. A database CHECK backs the list | PDR-01; `07-` §2 |
| Date of birth | Required. Not in the future. No minimum or maximum age (BRD is silent) | BRD; PDR-22 |
| Phone | 7 to 15 digits; spaces, hyphens and a leading `+` allowed. Stored as entered | PDR-22 |
| Email | Standard email format (same check as the account email) | PDR-22 |
| Age | Calculated from date of birth and the system date at display time. Not stored. Whole years | BRD; AD-07 |
| Duplicates | Exact match on name (case-insensitive), date of birth, or phone. Returned as a warning. The save continues only after confirmation | BRD; PDR-10; `13-` §4 |
| Search | Contains match on name or phone, case-insensitive, no minimum length. Empty term is an error | PDR-23; D4 (approved 2026-11-10) |
| Search input | `%` and `_` in the term are matched literally, not as wildcards | Plan (correctness of contains search) |
| Patients are not deleted or archived | No delete or archive action exists for patients | PDR-25; CLAUDE §6 |
| Time | The service receives the clock, so age tests are deterministic | CLAUDE §10 |
| Logging | Patient fields are never logged | CLAUDE §8; Developer Guide §7 |
| Layers | Controller calls one service; service rules; repository thin; entities stay in Data and DTOs cross to Web | AD-02, AD-03; `09-` §3 |

## 4. Decisions needed from the Project Owner

These are not in the BRD or the planning decisions. Each has a recommended answer. Approval of the plan means accepting the recommendation unless you say otherwise.

| # | Question | Recommendation | Why it needs a decision |
|---|---|---|---|
| D-M2-1 | Patient code: persisted computed column (set by the database), or set by a second update after insert (AD-09 wording)? | Computed persisted column. One insert, no second save, and the code cannot drift from the identity | Changes how AD-09 is implemented; the result is the same code format |
| D-M2-2 | Phone matching for duplicates and search: exact text as stored, or digits only? | Exact text as stored, for both (PDR-10 says exact match; PDR-23 says contains). A user who types `+91 98…` and `9198…` gets no match | Normalising phone numbers is a rule the BRD does not state |
| D-M2-3 | Maximum lengths: enforce the database column sizes in the validator (name 200, email 256, address 500, phone 20)? | Yes. Without it the database rejects the save with an unclear error | A rule not written in the BRD; it follows from the column sizes in `07-` |
| D-M2-4 | Empty-result link to "Register": prefill the name typed in the search? | No prefill in Phase 1. The link goes to the blank register form | Keeps the screen simple (NFR-USB-001) |
| D-M2-5 | Performance data: generated by a script into the test database `PatientManagement_Test`, not the development database? | Yes. The data is synthetic, and the test database is already guarded | Keeps the physician's development data free of test records |
| D-M2-6 | Results list: show all matches, with no paging? | Yes. At 1,000 records, all matches are shown, ordered by name. Paging is added only if the timing test fails | BRD does not set a result limit |

## 5. Work steps (each builds, tests pass and is committed on `development`)

Each step: build with 0 warnings, `dotnet test`, commit with the format in CLAUDE §5.2, push to `origin/development`, and list the commit in chat.

| Step | Work | Files (expected) | Tests added (project) |
|---|---|---|---|
| 1 | `Patient` entity, configuration, `DbSet<Patient>`; computed `PatientCode` (D-M2-1); gender CHECK; indexes per `07-` §4 (FullName, Phone, PatientCode unique, DateOfBirth and Phone); migration `AddPatients` | `Data/Entities/Patient.cs`, `Data/Configurations/PatientConfiguration.cs`, `Data/Context/`, `Data/Migrations/` | Integration (data): code has the `PT-000123` form; code is unique; gender outside the list is refused by the database |
| 2 | `IPatientRepository` and `PatientRepository`: GetById, Search (contains on name or phone, escaping `%` and `_`), FindPossibleDuplicates, Add, Update. Read-only queries use no tracking | `Data/Abstractions/IPatientRepository.cs`, `Data/Repositories/PatientRepository.cs` | Integration (data): search by name part and by phone part; case-insensitive; `%` and `_` are literal; duplicate query matches name, date of birth, or phone; update excludes the same patient from its own duplicate check |
| 3 | `PatientValidator`: required fields, formats (PDR-22), date of birth not in future, length limits (D-M2-3), empty search term | `Business/Validators/PatientValidator.cs` | Unit: each required field; phone with 6 and 16 digits rejected, 7 and 15 accepted; spaces, hyphens and leading `+` accepted; future date of birth rejected; today accepted; invalid email rejected; each length limit at the limit and one over |
| 4 | Age calculator using the injected clock: whole years, birthday not yet reached, leap-day birth | `Business/Services/AgeCalculator.cs` | Unit: age on the birthday, the day before, and for a 29 February birth in a non-leap year |
| 5 | DTOs: `PatientCreateDto` and `PatientUpdateDto` (input), `PatientDto` (profile, with age), `PatientSummaryDto` (search row). Manual mapping in Business | `Business/DTOs/Patient*.cs` | Unit: mapping keeps every field; the summary has no internal fields |
| 6 | `IPatientService` / `PatientService`: RegisterAsync and UpdateAsync (validate, then duplicate warning unless confirmed, then save), GetAsync, SearchAsync (empty term is an error). Generates no code itself | `Business/Interfaces/IPatientService.cs`, `Business/Services/PatientService.cs` | Unit (NSubstitute on the repository and the clock): TC-FR-PAT-001-1 to -5 (service part); TC-FR-PAT-002-1; TC-FR-PAT-003-1; TC-FR-PAT-004-1 to -3 (service part). A duplicate with no confirmation returns a warning and saves nothing |
| 7 | Quick search from the top bar (FR-SRC-001): search box in the layout, posting to `/Patients/Search` | `Web/Views/Shared/_Layout.cshtml`, `Web/Controllers/PatientsController.cs` | Integration (web): search from the layout finds a patient by name (TC-FR-SRC-001-1) |
| 8 | `PatientsController` (routes 7 to 12 in `09-` §2): Search, Create GET/POST, Details, Edit GET/POST; `ConfirmWarnings` hidden field; anti-forgery on every POST; ViewModels; views for SCR-04, SCR-05, SCR-06 | `Web/Controllers/PatientsController.cs`, `Web/ViewModels/Patients/`, `Web/Views/Patients/` | Integration (web) against the test database: register with all fields (TC-FR-PAT-001-1); missing field shows an error (-2); duplicate warning then continue (-4); edit then profile shows the change (TC-FR-PAT-002-1); profile shows all fields and age (TC-FR-PAT-003-1); name and phone search (TC-FR-PAT-004-1, -2); no-match message (-3); profile links back to search (TC-FR-SRC-003-1, profile part) |
| 9 | Performance: synthetic data script for 1,000 patients into `PatientManagement_Test` (D-M2-5); timed search for 10 terms; median of 5 runs; results recorded (WP-3.6) | `database/Scripts/03-generate-test-patients.sql`, `Documentation/04-Implementation/` (results) | Timed check, not a unit test; result recorded with the browser version used |
| 10 | Documentation: Developer Guide (patient rules, search escaping), Setup Guide (test data script), Module 2 review package | `Documentation/04-Implementation/` | — |

Steps 1 to 6 are data and business logic and need no web project. Steps 7 to 8 are web. Step 9 needs steps 1 to 8.

## 6. Verification at M2 exit (gate from `15-` §2)

| Check | How | Pass rule |
|---|---|---|
| Build | `dotnet build PatientManagement.sln --no-incremental` | 0 errors, 0 warnings |
| Tests | `dotnet test PatientManagement.sln` | All pass, including the tests in §2 |
| Migration | `dotnet ef database update` on the development database; `dotnet ef migrations script` reviewed | Patients table created with the constraints in `07-` §2 |
| Register, edit, view, search | Browser, development database, test patients | Each flow works as in SCR-04 to SCR-06 |
| Duplicate warning | Browser: register a patient whose phone matches an existing one | Warning shown; saves only after confirmation |
| Quick search | Top bar, from the home page | Finds by name and by phone |
| Search timing (NFR-PRF-002) | 1,000 synthetic patients in the test database; 10 terms; median of 5 runs | Recorded. Target 2 to 5 s is a guideline (BRD); a miss is reported, and paging or full-text search is decided by the Project Owner (`07-` §4) |
| Patient data not logged | Search the application log after the browser run | No patient name, date of birth, phone, email or address in the log |

## 7. Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Contains search on 1,000 rows misses the 2 to 5 s target (`07-` §4 search note) | Medium | Medium | Measure early (step 9 before M2 closes); options recorded for the Project Owner |
| LIKE wildcards in user input match too much | Medium | Low | Escape `%`, `_` and `\` in the repository; tested (step 2) |
| Duplicate warnings on common names or phones | Medium | Low | Warning only, never a block; the user confirms |
| Patient code overflows six digits | Low | Low | Six digits give room beyond 999,999 patients; the sample is 1,000 |
| Personal data appears in logs or test fixtures | Low | High | Logging rules; synthetic names and numbers only; log check in §6 |
| Phone formats differ between entries, so duplicates are missed | Medium | Low | Decision D-M2-2; the warning is a convenience, not a guarantee |

## 8. Completion criteria for Module 2

- Steps 1 to 10 are committed on `development`, each building and passing tests.
- All tests in §2 pass, and the test names carry their IDs.
- Verification in §6 is recorded with exact results, including the timing numbers and the browser version.
- No feature beyond patient management and top-bar search is built.
- Decisions D-M2-1 to D-M2-6 are confirmed or changed by the Project Owner.
