# 07 — Module 3 Implementation Plan (Appointments)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 3 (Appointment Management) and the home-page daily list (SCR-03)
**Milestone:** M3 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Status:** Approved 2026-11-11 with all six recommendations (D-M3-1 to D-M3-6) accepted, as part of plan approval. Implementation not started.
**Source rules:** BRD (amended 2026-11-10); CLAUDE.md §3, §4, §6, §7, §8, §9, §10; `Documentation/04-Implementation/05-Module-2-Plan.md` (pattern); `Documentation/02-Planning/19-Appointment-Status-Lifecycle.md`

---

## 1. Scope

| In scope | Requirement IDs | Work packages (`16-WBS`) |
|---|---|---|
| Appointment entity, configuration and migration `AddAppointments` (status string with CHECK, DurationMinutes = 30 CHECK, IsArchived, indexes `IX_Appointments_StartTime` and unique filtered `IX_Appointments_Slot`) | FR-APT-001 | WP-4.1 |
| Slot grid 09:00–13:00 and 17:00–20:00 (14 slots; last slots 12:30 and 19:30) and overlap check | FR-APT-001 (PDR-06, PDR-07) | WP-4.1 |
| Schedule screen (SCR-09, `/Appointments/Create`) | FR-APT-001 | WP-4.2 |
| Daily list (SCR-10, `/Appointments/Daily?date`) with date selector; archived rows hidden | FR-APT-002 | WP-4.3 |
| Status change (`UpdateStatus`) and Archive (route 30) | FR-APT-003 (PDR-08, PDR-09, DEC-022) | WP-4.4 |
| Home page (SCR-03): today's appointments and New patient link | FR-APT-002 | WP-4.5 |

Out of scope, each in its later milestone: Start and Finish consultation (M4), recent patients on the home page (M6), export links (M7). No placeholder links to unbuilt features are shown.

## 2. Requirement mapping

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md` §4) |
|---|---|---|
| FR-APT-001 | AC-1 schedule; AC-2 appears in schedule | TC-FR-APT-001-1 (schedule for registered patient), -2 (overlap rejected), -3 (unregistered patient rejected) |
| FR-APT-002 | AC-1 daily list for chosen day | TC-FR-APT-002-1 (list shows only that day), -2 (archived rows hidden) |
| FR-APT-003 | AC-1 lifecycle statuses; AC-2 shown in list; AC-3 allowed moves only; AC-4 manual archive | TC-FR-APT-003-1 (each allowed move), -2 (each disallowed move, including reversal and Scheduled or Checked-In → Completed), -4 (archive hides appointment). -3 (consultation start) and -5 (Finish) are completed in M4 |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points applied

| Point | Design | Source |
|---|---|---|
| Layers | Controller calls one service; service holds the rules; repository is thin; entities stay in Data and DTOs cross to Web | AD-02, AD-03; `09-` §3 |
| Repository | `IAppointmentRepository`: GetByIdAsync, GetDailyAsync (with patient name and code), HasOverlapAsync, AddAsync, UpdateAsync | `11-` §2.3 |
| Service | `IAppointmentService`: ScheduleAsync, GetDailyAsync, GetSlotsAsync, UpdateStatusAsync, ArchiveAsync | `10-` §2.3 |
| Status model | Six values: Scheduled, Checked-In, In Consultation, Completed, Cancelled, No-show. Transition table in the Business layer, not in the controller or the database | `19-` §2, §6 |
| Slot holding | Scheduled, Checked-In, In Consultation and Completed hold the slot. Cancelled and No-show release it | PDR-07 |
| Overlap | A new appointment may not take a slot held by a non-archived appointment with the same StartTime. The unique filtered index is the database backstop; a violation is mapped to the same overlap message | `07-` §3 (IX_Appointments_Slot); `06-` §2.3 |
| Slot grid | Fixed 30-minute grid; StartTime must match a grid slot | PDR-06 |
| Archive | Separate manual action. Does not change Status. Hides the appointment everywhere | PDR-09, DEC-022 |
| Clock | Injected `TimeProvider`, as in `AgeCalculator`. Times are local clinic time, no UTC | CLAUDE §6, §10 |
| Security | Anti-forgery on every POST; no patient data in logs | CLAUDE §8, §9 |
| Housekeeping | `06-Entity-Design.md` validation table said "four values" for the status; corrected to six in the plan commit | `19-` §2 |

## 4. Decisions

These were not in the BRD or the planning decisions. Each had a recommendation. Approval of this plan accepts them.

| # | Question | Decision | Why |
|---|---|---|---|
| D-M3-1 | Past bookings (BRD is silent) | Reject a date before today. Earlier slots today remain bookable | Keeps the daily list meaningful; late walk-ins are still possible |
| D-M3-2 | Manual status moves in M3 | UpdateStatus allows only Scheduled → Checked-In, Scheduled → Cancelled, Scheduled → No-show and Checked-In → Cancelled. In Consultation and Completed are rejected in M3 and are set by Start and Finish in M4 | PDR-27: Completed only after In Consultation |
| D-M3-3 | Archive eligibility | Only Completed, Cancelled and No-show appointments can be archived | Archiving a live appointment would release its slot silently |
| D-M3-4 | Choosing the patient on SCR-09 | Entry from a "Schedule appointment" link on the patient profile and on each search row. No separate patient picker | Patient must exist first (FR-APT-001) |
| D-M3-5 | Status and Archive confirmation | No confirmation dialog. One click, anti-forgery POST, errors shown on the list | Keeps the screen simple (NFR-USB-001) |
| D-M3-6 | Daily list contents | Time, patient name and code (link to profile), status, row actions. Ordered by time. Default date is today | `05-` SCR-10 |

Note on D-M3-3: the status diagram in `19-` shows no archive rule. The restriction is a design choice made here, recorded so the Project Owner can review it.

## 5. Work steps

Each step builds with 0 warnings, passes its tests, and is committed and pushed to `development`. Each step is reported in chat with its files and message. Work proceeds one step at a time on the Project Owner's go-ahead.

1. **Entity, configuration and migration.** `Appointment` entity, configuration (CHECK on Status and DurationMinutes, unique filtered index, FK restrict), migration `AddAppointments`. Persistence tests. Confirm the filtered index predicate works on SQL Server 2022; if not, use an equivalent predicate.
2. **Repository.** GetById, GetDaily by local date (excludes archived), HasOverlap, Add, Update. Integration tests on `PatientManagement_Test`.
3. **Slot grid and transition table.** Pure Business code: slot grid, valid StartTime check, transition table for all six statuses. Unit tests for every allowed and every blocked move (TC-FR-APT-003-1, -2).
4. **DTOs, mapper and service.** Schedule, slots, daily list, UpdateStatus, Archive. Unit tests with NSubstitute and the injected clock (TC-FR-APT-001-1 to -3, TC-FR-APT-002-1, -2, TC-FR-APT-003-4).
5. **Schedule screen (SCR-09).** `AppointmentsController.Create` GET and POST, form, slot picker, date selector. Links from patient profile and search rows. Web integration tests.
6. **Daily list (SCR-10).** `Daily` GET with date selector, status actions, Archive (route 30). Web integration tests.
7. **Home page.** Today's appointments on SCR-03, New patient link, Appointments link in the layout. Web tests.
8. **Close-out.** Full test run, update `18-Planning-Traceability-Matrix.md` and `12-Requirement-Traceability-Matrix.md`, write `08-Module-3-Review-Package.md` with the browser checks for the Project Owner.

## 6. Verification

- `dotnet build PatientManagement.sln --no-incremental`: 0 errors, 0 warnings.
- Unit and integration suites pass against `PatientManagement_Test`.
- Project Owner runs the migration on the development database and checks in Chrome: schedule a slot; overlap rejected; cancel a slot and rebook it; each allowed status move; disallowed moves rejected; Archive hides an appointment; home page list.

## 7. Risks

| Risk | Handling |
|---|---|
| Two bookings at the same time race past the service check | Unique filtered index is the backstop (step 1) |
| Filtered index predicate differs by SQL Server version | Confirm in step 1 |
| "Past slot today" ambiguity | D-M3-1 |
| Status changes in M3 are a subset of the lifecycle; Start and Finish arrive in M4 | D-M3-2 keeps the M4 transitions out of M3 |

## 8. Completion criteria for Module 3

- Requirements FR-APT-001, FR-APT-002 and FR-APT-003 (the parts in §2) pass their tests, with IDs in the test names.
- Home-page daily list works (FR-APT-002, WP-4.5).
- Traceability matrices updated in the same change.
- Project Owner reviews `08-Module-3-Review-Package.md` and approves.
