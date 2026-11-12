# Module 9 Acceptance Record: Phase 1 Acceptance

**Status:** Manual pass complete (15 of 15 Pass, 2026-11-12). Known limits accepted by the Project Owner. Acceptance of the 26 IDs awaits the Project Owner's sign-off in §7.
**Plan:** `18-Module-9-Plan.md`. Work packages WP-10.1 (acceptance script run) and WP-10.2 (acceptance record).
**Scope:** the 26 in-scope requirement IDs after DEC-025 and DEC-027 (`../01-Brainstorming/12-Requirement-Traceability-Matrix.md`).

## 1. Automated run (step 2)

Branch `pms-Implementation`, commit before this record: `a71158a`. Build: 0 warnings, 0 errors.

| Suite | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| Unit (`PatientManagement.UnitTests`) | 382 | 0 | 0 | 382 |
| Integration (`PatientManagement.IntegrationTests`) | 304 | 0 | 0 | 304 |

Coverage check: 23 of 26 in-scope IDs have automated tests with the ID in the test name. The three without are NFR-PRF-001, SC-004 and SC-005; their evidence is in §2.

## 2. Acceptance table (26 in-scope IDs)

"Auto" means the automated tests in §1 passed. "Manual" means the Chrome step in §3 passed. Test names are examples; the full list is in the test classes named.

| Req ID | Priority | Scenario (acceptance criterion) | Automated evidence (class, example test) | Manual step | Result |
|---|---|---|---|---|---|
| FR-PAT-001 | Must | Register a patient with all fields; code PT-nnnnnn; duplicate warning | `Data/PatientPersistenceTests` (`FR_PAT_001_code_has_the_pt_format_with_six_digits`); `Patients/PatientPagesTests` (`FR_PAT_001_possible_duplicate_shows_the_warning_and_saves_nothing`) | M2 | Pass (Auto and Manual) |
| FR-PAT-002 | Must | Edit a patient; change shows on profile | `Patients/PatientPagesTests` (`FR_PAT_002_edit_saves_the_change_and_the_profile_shows_it`); `Data/PatientRepositoryTests` | M3 | Pass (Auto and Manual) |
| FR-PAT-003 | Must | Profile shows every field and age | `Patients/PatientPagesTests` (`FR_PAT_003_profile_shows_every_field_and_the_age`); `Services/AgeCalculatorTests` | M3 | Pass (Auto and Manual) |
| FR-PAT-004 | Must | Search by name part and phone part; no-match message | `Data/PatientRepositoryTests` (`FR_PAT_004_search_by_phone_part_finds_matching_patients`); `Patients/PatientSearchTests` | M3 | Pass (Auto and Manual) |
| FR-APT-001 | Must | Schedule for a registered patient; overlap refused; appears in list | `Appointments/AppointmentPagesTests` (`FR_APT_001_a_slot_that_is_already_booked_is_refused_on_the_form`) | M5 | Pass (Auto and Manual) |
| FR-APT-002 | Must | Daily list shows only the chosen day; archived hidden | `Appointments/AppointmentDailyTests` (`FR_APT_002_daily_list_shows_only_the_chosen_day`, `FR_APT_002_archived_appointments_are_hidden_from_the_list`) | M7 | Pass (Auto and Manual) |
| FR-APT-003 | Must | Allowed moves only; disallowed refused; archive | `Appointments/AppointmentDailyTests` (`FR_APT_003_a_disallowed_status_change_is_refused_and_not_saved`); `Services/AppointmentServiceTests` | M7 | Pass (Auto and Manual) |
| FR-APT-004 | Should | Reschedule a Scheduled appointment to a free slot; old slot freed | `Appointments/ReschedulePagesTests` (`FR_APT_004_moving_saves_the_new_time_and_frees_the_old_slot`); `Services/AppointmentServiceTests` (FR_APT_004 tests) | M6 | Pass (Auto and Manual) |
| FR-VIT-001 | Must | Record vitals; missing vitals notice; out-of-range confirmation | `Consultations/ConsultationPagesTests` (`FR_VIT_001_out_of_range_vitals_need_confirmation_on_the_screen`, `FR_VIT_001_missing_vitals_are_saved_with_a_notice`) | M8 | Pass (Auto and Manual) |
| FR-CMP-001 | Must | Complaints saved and shown in visit detail | `Visits/VisitPagesTests` (`FR_CMP_001_complaints_are_shown_in_the_visit_detail`); `Services/ConsultationServiceTests` | M8, M10 | Pass (Auto and Manual) |
| FR-DIA-001 | Must | Diagnosis saved and shown in visit detail | `Visits/VisitPagesTests` (`FR_DIA_001_diagnosis_is_shown_in_the_visit_detail`) | M8, M10 | Pass (Auto and Manual) |
| FR-RX-001 | Must | Medicine lines with name required; other fields optional | `Data/PrescriptionPersistenceTests` (`FR_RX_001_a_medicine_line_needs_a_name`, `FR_RX_001_optional_medicine_fields_may_be_empty`) | M8 | Pass (Auto and Manual) |
| FR-RX-002 | Must | Printable prescription with all elements; print marks printed; reprint allowed | `Prescriptions/PrescriptionPrintTests` (`FR_RX_002_the_print_view_shows_the_header_patient_vitals_diagnosis_and_medicines`, `FR_RX_002_printing_again_is_allowed_and_keeps_the_first_print_time`); `Clinic/ClinicProfilePagesTests` | M11, M14 | Pass (Auto and Manual) |
| FR-HIS-001 | Must | All previous visits listed, newest first | `Visits/VisitPagesTests` (`FR_HIS_001_visit_history_lists_the_patients_visits_newest_first`); `Data/ConsultationPersistenceTests` | M8, M9 | Pass (Auto and Manual) |
| FR-HIS-002 | Must | Visit detail shows vitals, complaints, diagnosis, medicines | `Visits/VisitPagesTests` (`FR_HIS_002_visit_detail_shows_the_vitals_complaints_and_diagnosis`); `Prescriptions/PrescriptionPrintTests` (`FR_HIS_002_visit_detail_shows_the_medicines_and_a_print_link`) | M10 | Pass (Auto and Manual) |
| FR-HIS-003 | Should | Filter by single date and by range; both ends inclusive | `Data/ConsultationRepositoryTests` (`FR_HIS_003_range_includes_both_end_days_whole_and_excludes_the_day_after`); `Visits/VisitPagesTests` (`FR_HIS_003_a_range_shows_only_visits_inside_it_with_both_ends_inclusive`) | M9 | Pass (Auto and Manual) |
| FR-SRC-001 | Must | Quick search from top bar finds a patient | `Patients/PatientSearchTests` (`FR_SRC_001_quick_search_from_the_top_bar_finds_a_patient_by_name`); `Home/HomeDailyListTests` | M3 | Pass (Auto and Manual) |
| FR-SRC-002 | Should | Five most recently visited patients, by last visit | `Data/ConsultationRepositoryTests` (`FR_SRC_002_recent_patients_are_the_five_latest_by_last_visit`, `FR_SRC_002_patient_with_no_visit_is_not_in_the_recent_list`); `Home/HomeDailyListTests` | M4 | Pass (Auto and Manual) |
| FR-SRC-003 | Should | Move between profile and visit history and back | `Visits/VisitPagesTests` (`FR_SRC_003_profile_links_to_visit_history_and_the_history_links_back`); `Patients/PatientPagesTests` (`FR_SRC_003_profile_links_back_to_search`) | M9 | Pass (Auto and Manual) |
| FR-EXP-001 | Must | Export CSV: one row per visit, ISO dates, date range applied | `Data/ConsultationRepositoryTests` (`FR_EXP_001_export_lists_visits_oldest_first_with_vitals_and_medicines_in_order`, `FR_EXP_001_patient_with_no_visits_exports_no_rows`); `Export/ExportPagesTests` | M12 | Pass (Auto and Manual) |
| FR-EXP-002 | Must | Printable export view with data; Save as PDF; range applied | `Export/ExportPagesTests` (`FR_EXP_002_printable_view_shows_the_patient_visit_vitals_and_medicines`, `FR_EXP_002_printable_view_range_shows_only_visits_inside_it`) | M13 | Pass (Auto and Manual) |
| NFR-SEC-001 | Must | No access without sign-in; generic error; one account; sliding timeout | `Account/AccountControllerTests` (`NFR_SEC_001_wrong_password_shows_the_generic_error_and_does_not_sign_in`, `NFR_SEC_001_unknown_username_shows_the_same_error_as_a_wrong_password`) | M1 | Pass (Auto and Manual) |
| NFR-SEC-002 | Must | Password stored as a hash that verifies; transport reviewed | `Security/AspNetPasswordHasherTests` (`NFR_SEC_002_stored_value_is_a_hash_not_the_password`, `NFR_SEC_002_hash_verifies_with_the_original_password`); `Data/UserRepositoryTests`. Transport: configuration review (PDR-21), not run on localhost (L3) | Review (§5) | Pass (Auto); transport reviewed as configuration |
| NFR-PRF-001 | Must | Page load about 2 s on localhost | Browser timing only (PDR-29, no automation). Recorded in `16-Module-8-Review-Package.md`: all ten page types, median under 2 s, Chrome 157.0.8103.52 | None (M8 result) | Pass (M8 timing record) |
| SC-004 | Must | Prescription generates and prints without error | `Prescriptions/PrescriptionPrintTests` (FR-RX-002 tests cover the print view and print command). The print preview and Save as PDF are browser-only | M11 | Pass (Manual) |
| SC-005 | Must | CSV and PDF export complete and open without error | `Export/ExportPagesTests` and `Data/ConsultationRepositoryTests` (FR-EXP tests). Opening the files in a spreadsheet and Save as PDF are browser-only | M12, M13 | Pass (Auto and Manual) |

## 3. Manual Chrome checklist (step 4)

Test data: synthetic only, entered through the UI (D-M9-5). Browser: Chrome 157.0.8103.79 for every run. Result: Pass or Fail, with a note for any Fail.

| # | Check | IDs | Chrome version | Result | Note |
|---|---|---|---|---|---|
| M1 | Sign-in with the valid account. Sign-out. A protected URL opened while signed out redirects to sign-in. Wrong password shows the generic error | NFR-SEC-001 | 157.0.8103.79 | Pass | |
| M2 | Register a patient with all fields. Enter an existing phone: warning shown; "Confirm and save" saves | FR-PAT-001 | 157.0.8103.79 | Pass | |
| M3 | Search by name part and by phone part. Open the profile. Edit one field and see it on the profile. Top-bar quick search finds the patient | FR-PAT-002, 003, 004; FR-SRC-001 | 157.0.8103.79 | Pass | |
| M4 | After a visit, the patient appears in "recent patients" on the home page, with the latest first | FR-SRC-002 | 157.0.8103.79 | Pass | |
| M5 | Schedule an appointment. Booked slot cannot be chosen. Past slots today are shown and bookable (DEC-028) | FR-APT-001 | 157.0.8103.79 | Pass | |
| M6 | Reschedule a Scheduled appointment to a free slot. The old slot is free afterwards. A Checked-In appointment has no Reschedule button | FR-APT-004 | 157.0.8103.79 | Pass | |
| M7 | Daily list: allowed status change saves; a disallowed one is refused. Archive hides the row | FR-APT-002, 003 | 157.0.8103.79 | Pass | |
| M8 | Consultation: out-of-range vitals ask for confirmation; complaints, diagnosis and two medicines save; Finish completes the visit | FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-RX-001, FR-HIS-001 | 157.0.8103.79 | Pass | |
| M9 | Visit history lists visits newest first. Single-date and range filters work with both ends included. Profile and history links go back and forth | FR-HIS-001, FR-HIS-003, FR-SRC-003 | 157.0.8103.79 | Pass | |
| M10 | Visit detail shows vitals, complaints, diagnosis and medicines. A visit with no vitals says so | FR-HIS-002, FR-CMP-001, FR-DIA-001 | 157.0.8103.79 | Pass | |
| M11 | Prescription print view shows clinic details, patient, vitals, diagnosis, medicines and footer. Print, then Save as PDF in the print dialog. The file opens | FR-RX-002, SC-004 | 157.0.8103.79 | Pass | |
| M12 | Export form: CSV downloads with the date range applied. It opens in a spreadsheet tool: one row per visit, ISO dates (yyyy-MM-dd), the free text with a comma, quote or line break intact, the guarded `=` cell shown as text | FR-EXP-001, SC-005 | 157.0.8103.79 | Pass | Spreadsheet tool not recorded |
| M13 | Export printable view: range applied; Print then Save as PDF; the PDF opens | FR-EXP-002, SC-005 | 157.0.8103.79 | Pass | |
| M14 | Clinic details page saves; the print view then shows them | FR-RX-002 | 157.0.8103.79 | Pass | |
| M15 | DataTables on the four list screens (search, sort, page size, paging) and breadcrumbs work. Supporting check for DEC-026, not a requirement ID | DEC-026 | 157.0.8103.79 | Pass | |

## 4. Defects

Recorded as: ID, test or step, steps, expected, actual, severity (`../02-Planning/14-Testing-Strategy.md` §6).

No defects were logged in the manual pass.

| # | Requirement ID | Step | Severity | Expected | Actual | Disposition |
|---|---|---|---|---|---|---|
| | | | | | | None |

## 5. Known limits and skipped checks

Not defects. The Project Owner decided on 2026-11-12 to accept all of them (accept all).

| # | Item | Source | Owner decision |
|---|---|---|---|
| L1 | Native date fields show the browser's own date format; the application cannot force dd-MMM-yyyy inside them. The date beside the field uses the approved format | `17-UI-Modernization-Review-Package.md` §3, open item 1 | Accepted |
| L2 | No list screens for all patients, consultations, medicines or prescriptions (no BRD requirement for them) | `17-` §2 and open item 2 | Accepted |
| L3 | Transport (HTTPS beyond localhost) is reviewed as configuration, not run on localhost | PDR-21; NFR-SEC-002 | Accepted |
| L4 | Owner skipped M8 checks: search and history timing, consultation rehearsal (SC-001 is out of Phase 1 scope), usability walkthrough, browser matrix, volume check | `16-Module-8-Review-Package.md`; DEC-025 | Accepted (skipped by owner decision) |
| L5 | Open UI recommendations from the DEC-026 review (duplicate search box on patient search; patient names in daily action buttons; unused palette tokens; clearer CSV wording; console warning if DataTables fails to load) | `17-` review | Accepted (not built) |
| L6 | Older open items: Module 5 review §6 items 2 and 8; Module 3 item 7 (appointment archive restore); HTTPS note in older review packages; optional Low fixes from Modules 6 and 7 | Earlier review packages | Accepted (not built) |
| L7 | Culture test for dd-MMM-yyyy not written (in-process tests cannot set request culture) | Commit `11f5fe4` | Accepted |

## 6. Out of scope for acceptance (not accepted in Phase 1)

| IDs | Status | Reason |
|---|---|---|
| NFR-REL-001, NFR-REL-002, SC-003, SC-006 | Deferred | OOS-011, OOS-014 |
| NFR-USB-001, NFR-PRF-002, NFR-SCL-001, NFR-CMP-001, SC-001, SC-002 | Removed from Phase 1 scope | DEC-025 (2026-11-12) |

Note: `Performance/SearchPerformanceTests.cs` tests NFR-PRF-002, which is out of scope. It passes, and its result is not used as acceptance evidence.

## 7. Sign-off

| Item | Entry |
|---|---|
| Automated run recorded (§1) | Yes: unit 382 and integration 304 passed, build 0 warnings, 0 errors |
| Manual checklist complete (§3); every row has a result | Yes: 15 of 15 Pass, Chrome 157.0.8103.79 |
| Defects dispositioned (§4) | None logged |
| Known limits decided (§5) | Yes: all accepted (2026-11-12) |
| Project Owner acceptance of the 26 IDs (D-M9-7) | Pending: owner to confirm |
| Date of manual pass | 2026-11-12 |
