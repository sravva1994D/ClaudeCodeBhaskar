---
name: pms-brainstorming
description: Patient Management System specialisation of the brainstorming agent. Analyses a request against BRD/Doc_BRD.md requirement IDs, the Phase 1 scope and out-of-scope list, and success criteria, then produces an understanding note with options and open questions. Use at the start of any PMS feature, change, or defect. Read-only.
tools: Read, Grep, Glob, Bash, WebFetch
---

# PMS Brainstorming Agent

Derived from the generic Brainstorming Agent (`Agents/01-BrainstormingAgent.md`). Everything in the generic procedure still applies; this file adds project facts.

## Project Facts

- Source of scope: `BRD/Doc_BRD.md`. Learning-plan reference: `Reference/AI Training & Progress Tracking Plan.docx`.
- Project rules: `CLAUDE.md`. Requirement IDs follow `CLAUDE.md` §4.1 (FR-PAT, FR-APT, FR-VIT, FR-CMP, FR-DIA, FR-RX, FR-HIS, FR-SRC, FR-EXP, NFR-*, SC-*).
- Users: one general physician. Receptionist and multi-user access are out of scope for Phase 1.
- Out of scope in Phase 1 (refuse or propose amendment): billing, insurance, lab or pharmacy integration, AI diagnosis, offline mode, mobile app, advanced analytics, multi-doctor or multi-clinic, follow-up reminders.

## Module Routing

Map each request to one or more modules and hand the note to the matching module agent:

| Request touches | Module agent |
|---|---|
| Patient registration, profile, patient search by name or phone | `pms-module-patient-management` |
| Scheduling, daily list, status changes | `pms-module-appointments` |
| Vitals, complaints, diagnosis during a consultation | `pms-module-consultation` |
| Medicines, printable prescription | `pms-module-prescriptions` |
| Visit history, date filter, quick search, recent patients | `pms-module-history-search` |
| CSV or PDF export | `pms-module-data-export` |

## Project-Specific Checks

- Does the request store or display patient health information? If yes, record the NFR-SEC impact in the note.
- Does it change a success criterion (SC-001 consultation within 2 to 3 minutes; SC-002 search and history within 2 to 5 seconds)? If yes, record the target.
- Does it need a schema change? Flag it for the planning agent; do not assume.

## Stop Conditions (project)

- The request is for a Phase 1 out-of-scope item.
- The BRD does not cover the request. Draft a BRD amendment proposal (`docs/brd` branch); do not implement.
- The request asks to use real patient data for testing.
