---
name: pms-module-consultation
description: Patient Management System module agent for the consultation workflow: mandatory vitals (temperature, BP, pulse), complaints, and diagnosis notes (FR-VIT-001, FR-CMP-001, FR-DIA-001). Derived from the implementation and brainstorming agents. Use for any work in the consultations feature. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Consultation

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. This module carries the success criterion SC-001 (a consultation record in 2 to 3 minutes), so UI speed is a requirement here.

## Requirements Owned

| ID | Requirement (BRD, Consultation Workflow) |
|---|---|
| FR-VIT-001 | Record temperature, blood pressure, and pulse for every consultation (mandatory) |
| FR-CMP-001 | Enter patient complaints as free text |
| FR-DIA-001 | Record diagnosis notes |

## Scope in Code

- Frontend: `Frontend/src/app/consultations/` (consultation page with vitals, complaints, diagnosis sections; keyboard-first entry).
- Backend: `Application` consultation service; `Domain` consultation and vitals value objects; `Infrastructure` repository.
- Database: `Consultations` (linked to `Appointments` and `Patients`), `ConsultationVitals` or columns on `Consultations`, `Complaints` and `Diagnoses` as text fields with audit columns.

## Rules

- Vitals are mandatory. Save is blocked until temperature, systolic and diastolic BP, and pulse are entered. Enforce on server as well as UI.
- Validate ranges for vitals to catch entry errors, and show a confirmation prompt for values outside a normal range rather than blocking. The range table is recorded in the plan and reviewed by the physician.
- Complaints and diagnosis are free text. Do not auto-suggest diagnoses (AI diagnosis is out of scope).
- Each consultation is saved as a visit that stays in the patient history (FR-HIS).
- Tab order and keyboard shortcuts for vitals, complaint, and diagnosis are tested for SC-001.

## Test Focus

- Unit: vitals validation (required, numeric, range warnings), save guard.
- Integration: create consultation endpoint rejects missing vitals with `ProblemDetails`.
- End-to-end: full consultation in under 3 minutes on the test machine, measured and recorded in the verification report.
- Accessibility: keyboard-only completion of the consultation form.

## Privacy

- Complaints and diagnosis are clinical free text. Never copy them into logs, analytics events, or test output.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Prescription links go to `pms-module-prescriptions`. History display goes to `pms-module-history-search`.
