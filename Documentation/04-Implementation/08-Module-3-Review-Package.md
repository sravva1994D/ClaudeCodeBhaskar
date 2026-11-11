# 08 — Module 3 Review Package (Appointments)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M3 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/07-Module-3-Plan.md` (approved 2026-11-11 with decisions D-M3-1 to D-M3-6)
**Status:** **Approved by the Project Owner, 2026-11-11.** Automated tests pass. Browser checks passed on Chrome 157.0.8103.50. Items 5 and 7 in §6 remain open.

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 3 plan, approved with recommendations | `7a22f78` |
| 1 | Appointment entity, configuration, slot index, migration `AddAppointments` | `09debc2` |
| 2 | Appointment repository: daily list, overlap check, add, update | `ade5040` |
| 3 | Slot grid, status lifecycle table, appointment validator | `05b0047` |
| 4 | Appointment service, DTOs and mapper; slot-taken translation | `4d8d5e0` |
| 5 | Schedule screen (SCR-09); links from profile and search | `af7d751` |
| 6 | Daily list (SCR-10); status change and archive | `e6ac039` |
| 7 | Home page daily list (SCR-03); Appointments link in the top bar | `7abad6d` |
| 8 | Close-out: test run, traceability, design corrections, this review | this commit |

Out of scope for Module 3, as planned: Start and Finish consultation (M4, which set In Consultation and Completed), recent patients on the home page (M6), export links (M7).

## 2. Requirements Covered

| Requirement | Result | Tests (named in the test code) |
|---|---|---|
| FR-APT-001 Schedule appointment on the slot grid, no overlap | Implemented | TC-FR-APT-001-1 (schedule saves a scheduled 30-minute appointment): `AppointmentServiceTests.FR_APT_001_schedule_saves_a_scheduled_thirty_minute_appointment`; `AppointmentPagesTests.FR_APT_001_scheduling_saves_the_appointment_and_opens_the_profile_with_a_message`. TC-FR-APT-001-2 (overlap rejected, nothing saved): `AppointmentServiceTests.FR_APT_001_overlap_is_rejected_and_nothing_is_saved`; `AppointmentPagesTests.FR_APT_001_a_slot_that_is_already_booked_is_refused_on_the_form`; `AppointmentRepositoryTests.FR_APT_001_a_second_live_appointment_in_the_same_slot_raises_slot_taken`. TC-FR-APT-001-3 (unregistered patient rejected): `AppointmentServiceTests.FR_APT_001_unregistered_patient_is_rejected`; `AppointmentPagesTests.FR_APT_001_an_unregistered_patient_gets_not_found_on_the_screen` |
| FR-APT-002 Daily list | Implemented | TC-FR-APT-002-1 (only that day): `AppointmentRepositoryTests.FR_APT_002_daily_list_shows_only_the_chosen_day_in_time_order`; `AppointmentDailyTests.FR_APT_002_daily_list_shows_only_the_chosen_day`; `HomeDailyListTests.FR_APT_002_home_does_not_show_other_days`. TC-FR-APT-002-2 (archived hidden): `AppointmentRepositoryTests.FR_APT_002_daily_list_hides_archived_appointments`; `AppointmentDailyTests.FR_APT_002_archived_appointments_are_hidden_from_the_list`; `HomeDailyListTests.FR_APT_002_home_does_not_show_archived_appointments` |
| FR-APT-003 Lifecycle, allowed moves only, manual archive | Implemented for M3 scope | TC-FR-APT-003-1 (each allowed move): `AppointmentLifecycleTests.FR_APT_003_each_allowed_manual_move_is_accepted`; `AppointmentServiceTests.FR_APT_003_an_allowed_status_change_is_saved`. TC-FR-APT-003-2 (each disallowed move, including reversal and skipped states): `AppointmentLifecycleTests.FR_APT_003_manual_change_is_allowed_only_for_the_listed_moves` (all 36 pairs); `AppointmentLifecycleTests.FR_APT_003_skipped_and_reversed_moves_are_blocked`; `AppointmentLifecycleTests.FR_APT_003_reversal_out_of_a_terminal_status_is_blocked`; `AppointmentServiceTests.FR_APT_003_a_disallowed_status_change_is_rejected_and_not_saved`. TC-FR-APT-003-4 (archive hides, status kept): `AppointmentServiceTests.FR_APT_003_archive_hides_a_completed_appointment_and_keeps_its_status`; `AppointmentDailyTests.FR_APT_003_archive_hides_a_completed_appointment_and_keeps_its_status` |
| FR-APT-003 TC-FR-APT-003-3 (consultation start sets In Consultation) | **Not in M3.** Module 4 | — |
| FR-APT-003 TC-FR-APT-003-5 (Finish sets Completed) | **Not in M3.** Module 4 | — |
| FR-SRC-001 Quick search (home page composition) | Top bar still present on the home page | `HomeDailyListTests.FR_SRC_001_home_has_the_quick_search_in_the_top_bar` |
| Supporting: sign-in and anti-forgery on all new screens | Enforced | `AppointmentPagesTests.FR_APT_001_the_schedule_screen_needs_sign_in`; `AppointmentPagesTests.FR_APT_001_a_post_without_an_anti_forgery_token_is_refused`; `AppointmentDailyTests.FR_APT_002_the_daily_list_needs_sign_in`; `AppointmentDailyTests.FR_APT_003_a_status_change_without_an_anti_forgery_token_is_refused` |

Timing (NFR-PRF-002) is not measured for Module 3. The search timing from Module 2 is unaffected.

## 3. Test Results

Final run, 2026-11-11, after the step 8 documentation changes (no code change in this step).

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings (checked at the end of step 7) |
| Unit tests | 248 passed, 0 failed |
| Integration tests | 147 passed, 0 failed |

Module 2 counts were 95 unit and 89 integration. Module 3 added 153 unit and 58 integration tests.

## 4. Browser Checks (for the Project Owner)

The migration is not yet applied to the development database. Apply it once, then run the app:

```
dotnet ef database update --project src/PatientManagement.Data --startup-project src/PatientManagement.Web
```

| # | Check | Expected | Result |
|---|---|---|---|
| 1 | Sign in; home page shows "Today's appointments" and "New patient" | Page loads; list empty or as expected | Passed (Project Owner, Chrome 157.0.8103.50) |
| 2 | Open a patient profile → "Schedule appointment"; choose tomorrow; pick 09:00; Save | Profile opens with "Appointment booked for …" | Passed (Project Owner, Chrome 157.0.8103.50) |
| 3 | Schedule a second patient at the same slot | Form shows "That slot is already booked"; the slot is shown as "(booked)" and cannot be chosen | Passed (Project Owner, Chrome 157.0.8103.50) |
| 4 | Pick yesterday as the day, click "Show slots", pick a slot, Save | "The appointment date cannot be in the past." | Passed (Project Owner, Chrome 157.0.8103.50). First attempt booked today's 11:30 slot because the day box was changed without "Show slots"; the form now refuses that (see §7) |
| 5 | Submit with no slot chosen | "Choose a slot." | Passed (Project Owner, Chrome 157.0.8103.50) |
| 6 | Daily list for tomorrow: "Check in" on the booking | Message "Appointment status set to Checked-In."; status shown | Passed (Project Owner, Chrome 157.0.8103.50) |
| 7 | On the Checked-In row | Offers Cancel, and no status that skips a step | Passed (Project Owner, Chrome 157.0.8103.50) |
| 8 | Cancel a different booking; then book that slot for another patient | The slot is free after the cancel and can be booked | Passed (Project Owner, Chrome 157.0.8103.50) |
| 9 | Archive a Cancelled or No-show row | Row disappears from the day list and from the home page; no screen shows it | Passed (Project Owner, Chrome 157.0.8103.50) |
| 10 | Search results: "Schedule" link on a row | Opens the schedule screen for that patient | Passed (Project Owner, Chrome 157.0.8103.50) |
| 11 | Top bar "Appointments" link | Opens the daily list for today | Passed (Project Owner, Chrome 157.0.8103.50) |

Note for check 9: a Completed appointment can only be reached through the consultation Finish action (Module 4), so Archive is checked here with a Cancelled or No-show row.

Record the browser name and version for the run (PDR-28).

## 5. Decisions Taken

| Item | Decision | Reference |
|---|---|---|
| D-M3-1 past bookings | A booking date before today is refused. Earlier slots today stay bookable | Plan §4; `AppointmentValidatorTests.FR_APT_001_an_earlier_slot_today_is_not_rejected_as_past` |
| D-M3-2 manual status moves | Scheduled → Checked-In, Cancelled or No-show; Checked-In → Cancelled. In Consultation and Completed are not manual | Plan §4; `AppointmentLifecycleTests.FR_APT_003_manual_change_is_allowed_only_for_the_listed_moves` |
| D-M3-3 archive eligibility | Completed, Cancelled or No-show only | Plan §4; `AppointmentLifecycleTests.FR_APT_003_only_finished_or_released_appointments_can_be_archived` |
| D-M3-4 entry point | From the patient profile and from each search row. No patient picker | Plan §4; `AppointmentPagesTests.FR_APT_001_search_results_link_to_the_schedule_screen` |
| D-M3-5 confirmation | No confirmation dialog for status changes or archive | Plan §4 |
| D-M3-6 list contents | Time, patient name and code (linked), status and actions; ordered by time; default today | Plan §4; `AppointmentDailyTests.FR_APT_002_the_list_shows_time_patient_code_and_status` |

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | Start and Finish consultation not built | In Consultation and Completed cannot be reached in the UI yet; TC-FR-APT-003-3 and -5 not run | Module 4 |
| 2 | Archive can only be exercised on Cancelled or No-show rows in M3 | Completed rows (the normal archive case) appear only after Module 4 | Covered by tests; browser check 9 uses the other statuses |
| 3 | Race between two bookings | Protected by the unique filtered index (`IX_Appointments_Slot`). Tested with one integration test, not a concurrent load test | Accepted for a single-user local system |
| 4 | Server clock decides "today" and "past" | Local server time; a booking made at midnight may use the old day | Accepted (no UTC, CLAUDE §6) |
| 5 | HTTPS redirect warning in Development (from Module 1) | Log noise on plain HTTP | Project Owner decision still pending |
| 6 | Daily list has no paging | All appointments for a day are shown | Fine at clinic scale; revisit if a day grows large |
| 7 | Archived appointments have no screen to restore them | None in the BRD; archive is one-way in Phase 1 | Raise with the Project Owner if restoring is wanted |
| 8 | Consultation link on appointments | Not built; a checked-in appointment has no "Start consultation" yet | Module 4 |
| 9 | Search timing for the day list not measured | Not in NFR-PRF-002 scope for the day list | Measure with the Module 8 search work if needed |

## 7. Design Documents Corrected in This Step

| Document | Correction |
|---|---|
| `07-Database-Design` | Slot index written with `<>` instead of `IN` (SQL Server filtered indexes do not accept IN); `IX_Appointments_PatientId` added |
| `10-Service-Design` | `ArchiveAsync` returns `Result<AppointmentDto>`; archive rules and not-found behaviour added |
| `11-Repository-Design` | `AddAsync` returns `Task` and raises `SlotTakenException`; `UpdateAsync` added |
| `18-Planning-Traceability-Matrix` | FR-APT-001 to FR-APT-003 test names updated to the implemented tests; TC-003-3 and -5 marked for M4 |
| `06-Entity-Design` | Status count corrected to six (plan commit) |
| Schedule screen (SCR-09) | The day box is posted with the slot; "Show slots" reloads the list without saving; a save whose slot is on another day is refused. Added after browser check 4 (Module 3 review) |

The Business-layer decisions D-M3-1 to D-M3-6 are recorded here and in the plan, not in `17-Planning-Decisions-and-Open-Items`. Add them there if the Project Owner wants them in the decision log.

## 8. Commits

| Commit | Message |
|---|---|
| `7a22f78` | docs(docs): add Module 3 implementation plan |
| `09debc2` | feat(data): add Appointment entity and AddAppointments migration |
| `ade5040` | feat(data): add appointment repository with daily list and overlap check |
| `05b0047` | feat(business): add slot grid, status lifecycle and appointment validator |
| `4d8d5e0` | feat(business): add appointment service and DTOs |
| `af7d751` | feat(web): add schedule appointment screen and links |
| `e6ac039` | feat(web): add daily appointments list with status and archive |
| `7abad6d` | feat(web): add home page daily list and appointments link |
| this commit | docs(docs): record Module 3 review package and traceability |

## 9. Approval

Module 3 approved by the Project Owner on 2026-11-11, with all browser checks in §4 passed (Chrome 157.0.8103.50). Open items at approval: §6 item 5 (HTTPS redirect warning in Development, from Module 1) and §6 item 7 (restoring archived appointments). Both remain open for a later decision. Items 5 and 9 in §6 need a decision or a note at approval.
