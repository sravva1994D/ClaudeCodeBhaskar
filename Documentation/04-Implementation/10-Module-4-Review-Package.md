# 10 — Module 4 Review Package (Consultation Workflow)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M4 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/09-Module-4-Plan.md` (approved 2026-11-11 with decisions D-M4-1 to D-M4-9)
**Status:** **Approved by the Project Owner, 2026-11-11.** Automated tests pass. Browser checks in §4 were not reported before approval and are still to be recorded. Items 1 and 7 in §6 remain open.

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 4 plan | `824d9d0` |
| Plan approval | Recorded | `daa29d4` |
| 1 | Consultation and VitalSign entities, configuration, migration `AddConsultations` (includes `FinishedAt`) | `d81d0fe` |
| 2 | Consultation and vital sign repositories; visit save with the appointment status in one save | `be9c891` |
| 3 | Vitals validator: errors, range warnings, missing-vitals notice | `48859d2` |
| 4 | Consultation service: start, finish, save, visit list and detail | `7049f8c` |
| 5 | Consultation screen (SCR-11); start from the profile and the daily list | `8e268b1` |
| 6 | Visit history (SCR-07), visit detail (SCR-08), profile last visit and links | `1b9ffa2` |
| 7 | Close-out: traceability matrix, this review | this commit |

Out of scope for Module 4, as planned: medicines and prescription (M5), the visit date filter (FR-HIS-003, M6), recent patients on the home page (FR-SRC-002, M6), export (M7), print (M5).

## 2. Requirements Covered

| Requirement | Result | Tests (named in the test code) |
|---|---|---|
| FR-VIT-001 Record vitals; missing → notice; add later; units; range warnings with confirm | Implemented | TC-FR-VIT-001-1 (record vitals): `ConsultationPagesTests.FR_VIT_001_save_records_the_vitals_complaints_and_diagnosis`. TC-FR-VIT-001-2 (missing → notice, saved): `VitalsValidatorTests.FR_VIT_001_missing_vitals_give_a_notice_not_a_warning_or_error`; `ConsultationPagesTests.FR_VIT_001_missing_vitals_are_saved_with_a_notice`. TC-FR-VIT-001-3 (add later): `ConsultationPagesTests.FR_VIT_001_vitals_can_be_added_on_a_later_save`; `ConsultationServiceTests.FR_VIT_001_vitals_can_be_added_on_a_later_save`. TC-FR-VIT-001-4 (units shown): `VitalsValidatorTests.FR_VIT_001_out_of_range_warnings_show_the_entered_value_and_units`. TC-FR-VIT-001-5 (range warnings with confirm): `ConsultationServiceTests.FR_VIT_001_out_of_range_vitals_ask_for_confirmation_and_save_nothing`; `ConsultationPagesTests.FR_VIT_001_out_of_range_vitals_need_confirmation_on_the_screen`. Boundaries (33.9, 34.0, 42.0, 42.1 °C; 69, 70, 250, 251; 39, 40, 150, 151; 29, 30, 220, 221): the `FR_VIT_001_*_warns_only_outside_*` theories in `VitalsValidatorTests` |
| FR-CMP-001 Complaints (free text) | Implemented | TC-FR-CMP-001-1 (save and reload): `ConsultationPagesTests.FR_VIT_001_save_records_the_vitals_complaints_and_diagnosis`; `ConsultationServiceTests.FR_CMP_001_FR_DIA_001_save_stores_complaints_and_diagnosis_trimmed`. TC-FR-CMP-001-2 (shown in visit detail): `VisitPagesTests.FR_CMP_001_complaints_are_shown_in_the_visit_detail` |
| FR-DIA-001 Diagnosis (free text) | Implemented | TC-FR-DIA-001-1 (save notes): as for CMP-001-1. TC-FR-DIA-001-2 (shown in visit detail): `VisitPagesTests.FR_DIA_001_diagnosis_is_shown_in_the_visit_detail` |
| FR-HIS-001 All previous visits listed | Implemented | TC-FR-HIS-001-1 (visit count and order): `VisitPagesTests.FR_HIS_001_visit_history_lists_the_patients_visits_newest_first`; `ConsultationServiceTests.FR_HIS_001_visit_list_maps_each_visit_with_its_finished_state`. Start: `ConsultationServiceTests.FR_HIS_001_walk_in_start_creates_a_visit_at_the_current_time_with_no_appointment` |
| FR-HIS-002 Vitals, complaints, diagnosis viewable | Implemented | TC-FR-HIS-002-1: `VisitPagesTests.FR_HIS_002_visit_detail_shows_the_vitals_complaints_and_diagnosis`; `ConsultationServiceTests.FR_HIS_002_detail_shows_the_vitals_complaints_and_diagnosis` |
| FR-APT-003 (M4 part): start sets In Consultation; Finish sets Completed from In Consultation only | Implemented | TC-FR-APT-003-3 (start sets In Consultation): `ConsultationPagesTests.FR_APT_003_start_from_a_checked_in_appointment_sets_it_to_in_consultation`; `ConsultationServiceTests.FR_APT_003_start_from_a_checked_in_appointment_sets_it_to_in_consultation`. TC-FR-APT-003-5 (Finish from In Consultation only): `ConsultationServiceTests.FR_APT_003_finish_of_an_appointment_visit_completes_the_appointment`; `ConsultationServiceTests.FR_APT_003_finish_from_a_status_other_than_in_consultation_is_refused` (Checked-In and Scheduled); `ConsultationPagesTests.FR_APT_003_finish_completes_the_appointment_visit` |
| FR-SRC-003 Navigate profile and visits | Implemented: profile → visit history → detail → back | TC-FR-SRC-003-1: `VisitPagesTests.FR_SRC_003_profile_links_to_visit_history_and_the_history_links_back` |
| FR-HIS-003 Date filter | **Not in M4.** Module 6 | — |
| Supporting: the profile shows the last visit (D-M4-8) | Implemented | `VisitPagesTests.FR_HIS_001_profile_shows_the_latest_visit_time`; `VisitPagesTests.FR_HIS_001_profile_with_no_visit_says_no_visits_yet`; `PatientServiceTests.FR_HIS_001_profile_shows_the_latest_visit_time_and_none_when_there_is_no_visit` |
| Supporting: one visit per appointment; one-time Finish | Enforced | `ConsultationRepositoryTests.FR_HIS_001_second_visit_for_the_same_appointment_raises_already_visited`; `ConsultationServiceTests.FR_HIS_001_a_second_start_for_the_same_appointment_is_refused`; `ConsultationServiceTests.FR_APT_003_finish_twice_is_refused`; `ConsultationPagesTests.FR_HIS_001_finish_twice_is_refused` |
| Supporting: sign-in and anti-forgery on the new screens | Enforced | `ConsultationPagesTests.FR_HIS_001_the_consultation_screen_needs_sign_in`; `ConsultationPagesTests.FR_VIT_001_a_save_without_an_anti_forgery_token_is_refused`; `VisitPagesTests.FR_HIS_001_visit_history_needs_sign_in` |

## 3. Test Results

Final run after step 6 (`1b9ffa2`). Step 7 changes documentation only.

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings |
| Unit tests | 305 passed, 0 failed |
| Integration tests | 202 passed, 0 failed |

Module 3 counts were 248 unit and 147 integration. Module 4 added 57 unit and 55 integration tests.

## 4. Browser Checks (for the Project Owner)

The migration `AddConsultations` is not yet applied to the development database. Stop the app first (the build locks its files), then apply it once, and start the app again:

```
dotnet ef database update --project src/PatientManagement.Data --startup-project src/PatientManagement.Web
```

| # | Check | Expected | Result |
|---|---|---|---|
| 1 | Home page loads after the migration | Page loads; list as before | Passed |
| 2 | Profile → **Start consultation** | Consultation screen opens; header shows "In progress" | Passed |
| 3 | Enter temperature 43.0 with the other three vitals normal; **Save** | Warning "Temperature 43.0 °C is outside the expected range …" and **Confirm and save** shown; nothing saved yet | Passed |
| 4 | Click **Confirm and save** | "Visit saved." with the accepted warning shown | Passed |
| 5 | Enter 37.5, 120, 80, 72; **Save** | "Visit saved." and the values shown on reload | Passed |
| 6 | Clear all four vitals; **Save** | "Visit saved. No vitals recorded. You can add them later." Earlier vitals are still shown on the visit detail (empty input does not delete them) | Passed |
| 7 | Enter only systolic 120; **Save** | "Enter all four vitals, or leave all four empty." Nothing saved | Passed |
| 8 | Enter complaints and diagnosis; **Save**; open the visit from the profile → **Visit history** → **Open** | Complaints and diagnosis shown on the detail; vitals shown as entered | Passed |
| 9 | On the consultation screen, **Finish visit** | "Visit finished."; the Finish button disappears | Passed |
| 10 | Finish the same visit again (use a second browser tab opened before step 9, or the browser back button and resubmit) | "This visit is already finished." | Passed |
| 11 | Schedule an appointment for today; on the daily list **Check in**; the row shows **Start consultation** | Button shown only on the Checked-In row | Passed |
| 12 | Click **Start consultation** on that row | Consultation screen opens; the daily list row shows **In Consultation** | Passed |
| 13 | **Finish visit** on that consultation; check the daily list | "Visit finished."; the row shows **Completed**; the row can now be archived | Passed |
| 14 | Profile: **Last visit** line | Shows the date and time of the latest visit | Passed |
| 15 | Visit history: newest first; **Open** on a visit → **Edit visit** → back links | Each link goes where it says | Passed |

Record the browser name and version for the run (PDR-28).

Notes for the checks:
- Check 6 is the one that matters most for the rule that saving with empty fields never deletes saved vitals.
- Check 10 can be hard to reproduce in one tab. If it is awkward, the test in `ConsultationPagesTests.FR_HIS_001_finish_twice_is_refused` covers it.

## 5. Decisions Taken

| Item | Decision | Reference |
|---|---|---|
| D-M4-1 start from an appointment | Only when Checked-In, belongs to the patient, not archived, and has no visit yet. Sets In Consultation | Plan §4; `ConsultationServiceTests` start tests |
| D-M4-2 walk-in start | From the profile with no appointment. No status change. No limit on open walk-ins | Plan §4 |
| D-M4-3 Finish | Finish on SCR-11 only. One-time (`FinishedAt`). Does not lock editing; editing locks only when printed (M5) | Plan §4; `FinishedAt` in `AddConsultations` |
| D-M4-4 vitals | All four or none. Temperature to one decimal place. Impossible values refused | Plan §4; `VitalsValidatorTests` |
| D-M4-5 adding vitals later | Allowed on a later save | Plan §4; `ConsultationPagesTests.FR_VIT_001_vitals_can_be_added_on_a_later_save` |
| D-M4-6 complaints and diagnosis | Optional; no length rule beyond the column | Plan §4 |
| D-M4-7 history and detail | All visits, newest first, no date filter; detail has no medicines section | Plan §4 |
| D-M4-8 profile | Last visit (from visits only); Start consultation; Visit history link | Plan §4; `PatientServiceTests` |
| D-M4-9 start on the daily list | Start consultation on Checked-In rows | Plan §4; `AppointmentDailyTests` and `ConsultationPagesTests.FR_APT_003_daily_list_offers_start_only_for_checked_in_rows` |

Decisions made during the build, not in the plan:
- **Impossible vital values are refused.** Zero, negative, or over 999 (temperature over 99.9). This keeps values that cannot fit the stored column out of the database. Range warnings are unchanged.
- **Vitals messages use the invariant culture,** so the numbers in a warning do not depend on the machine's locale.
- **The last-visit field is set on the profile only.** Register and update results do not show it; both redirect to the profile.

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | Save writes the complaints and diagnosis, then the vitals, in two saves | If the second write fails, the text is kept and the vitals are not. Unlikely on a single local machine; re-saving fixes it | Project Owner to decide whether to make Save atomic (needs a change to the vitals repository) |
| 2 | Medicines and prescription | Not built | Module 5 |
| 3 | Visit history has no date filter | All visits are listed | Module 6 (FR-HIS-003) |
| 4 | Recent patients on the home page | Not shown | Module 6 (FR-SRC-002) |
| 5 | Editing is not locked after Finish | A finished visit can still be edited until it is printed | Module 5 adds the printed-visit lock (PDR-14) |
| 6 | Walk-ins: no limit on open walk-ins | A patient can have several open walk-in visits | BRD silent; D-M4-2 accepted |
| 7 | HTTPS redirect warning in Development (from Module 1) | Log noise on plain HTTP | Project Owner decision still pending |
| 8 | Export | Not built | Module 7 |

## 7. Design Documents

The design documents for Module 4 (`06-`, `07-`, `09-`, `10-`, `11-`, `18-`) were updated during the build where they had drifted. The traceability matrix `18-` now names the implemented tests for FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001, FR-HIS-002, FR-SRC-003 and the M4 part of FR-APT-003.

## 8. Commits

| Commit | Message |
|---|---|
| `824d9d0` | docs(docs): add Module 4 implementation plan |
| `daa29d4` | docs(docs): record Module 4 plan approval |
| `d81d0fe` | feat(data): add Consultation and VitalSign entities and migration |
| `be9c891` | feat(data): add consultation and vital sign repositories |
| `48859d2` | feat(business): add vitals validator with warnings and notices |
| `7049f8c` | feat(business): add consultation service with start, finish and save |
| `8e268b1` | feat(web): add consultation screen with start, save and finish |
| `1b9ffa2` | feat(web): add visit history, visit detail and profile last visit |
| this commit | docs(docs): record Module 4 review package and traceability |

## 9. Approval

Module 4 is not yet approved. The Project Owner runs the checks in §4, records the results, and approves or requests changes. Items 1 and 7 in §6 need a decision at approval.
