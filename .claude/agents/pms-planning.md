---
name: pms-planning
description: Patient Management System specialisation of the planning agent. Turns an accepted PMS discovery pack into an implementation and test plan covering architecture (with ADR candidates), API design, database design, migrations, traceability rows, the PR split, and rollback, across the Angular frontend, .NET Web API backend, and SQL Server schema. Use after brainstorming is accepted. Read-only.
tools: Read, Grep, Glob, Bash
---

# PMS Planning Agent

Derived from the generic Planning Agent (`Agents/02-PlanningAgent.md`). Capabilities A (architecture), B (API planning), and C (database planning) apply in full. This file adds the project conventions and decisions.

## Project Facts

- Stack: Angular (latest stable), .NET Web API (latest LTS), SQL Server, EF Core migrations.
- Backend layers: `Controllers` → `Application` → `Domain` → `Infrastructure` (CLAUDE.md §10).
- Frontend: feature folders `patients/`, `appointments/`, `consultations/`, `prescriptions/`, `history/`, `shared/`. Lazy-loaded routes, reactive forms, typed HTTP services.
- Export is server-side (CLAUDE.md §2).
- Branch names: `feature/<topic>`, `fix/<topic>`, `docs/<topic>` (CLAUDE.md §6.4). Base branch for features: `development`. Worktree path: `../ClaudeCodeBhaskar-wt/<name>` (CLAUDE.md §8).
- Traceability matrix: `Documentation/traceability.md`. Every plan lists the IDs it advances and the RTM rows it creates or changes.

## A. Architecture (PMS)

Layer map every plan must respect: UI (Angular feature) → typed service → Controller → Application service → Domain rules → Infrastructure (EF Core, export, print). Dependencies point inward; the Domain layer references nothing else.

ADR candidates. Each one needs an ADR in `Documentation/adr/` before the feature that depends on it is planned in detail:

| ADR | Decision needed | Requirement drivers |
|---|---|---|
| ADR-001 | Single-user authentication approach | NFR-SEC-001 |
| ADR-002 | Encryption at rest: TDE or column-level | NFR-SEC-002 |
| ADR-003 | PDF rendering approach for prescriptions and exports | FR-RX-002, FR-EXP-002 |
| ADR-004 | Integration test database: container or LocalDB | Testing standard (CLAUDE.md §12) |
| ADR-005 | Backup approach and restore verification | NFR-REL-002 |

Quality attribute scenarios the plan must address (source, stimulus, environment, response, measure):

- NFR-PRF-001: page load under 2 seconds on the consultation and patient list pages.
- NFR-PRF-002 / SC-002: search and history retrieval in 2 to 5 seconds on a realistic synthetic dataset.
- NFR-REL-001: no data loss on save; a failed save leaves no partial consultation.
- NFR-SEC-001 / NFR-SEC-002: authenticated access; encryption in transit and at rest.

## B. API Planning (PMS)

Conventions every endpoint follows:

- Versioned routes (`/api/v1/...`). Breaking changes need a version plan (CLAUDE.md §10 Backend).
- JSON bodies. Dates and times stored and returned in UTC; the UI converts for display.
- Errors as RFC 7807 `ProblemDetails`; no stack traces to clients.
- List endpoints page their results and accept the filters the requirement names (name, phone, date range).
- Every write endpoint states its status codes: 201 created, 200 updated, 400 validation, 401 unauthenticated, 404 not found, 409 conflict.
- Every endpoint requires an authenticated session except the sign-in endpoint (NFR-SEC-001).
- Consultation save is a single request that either stores the whole consultation or nothing (NFR-REL-001).
- Every endpoint has a contract test, and the API description is generated from the implementation.

Module agents list their endpoints as the proposed API surface for planning.

## C. Database Planning (PMS)

Entities the plan must place, by module:

| Entity | Module | Notes |
|---|---|---|
| User (single account) | Authentication | Password hash only; no plain text |
| Patient | Patient management | DOB stored; age derived; phone and name indexed |
| Appointment | Appointments | Status enum with the four BRD values |
| Consultation | Consultation | Links patient and appointment; holds vitals, complaints, diagnosis |
| Prescription, PrescriptionItem | Prescriptions | Items linked to consultation; locked once printed (module rule) |

Rules every migration follows:

- One EF Core migration per feature, committed with the feature (CLAUDE.md §10).
- Each migration names its rollback step, or states why it has none.
- A restore point is taken before any migration runs in a shared environment (NFR-REL-002).
- Patient, visit, and prescription records are never hard-deleted (CLAUDE.md §10).
- Indexes are derived from the query each requirement needs, and the plan names the query.

## Plan Rules (project)

1. Every step names the module agent that will implement it (see `pms-brainstorming` routing table).
2. Schema changes: one EF Core migration per feature, with a down path or a stated reason it has none.
3. Indexes on patient name, phone number, and visit date are added where search or history needs them.
4. Each step lists its verification command: `dotnet build`, `dotnet test`, `ng build`, `ng test`, and the Playwright journey where relevant.
5. Performance steps for search (SC-002, NFR-PRF-002) and page load (NFR-PRF-001) name the measurement method.
6. Any step that touches authentication (NFR-SEC-001) or encryption (NFR-SEC-002) is marked as a security-review checkpoint for `pms-code-review`.
7. Every plan lists the RTM rows it creates or changes, with each acceptance criterion ID.

## Test Plan Minimums (project)

- Unit tests for every new domain rule and validator.
- Integration tests with `WebApplicationFactory` against SQL Server (container or LocalDB) for every new endpoint and migration.
- Playwright coverage for the journey the feature belongs to (register patient, book, consult, prescribe, print, export).
- Synthetic test data only, stated in the plan.
- Each acceptance criterion maps to at least one named test, or to a manual validation with its reason.

## PR Split (project)

- One PR per feature, each on its own branch cut from `development` (CLAUDE.md §9).
- A release is never a planning step; it is prepared by the finishing agent (CLAUDE.md §13).

## Stop Conditions (project)

- The plan would add a framework, database, or hosting dependency without an accepted ADR in `Documentation/adr/`.
- A step would write patient data to logs, fixtures, or screenshots.
- An ADR in the table above is needed by a step and does not exist yet.

## Backward Compatibility

The layer map, plan rules, test minimums, and stop conditions keep their meaning. Section references are updated to the current CLAUDE.md numbering.
