# 15 — Module 7 Review Package (Export: CSV and Printable View)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Milestone:** M7 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Plan:** `Documentation/04-Implementation/14-Module-7-Plan.md` (plan approved in chat 2026-11-12; decisions D-M7-1 to D-M7-8 as recommended)
**Status:** **Awaiting Project Owner browser checks (§4) and approval (§7).** Automated checks pass.

---

## 1. Scope Delivered

| Plan step | Work | Commit |
|---|---|---|
| Review fix | Phone number with a leading `+` exported unchanged; other text starting with `+` still guarded (D-M7-3, option b) | this commit |
| Plan step | Work | Commit |
|---|---|---|
| Plan | Module 7 plan (`14-`) | `37a809f` |
| 1 | Data: visit export query (visits oldest first, vitals, medicines in order); day filter shared with the visit list | `b2c97e5` |
| 2 | Business: CSV writer (16 columns, quoting, byte-order mark, CRLF), export service (CSV, print model, form options), visit range rule shared in `VisitDateRange`, formula guard | `75b3432` |
| 3 | Web: export form SCR-13 (From and To, Download CSV), CSV endpoint, refusals shown on the form, Export button on the profile | `9fba29d` |
| 4 | Web: printable view (A4, Print button, Save as PDF hint, newest visit first), Open printable view on the form | `ad06f12` |
| 5 | Close-out: this review package; traceability matrices `18-` and `12-` | this commit |

Out of scope for Module 7, as planned: search timing and browser matrix (M8). No BRD change. No schema change and no migration. No new library; no PDF library (PDR-18).

**Design points that differ from or extend the plan**
- The export query returns visit rows only. The service adds the patient details, so the repository stays thin (plan step 1 note).
- Medicines come from the prescription items of each visit, in one query. The plan's list of repositories (`10-` §4) named `IPrescriptionRepository`; it is not used, because the query needs no prescription rules.
- The visit range rule is one class, `VisitDateRange`, used by the visit list and by the export. Behaviour of the visit list is unchanged; its 274 integration tests still pass.

## 2. Requirements Covered

| Requirement | Result | Tests (named in the test code) |
|---|---|---|
| FR-EXP-001 Export patient data with visits as CSV | Implemented. Browser checks pending (§4) | **TC-FR-EXP-001-1** (opens with expected columns, one row per visit): unit `ExportServiceTests.FR_EXP_001_csv_has_the_sixteen_columns_in_order`, `FR_EXP_001_csv_starts_with_a_byte_order_mark_and_uses_crlf_line_ends`, `FR_EXP_001_csv_row_repeats_the_patient_and_uses_iso_dates`, `FR_EXP_001_csv_lists_visits_oldest_first`; web `ExportPagesTests.FR_EXP_001_csv_downloads_as_utf8_csv_named_with_the_patient_code`, `FR_EXP_001_csv_has_the_header_and_one_row_per_visit`. **TC-FR-EXP-001-2** (date range applied): unit `FR_EXP_001_csv_a_from_date_after_the_to_date_is_refused_by_the_visit_range_rule`, `FR_EXP_001_csv_a_to_date_without_a_from_date_is_refused_by_the_visit_range_rule`; data `ConsultationRepositoryTests.FR_EXP_001_export_range_uses_the_visit_day_rule_and_only_this_patient`; web `FR_EXP_001_csv_range_includes_both_ends_and_leaves_out_other_days`, `FR_EXP_001_a_range_with_no_visits_shows_the_message_on_the_form_and_downloads_nothing`, `FR_EXP_001_a_from_date_after_the_to_date_shows_the_error_and_downloads_nothing`, `FR_EXP_001_a_to_date_without_a_from_date_shows_the_error`, `FR_EXP_001_a_malformed_date_is_refused_and_not_silently_dropped`. **TC-FR-EXP-001-3** (ISO dates; one row for a patient with no visits): unit `FR_EXP_001_patient_with_no_visit_gives_one_row_with_blank_visit_columns`, `FR_EXP_001_csv_leaves_vitals_blank_when_not_recorded`, `FR_EXP_001_numbers_are_not_changed_by_the_formula_guard`; web `FR_EXP_001_csv_for_a_patient_with_no_visits_has_one_row`; data `FR_EXP_001_export_visit_without_vitals_or_prescription_has_blank_parts`, `FR_EXP_001_patient_with_no_visits_exports_no_rows`. Other: medicine cell `FR_EXP_001_csv_medicines_form_one_cell_with_blank_parts_omitted`; quoting `FR_EXP_001_csv_quotes_commas_quotes_and_line_breaks`; formula guard `FR_EXP_001_csv_text_that_starts_with_a_formula_character_is_shown_as_text`; phone rule `FR_EXP_001_phone_number_with_a_leading_plus_is_not_changed`, `FR_EXP_001_plus_text_that_is_not_a_phone_number_is_still_guarded`; file name `FR_EXP_001_csv_file_name_carries_the_patient_code_only`; sign-in and not-found `FR_EXP_001_export_pages_need_sign_in`, `FR_EXP_001_unknown_patient_is_not_found_for_the_form_and_the_csv`; profile link `FR_EXP_001_the_patient_profile_links_to_the_export` |
| FR-EXP-002 Export as PDF (printable view; browser Save as PDF) | Implemented. Browser checks pending (§4) | **TC-FR-EXP-002-1** (printable view shows the data; Print and Save as PDF): web `ExportPagesTests.FR_EXP_002_printable_view_shows_the_patient_visit_vitals_and_medicines`, `FR_EXP_002_printable_view_has_a_print_button_and_the_save_as_pdf_hint`, `FR_EXP_002_printing_the_view_does_not_mark_the_prescription_printed`; unit `ExportServiceTests.FR_EXP_002_print_view_lists_the_newest_visit_first`. **TC-FR-EXP-002-2** (date range applied): web `FR_EXP_002_printable_view_range_shows_only_visits_inside_it`, `FR_EXP_002_printable_view_with_no_visits_in_a_range_shows_the_message_on_the_form`; unit `FR_EXP_002_print_view_with_a_single_day_shows_that_day_as_the_range`, `FR_EXP_002_print_view_with_no_visits_in_a_range_is_refused`; web `FR_EXP_002_printable_view_lists_the_newest_visit_first`, `FR_EXP_002_printable_view_needs_sign_in_and_unknown_patient_is_not_found` |
| SC-005 CSV and PDF export succeeds | Automated checks cover the content and the refusals. Opening the CSV in a spreadsheet and printing or saving the PDF are checked in §4 | §4 checks 3 to 9 and 13 to 15 |

FR-HIS-001, FR-HIS-003, FR-SRC-002 and the visit pages keep their behaviour. Their tests still pass.

## 3. Test Results

Run after the phone-rule review fix, on the code of this commit.

| Check | Result |
|---|---|
| Build (`dotnet build PatientManagement.sln --no-incremental`) | 0 errors, 0 warnings |
| Unit tests | 368 passed, 0 failed (Module 6 end: 345; Module 7 added 23, including 4 for the phone rule) |
| Integration tests | 297 passed, 0 failed (Module 6 end: 274; Module 7 added 23: 4 data, 12 CSV and form, 7 printable view) |

## 4. Browser Checks (for the Project Owner)

Stop the app if it is running, then start it again. Module 7 adds no migration. Use a patient with several visits on different days. At least one visit should have vitals and a prescription with two medicines. Record the browser and version for each run (PDR-28).

**Test data for the checks (use synthetic names only):** one patient with three visits on three days. Visit 1: vitals recorded, two medicines, complaint "Test cough, with fever". Visit 2: no vitals, no prescription, complaint starting with `=` (for example `=1+1`). Visit 3: no vitals, one medicine. A second patient with no visits.

| # | Check | Expected | Result |
|---|---|---|---|
| 1 | Open the patient profile | An "Export" button sits beside "Visit history" | |
| 2 | Click Export | Form shows the patient name and code, From and To fields, Download CSV and Open printable view, and the line "The CSV lists visits oldest first" | |
| 3 | Download CSV with both dates empty | File downloads, named `patient-PT-…-visits.csv`. Opened in a spreadsheet tool: one header row with 16 columns in order, one row per visit (3 rows) | |
| 4 | Look at the dates and times in the CSV | VisitDate as yyyy-MM-dd, VisitTime as HH:mm, DateOfBirth as yyyy-MM-dd; temperature with one decimal | |
| 5 | Look at visit 2 in the CSV | Vitals columns are empty; Medicines cell is empty | |
| 6 | Look at visit 1's Medicines cell | Two medicines separated by " \| ", each as `Name; Dosage; Frequency; Duration; Instructions` with blank parts left out | |
| 7 | Look at visit 2's complaint | The text `=1+1` shows as text in the spreadsheet, not as a result (the guard adds a hidden leading quote) | |
| 8 | Look at the names and special characters | Names, comma in "Test cough, with fever" and any line break keep their text in one cell; accents show correctly (UTF-8) | |
| 9 | Enter From only (the day of visit 2), Download CSV | CSV has only visit 2 | |
| 10 | Enter From and To covering visits 1 and 2 only | CSV has visits 1 and 2 (both ends included); visit 3 is not in it | |
| 11 | Enter From later than To, Download CSV | Red message "From date must not be after To date."; no download; dates stay in the form | |
| 12 | Enter only To, Download CSV | Message "Enter a From date to filter by a range."; no download | |
| 13 | Enter a range with no visits, Download CSV | Message "No visits in this date range."; no download | |
| 14 | Export the second patient (no visits), Download CSV | CSV with one row: patient details, blank visit columns | |
| 15 | Click Open printable view with the range empty | A4 view opens: clinic header, patient details, "All visits, newest first"; visit 3 is first, then 2, then 1 | |
| 16 | In the printable view, check visit 1 | Vitals line shows temperature, blood pressure and pulse; the medicines table lists both medicines with dosage | |
| 17 | Click Print in the printable view | Print dialog opens; in the Chrome print preview the page fits A4, the header and footer are in place, each visit section stays whole | |
| 18 | In the print dialog, choose Save as PDF | A PDF file is produced; open it and check it shows the same content as the preview | |
| 19 | Open printable view with a range (From and To covering visits 1 and 2) | Only visits 1 and 2 appear; header line reads "From … to …" | |
| 20 | Open the consultation of visit 1 after printing the export | The prescription still shows as not printed; export printing changed nothing | |
| 21 | Enter From only with a day that has no visit, Open printable view | Message "No visits in this date range." on the form; no printable view | |
| 22 | Open `/Export/Patient/999999` in the address bar | Not found page | |
| 23 | Sign out, then open the export address of a patient | Login page | |

Owner report: record here the browser and version, and the result of each check (PDR-28).

Browser used: ______. Version: ______. Result: ______.

## 5. Decisions

The plan (`14-`) recorded D-M7-1 to D-M7-8 with recommendations. They were approved with the plan, as recommended:

| # | Point | Decision |
|---|---|---|
| D-M7-1 | Date range rule | Same as the visit filter (From alone is one day; both ends inclusive; To alone and From after To refused). Shared in `VisitDateRange` |
| D-M7-2 | Malformed dates | Refused with a message, never dropped |
| D-M7-3 | CSV formula guard | A text cell starting with `=`, `+`, `-`, `@`, tab or carriage return gets a leading `'`. Decided 2026-11-12 (Project Owner, option b): a value that starts with `+` followed only by digits, spaces, brackets and hyphens is a phone number and is not guarded. Other `+` text stays guarded (for example `+1+1`) |
| D-M7-4 | File name | Patient code only |
| D-M7-5 | Printable view layout | Clinic header, patient block, one section per visit, newest first; printing marks nothing as printed |
| D-M7-6 | Empty result | A range with no visits is refused on the form. A patient with no visits gives one CSV row |
| D-M7-7 | Sort order | CSV oldest first; printable view newest first |
| D-M7-8 | Unfinished visits | Included, as in the visit history |

## 6. Known Limits and Open Items

| # | Item | Effect | Proposed handling |
|---|---|---|---|
| 1 | Phone rule (closed 2026-11-12) | A phone number with a leading `+` is exported unchanged. Other text starting with `+`, `-`, `=`, `@` is still guarded, including a text value with a leading `-` | Closed by the decision in §5 (D-M7-3, option b) |
| 2 | Date inputs look different in each browser | The date picker and its display are set by the browser. The page always sends yyyy-MM-dd | Check in Chrome (§4); Edge in M8 |
| 3 | Print output depends on the printer and its margins | The A4 preview in Chrome is checked; a physical printer may differ | Owner checks a printed copy if a printer is available; M8 browser matrix |
| 4 | Long histories span several printed pages | Each visit section is kept whole where possible; a page header or page numbers are not repeated | Accepted for Phase 1 |
| 5 | Export covers one patient only | As the BRD and the screen catalogue (SCR-13) state | No change |
| 6 | Module 5 review §6 items 2 and 8 | Still open, not part of Module 7 | Project Owner decision (see `11-Module-5-Review-Package.md` §6) |
| 7 | Search timing and browser matrix | Not measured | Module 8 |

## 7. Approval

**Status: awaiting the Project Owner.** Automated checks pass (§3). Browser checks (§4) and the decision on open item 1 are needed before approval. After approval, the module is complete, pending the final `development` → `main` PR after Modules 7 and 8.

Approved by the Project Owner on: ______ (browser and version: ______)
