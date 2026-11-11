# 11 — Module 5 Review Package (Medication and Prescription)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M5 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/11-Module-5-Plan.md` (approved 2026-11-11; decisions D-M5-1 to D-M5-8)
**Status:** Browser checks passed (§4, 15 of 15, Chrome 157.0.8103.51). Awaiting the Project Owner's approval (§9). Item 8 in §6 is still open.

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 5 plan | `c139e5b` |
| 1 | Prescription, PrescriptionItem and ClinicProfile entities, configurations and migration `AddPrescriptions` | `5264263` |
| 2 | Prescription and clinic profile repositories; replace lines in one transaction; print once | `e498609` |
| 3 | Medicine line rules; prescription service (save, print, print view); clinic profile service; printed lock in the visit save | `74c1c48` |
| 4 | Medicine section on the consultation screen (SCR-11); save route 22; printed lock on the screen | `234f9dd` |
| 5 | Clinic details screen (SCR-14) and menu link | `1bba3e8` |
| 6 | Print view (SCR-12), Print command (route 23), reprint, medicines and print link on visit detail (SCR-08) | `e9bb4e4` |
| 7 | Close-out: traceability matrix `18-`, this review | this commit |

Out of scope for Module 5, as planned: visit date filter (FR-HIS-003, M6), recent patients (FR-SRC-002, M6), export (M7), search timing and browser matrix (M8).

## 2. Requirements Covered

| Requirement | Result | Tests (named in the test code) |
|---|---|---|
| FR-RX-001 Add medicines; only name required; more than one per prescription; at least one before print | Implemented | TC-FR-RX-001-1 (save and read back, in order): `PrescriptionServiceTests.FR_RX_001_saving_lines_saves_them_trimmed_in_order_and_leaves_out_blank_ones`; `MedicinePagesTests.FR_RX_001_saving_medicines_keeps_their_order_and_returns_to_the_visit`; `PrescriptionRepositoryTests.FR_RX_001_lines_are_read_back_in_display_order`. TC-FR-RX-001-2 (name required; blank lines ignored; limits): `PrescriptionServiceTests.FR_RX_001_a_line_without_a_name_saves_nothing`; `MedicinePagesTests.FR_RX_001_a_medicine_without_a_name_is_refused_and_the_typed_values_stay_on_the_page`; `MedicineLineValidatorTests` (boundaries 200/201, 100/101, 500/501; twenty lines allowed, twenty-one refused) |
| FR-RX-002 Printable A4 view with Print; header, patient, vitals, diagnosis, medicines, footer; one per consultation; Print marks printed; edits blocked after print; reprint allowed | Implemented | TC-FR-RX-002-1 (view content): `PrescriptionPrintTests.FR_RX_002_the_print_view_shows_the_header_patient_vitals_diagnosis_and_medicines`; `PrescriptionServiceTests.FR_RX_002_print_view_shows_the_visit_the_patient_and_the_clinic_header`. TC-FR-RX-002-2 (Print marks printed; reprint keeps first time): `PrescriptionPrintTests.FR_RX_002_printing_marks_the_prescription_printed_and_opens_the_print_dialog`; `PrescriptionPrintTests.FR_RX_002_printing_again_is_allowed_and_keeps_the_first_print_time`; `PrescriptionRepositoryTests.FR_RX_002_a_second_print_changes_nothing`. TC-FR-RX-002-3 (edits blocked after print; no print without a medicine): `MedicinePagesTests.FR_RX_002_medicines_cannot_be_saved_once_printed`; `MedicinePagesTests.FR_RX_002_a_printed_visit_cannot_be_saved`; `PrescriptionPrintTests.FR_RX_002_the_print_view_is_not_available_without_a_medicine`; `PrescriptionServiceTests.FR_RX_002_a_visit_without_medicine_cannot_be_printed`. One per consultation: `PrescriptionPersistenceTests.FR_RX_002_only_one_prescription_can_belong_to_a_consultation`; `PrescriptionRepositoryTests.FR_RX_002_a_second_prescription_for_a_consultation_is_refused` |
| FR-RX-002 Clinic header (PDR-15) | Implemented | `ClinicProfilePagesTests` (six tests: sign-in, menu link, trimmed save, single row, length refusal, blank header); `ClinicProfileServiceTests`; `ClinicProfileRepositoryTests`; `ClinicProfilePersistenceTests` |
| FR-HIS-002 Medicines viewable from a visit | Implemented | TC-FR-HIS-002-2: `PrescriptionPrintTests.FR_HIS_002_visit_detail_shows_the_medicines_and_a_print_link`; `PrescriptionPrintTests.FR_HIS_002_visit_detail_without_a_medicine_says_so` |
| SC-004 Prescriptions generate and print without error (A4 view) | Automated part implemented. **Browser part pending** (§4, checks 7 to 11) | Browser check only (PDR-29: no browser automation in Phase 1) |

FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001 and FR-APT-003 were not changed in this module, except that a printed prescription now blocks their visit save (FR-RX-002 AC-4).

## 3. Test Results

Run after step 6 (`e9bb4e4`), then after the review fixes (`5d6f62c`) and the Project Owner changes (`07c5971` and the HTTPS change in this commit).

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings |
| Unit tests | 338 passed, 0 failed (after the D-M5-9 and D-M5-10 changes) |
| Integration tests | 258 passed, 0 failed (after the D-M5-9 and D-M5-10 changes) |

Module 4 counts were 305 unit and 202 integration. Module 5 added 31 unit and 51 integration tests.

## 4. Browser Checks (for the Project Owner)

Before the checks, stop the app (the build locks its files), apply the new migration once, and start the app again. This is the only migration added in Module 5:

```
dotnet ef database update --project src/PatientManagement.Data --startup-project src/PatientManagement.Web
```

Use a patient from Module 4 with a visit that has vitals and a diagnosis, or start a new consultation.

| # | Check | Expected | Result |
|---|---|---|---|
| 1 | Menu → **Clinic details**. Leave all fields blank; **Save** | Refused: "Clinic name is required." and the same for each empty field. Nothing saved. Then fill all five fields and **Save**: "Clinic details saved." | Passed |
| 2 | Open the consultation screen for a visit; in **Medicines**, enter a name only for line 1 and name + dosage for line 2; **Save medicines** | "Medicines saved." Both lines shown in the order entered | Passed |
| 3 | Add a medicine row with **Add medicine**, enter only a dosage, **Save medicines** | "Line N: enter the medicine name." beside that row. Nothing saved; the typed values stay | Passed |
| 4 | **Remove** a row; **Save medicines** | The removed row is gone after the save; the others keep their order | Passed |
| 5 | Enter complaints, diagnosis and vitals; **Save medicines** | Visit values stay as typed (not lost); message "Medicines saved." | Passed |
| 6 | Visit detail for the visit (from the profile → **Visit history** → **Open**) | Medicines listed; "Not printed yet."; **Print prescription** button | Passed |
| 7 | Click **Print prescription** | A4 sheet: clinic header with placeholder logo, patient name and code, date of birth, gender, visit time, vitals, diagnosis, medicines table, doctor name, registration number, signature line. Toolbar hidden in print. Clinic details were saved in full (check 1), so the sheet shows no incomplete notice | Passed |
| 8 | On the sheet, click **Print** | Browser print dialog opens; toolbar line says "Printed on …". Cancel the dialog is fine | Passed |
| 9 | In the print dialog, check Print preview | One A4 page for this data; margins about 15 mm; no toolbar; no page cut through the medicines table | Passed |
| 10 | **Save as PDF** from the print dialog (SC-004). Open the saved PDF | The PDF shows the same content as the sheet | Passed |
| 11 | Click **Print** again on the sheet | Works; "Printed on …" keeps the first time | Passed |
| 12 | Back to the visit (**Back to visit**) | Visit and medicine sections are greyed out; a note says the prescription is printed and the page is read-only; saving has no effect | Passed |
| 13 | Start a walk-in consultation with no medicine. Note its visit number from the address bar, then open `/Prescriptions/Print/<number>` | Redirected back to the visit with "Add at least one medicine before printing." | Passed |
| 14 | Open visit detail for check 13's visit | "No medicines recorded." and no print link | Passed |
| 15 | Visit history shows the printed visit; open it | "Printed on …" shown with the medicines | Passed |

Browser for this run (PDR-28): Chrome 157.0.8103.51. Result line: 15 of 15 checks passed (Project Owner, 2026-11-11).

Notes for the checks:
- Check 8: some browsers block a print dialog that opens without a click. The dialog is opened after the page loads from the Print click. If it does not open, report the browser name and version; the Print button can be clicked again.
- Check 9: the layout was written for A4 but not previewed. Long medicine lists may run onto a second page; report the page count.
- Check 10 is the SC-004 check. A PDF must open and contain the sheet's content.

## 5. Decisions Taken

| Item | Decision | Reference |
|---|---|---|
| D-M5-1 medicines entry | Medicines are on the consultation screen. Saved by **Save medicines**, which replaces the lines in one transaction | Plan §4; `PrescriptionRepository.ReplaceItemsAsync` |
| D-M5-2 blank lines | A line with every field empty is ignored; a line with other fields and no name is an error | Plan §4; `MedicineLineValidatorTests` |
| D-M5-3 line limit and lengths | Twenty lines per prescription; lengths from the database columns | Plan §4; `MedicineLineValidator` |
| D-M5-4 printed lock | After the first print, medicines, complaints, diagnosis and vitals cannot be saved. Finish stays allowed | Plan §4; `ConsultationService.SaveAsync`; `MedicinePagesTests` |
| D-M5-5 print flow | The view is read-only (GET). The Print button posts (marks printed once), shows the same view, and opens the print dialog. Reprint is allowed | Plan §4; `PrescriptionsController`; `PrescriptionPrintTests` |
| D-M5-6 clinic header not complete | **Superseded by D-M5-9.** Was: printing allowed with a notice. The profile row is still created blank on first read, and the print notice still shows for a header never saved | Plan §4; `ClinicProfileService`; `PrescriptionPrintTests` |
| D-M5-9 clinic header required (Project Owner, 2026-11-11) | All five header fields are required on save; a save with any blank field is refused with "<field> is required." Column lengths still apply. Restores the original PDR-15 rule | `ClinicProfileValidator`; `ClinicProfileServiceTests`; `ClinicProfilePagesTests` |
| D-M5-10 HTTPS redirect in Development (Project Owner, 2026-11-11) | The HTTPS redirect is applied outside Development only. Plain HTTP on localhost in Development, as PDR-21 allows; removes the redirect warning | `src/PatientManagement.Web/Program.cs`; PDR-21 amendment in `17-` |
| D-M5-7 logo | Static placeholder image (`wwwroot/img/clinic-logo-placeholder.svg`); no upload | Plan §4 |
| D-M5-8 atomic save | Part 1 built: the medicine replace is one transaction. **Part 2 not built:** the visit text and vitals are still two saves | §6 item 1 |

Decisions made during the build, not in the plan:
- **Locked controls are disabled, not hidden.** When the prescription is printed, the whole consultation form is disabled and a note explains why. The plan said "hide"; disabling keeps the saved values visible.
- **One form for the visit and the medicines.** The "Save medicines" button posts the whole page, so typed visit text is not lost when medicines are saved. The Enter key submits the visit Save (first button on the page).
- **Printed prescriptions show the saved medicines,** not what was typed, even after a refused save.

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | D-M5-8 part 2: the visit text and the vitals are saved in two writes | If the second write fails, the text is kept and the vitals are not. As in Module 4 item 1 | Fixed after code review (§10): the visit text, the vitals and the printed check are one save |
| 2 | Edits to a finished visit are still allowed until print (Module 4 open decision 3) | D-M5-4 keeps Finish open to edits; only print locks | Project Owner to confirm D-M5-4 as the answer to Module 4 decision 3, and record it in `17-` |
| 3 | HTTPS redirect warning in Development (Module 1 item) | Log noise on plain HTTP | **Fixed 2026-11-11** (D-M5-10): the redirect is applied outside Development only |
| 4 | Archive restore for appointments (Module 3 item 7) | Not decided | Project Owner decision still pending |
| 5 | Walk-ins: no limit on open walk-ins (D-M4-2) | A patient can have several open walk-in visits | BRD silent; accepted |
| 6 | Print layout not previewed by the author | Page breaks may differ from the plan | Check 9 |
| 7 | Print dialog on load | Some browsers may block it | Check 8; Print can be clicked again |
| 8 | Latent: `AppointmentsController` UpdateStatus and Archive redirects format the date with the current culture (Module 4 note) | Could fail for non-English locales | Same fix as the Start redirect: invariant date format. Owner to approve |
| 9 | Export and the browser matrix | Not built | Modules 7 and 8 |

## 7. Design Documents

Updated in this module:
- `Documentation/02-Planning/18-Planning-Traceability-Matrix.md`: FR-RX-001, FR-RX-002, FR-HIS-002 (M5 test part) and SC-004 (browser check pending).
- `Documentation/04-Implementation/11-Module-5-Plan.md`: approval status.

Not changed:
- `Documentation/01-Brainstorming/12-Requirement-Traceability-Matrix.md`: lists requirements and their sources, not implementation status, so no change is needed for M5.

## 8. Commits

| Commit | Message |
|---|---|
| `c139e5b` | docs(docs): add Module 5 implementation plan |
| `5264263` | feat(data): add prescription, medicine line and clinic profile tables |
| `e498609` | feat(data): add prescription and clinic profile repositories |
| `74c1c48` | feat(business): add prescription and clinic header services and rules |
| `234f9dd` | feat(web): add the medicine section to the consultation screen |
| `1bba3e8` | feat(web): add the clinic details screen (SCR-14) |
| `e9bb4e4` | feat(web): add the printable prescription and medicines on visit detail |
| this commit | docs(docs): record Module 5 review package and traceability |

## 9. Approval

Module 5 is not yet approved. The Project Owner runs the checks in §4, records the results and the browser, and decides items 2 and 8 in §6. Approval then closes Module 5. The final pull request `development` → `main` waits for Modules 6 to 8.

## 10. Fixes After Code Review

The code review of Module 5 (requested by the Project Owner) raised six findings. All six are fixed in the commit after `eaecf2c`. The browser checks in §4 still apply. Checks 1, 4, 5 and 12 exercise the code changed here. No schema change was made, so no new migration is needed.

| Finding | Fix | Tests |
|---|---|---|
| 1. The printed lock was check-then-act; the visit save could land after a print | `ConsultationRepository.UpdateAsync` now takes the prescription's row lock, refuses the save when it is printed, and writes the visit text and vitals in one transaction. This also closes D-M5-8 part 2 | `ConsultationRepositoryTests.FR_RX_002_a_visit_is_not_saved_once_its_prescription_is_printed`; `ConsultationRepositoryTests.FR_VIT_001_visit_text_and_new_vitals_are_saved_in_one_save`; `VitalSignRepositoryTests` (rewritten on the same save) |
| 2. PDR-15 says the header fields are required; the code allowed blanks | First handled as an amendment (D-M5-6). Reversed on 2026-11-11 by the Project Owner (D-M5-9): all five fields are required on save again | `ClinicProfileServiceTests`; `ClinicProfilePagesTests.FR_RX_002_a_blank_required_field_is_refused_and_nothing_is_saved` |
| 3. Removing every medicine row and then failing a visit save brought the saved medicines back on the page | The saved medicines load only when the page opens or the prescription is printed. A refused save keeps what was on the page | `MedicinePagesTests.FR_RX_001_removed_medicines_stay_removed_after_a_refused_visit_save` |
| 4. Two first reads of the clinic profile could collide and fail with a 500 | `ClinicProfileRepository.GetOrCreateAsync` reads the row, creates it if missing, and reads it again if another request created it first | `ClinicProfileRepositoryTests.FR_RX_002_get_or_create_adds_one_blank_row_and_then_returns_it`; `ClinicProfileRepositoryTests.FR_RX_002_get_or_create_returns_the_row_another_request_saved`; `ClinicProfileServiceTests.FR_RX_002_the_first_read_creates_a_blank_profile` |
| 5. The print step ignored the result of the mark | The mark's result decides: a refused mark is an error only when the prescription is still unprinted | `PrescriptionServiceTests.FR_RX_002_a_print_that_loses_its_lines_to_a_removal_is_refused` |
| 6. Prescriptions depended on the consultation service's constants | Shared codes in `Business/Common/VisitErrors.cs` | Covered by the existing not-found and printed tests |

Also removed: the vitals repository (`IVitalSignRepository`, `VitalSignRepository`), which had no caller after finding 1.

Results after the fixes (clean build, 0 warnings): unit tests 337 passed, integration tests 258 passed, 0 failed.
