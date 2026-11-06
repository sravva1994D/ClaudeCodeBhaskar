---
name: pms-module-appointments
description: Patient Management System module agent for appointment scheduling, the daily appointment list, and status updates (Scheduled, Completed, Cancelled, No-show) (FR-APT-001 to FR-APT-003). Holds the module's acceptance criteria, proposed API surface, and data rules. Derived from the implementation and brainstorming agents. Use for any work in the appointments feature. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Appointments

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD, Appointment Management) |
|---|---|
| FR-APT-001 | Schedule appointments |
| FR-APT-002 | View daily appointment list |
| FR-APT-003 | Update status to Scheduled, Completed, Cancelled, or No-show |

## Acceptance Criteria

| ID | Given | When | Then |
|---|---|---|---|
| FR-APT-001-AC-01 | An existing patient | An appointment is scheduled with a date and time | It is saved with status Scheduled |
| FR-APT-001-AC-02 | A patient ID that does not exist | An appointment is scheduled | The request is rejected |
| FR-APT-002-AC-01 | Appointments exist for today | The daily list is opened | Today's appointments are shown, ordered by time |
| FR-APT-002-AC-02 | Appointments exist for another day | That date is selected | That day's appointments are shown (*proposed, owner to confirm*) |
| FR-APT-003-AC-01 | A Scheduled appointment | Its status is set to Completed, Cancelled, or No-show | The status is stored |
| FR-APT-003-AC-02 | A status outside the four BRD values | It is set | The request is rejected |
| FR-APT-003-AC-03 | A Completed, Cancelled, or No-show appointment | A different status is requested (*proposed, owner to confirm*) | The change is rejected |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `POST /api/v1/appointments` | Schedule | FR-APT-001 |
| `GET /api/v1/appointments?date=` | Daily list, defaults to today | FR-APT-002 |
| `PATCH /api/v1/appointments/{id}/status` | Update status | FR-APT-003 |

## Data (planning input)

- `Appointments`: `Id`, `PatientId`, date and time (UTC), status (enum of the four values), audit fields.
- Index: visit date, to serve the daily list.

## Scope in Code

- Frontend: `Frontend/src/app/appointments/` (scheduling form, daily list, status actions).
- Backend: `Application` appointment services; `Domain` appointment entity and status enum; `Infrastructure` repository.
- Database: `Appointments` table as above.

## Rules

- Status is an enum with exactly four values from FR-APT-003. No other values.
- Allowed transitions are defined in the Domain layer and tested. Default: Scheduled to Completed, Cancelled, or No-show. Completed, Cancelled, and No-show are final unless the owner records an exception.
- The daily list is ordered by time and defaults to today.
- An appointment cannot be scheduled for a patient who does not exist (FR-PAT).
- Changing an appointment's status does not create or alter a consultation record.

## Test Focus

- Unit: status transitions, time-slot validation, daily list ordering.
- Integration: schedule, list, and status-update endpoints; rejection of invalid transitions; index use for the daily list.
- End-to-end: book an appointment, see it in today's list, mark it Completed.
- Test names include criterion IDs, for example `FR_APT_003_AC_03_no_show_cannot_change_to_completed`.

## Validation

- Automated tests validate every criterion above. No manual validation is required for this module.

## Privacy

- Appointment lists show names only where the screen needs them. Do not log patient names with appointment events.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. A link from an appointment to a consultation goes to `pms-module-consultation`.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs are unchanged. The acceptance criteria, API surface, data notes, and validation section are added.
