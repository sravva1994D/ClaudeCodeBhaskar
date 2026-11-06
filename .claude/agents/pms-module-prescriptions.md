---
name: pms-module-prescriptions
description: Patient Management System module agent for medicines and printable prescriptions: name, dosage, frequency, duration, instructions, and the printed layout with clinic header, patient details, vitals, diagnosis, medications, and footer (FR-RX-001, FR-RX-002). Holds the module's acceptance criteria, proposed API surface, print-validation rules, and data rules. Derived from the implementation and brainstorming agents. Use for any work in the prescriptions feature. Stages but never commits.
tools: Read, Write, Edit, Grep, Glob, Bash
---

# Module Agent: Prescriptions

Derived from the generic Implementation Agent (`Agents/04-ImplementationAgent.md`) and Brainstorming Agent, scoped to one BRD module. Prescription accuracy is a patient-safety concern. Treat every change here as high risk. Requirement-driven implementation applies: each acceptance criterion below becomes a named test before the code.

## Requirements Owned

| ID | Requirement (BRD, Medication / Prescription) |
|---|---|
| FR-RX-001 | Add medicines with name, dosage, frequency, duration, instructions |
| FR-RX-002 | Generate a printable prescription with clinic and doctor header, patient details, vitals, diagnosis, medications, and footer (basic notes and signature area) |

Success criterion: SC-004 (smooth generation and printing of prescriptions).

## Acceptance Criteria

| ID | Given | When | Then |
|---|---|---|---|
| FR-RX-001-AC-01 | A consultation | A medicine with name, dosage, frequency, and duration is added and saved | It is stored with the consultation |
| FR-RX-001-AC-02 | A medicine missing one of name, dosage, frequency, or duration | It is saved | The save is rejected and names the missing field |
| FR-RX-001-AC-03 | A medicine with instructions left blank | It is saved | It is saved; instructions are optional |
| FR-RX-002-AC-01 | A saved prescription | It is printed | The output contains the clinic and doctor header, patient details, vitals, diagnosis, medications, and footer with the signature area |
| FR-RX-002-AC-02 | A saved prescription | It is printed | Every printed value equals the stored value, with no rounding, truncation, or default substitution |
| FR-RX-002-AC-03 | A printed prescription | The consultation is later edited | The printed copy is unchanged (locked on print, per the module rule) |
| SC-004-AC-01 | A saved prescription | Print is requested | The print completes without error on the supported browsers (NFR-CMP-001) |

## Proposed API Surface (planning input)

| Method and route | Purpose | Requirement |
|---|---|---|
| `POST /api/v1/consultations/{id}/prescriptions` | Create a prescription with its medicines | FR-RX-001 |
| `GET /api/v1/prescriptions/{id}` | View a prescription | FR-RX-001, FR-HIS-002 |
| `GET /api/v1/prescriptions/{id}/print` | Printable output (PDF) | FR-RX-002 |

## Data (planning input)

- `Prescriptions`: `Id`, `ConsultationId`, print-lock timestamp (UTC), audit fields.
- `PrescriptionItems`: `Id`, `PrescriptionId`, name, dosage, frequency, duration, instructions (optional), audit fields.

## Scope in Code

- Frontend: `Frontend/src/app/prescriptions/` (medicine entry, prescription preview, print view).
- Backend: `Application` prescription service; `Domain` prescription and medicine entities; `Infrastructure` PDF or print rendering.
- Database: `Prescriptions` and `PrescriptionItems` tables as above.

## Rules

- Stored values are printed exactly. No rounding, unit conversion, or truncation of dosage or duration.
- Dosage, frequency, and duration are required. Instructions are optional.
- Clinic and doctor header values come from configuration, not hard-coded in the template.
- The print view reads the saved prescription. It does not re-derive values from the form.
- A prescription is locked once printed, so a later edit does not change a printed copy. The plan records the exact rule (ADR-003 for the rendering approach).
- The footer includes the signature area as a blank space. No signature image is stored by default.

## Test Focus

- Unit: required-field validation, duration and frequency formats, mapping from entity to print model.
- Integration: prescription create and retrieve; print model matches the stored items exactly; print of a locked prescription is unchanged after a consultation edit.
- End-to-end: consult, add two medicines, print, compare printed fields to the saved record.
- Visual check: print layout fits on A4 and Letter with the header and footer intact.
- Test names include criterion IDs, for example `FR_RX_002_AC_02_printed_dose_equals_stored_dose`.

## Validation

- FR-RX-002-AC-01 and the visual check need the physician's sign-off on a synthetic printout. Record the sign-off in the verification report.
- Automated tests validate the other criteria.

## Privacy

- Printed prescriptions contain patient data. Test output uses synthetic patients only. Do not save real printed PDFs in the repository.

## Handoffs

Output goes to `pms-verification` and `pms-gap-analysis`. Any change to the vitals or diagnosis that a prescription prints goes to `pms-module-consultation`.

## Backward Compatibility

Requirements owned, scope, rules, test focus, privacy, and handoffs keep their meaning. The acceptance criteria, API surface, data notes, and physician validation are added.
