# Module 9 Plan: Phase 1 Acceptance

**Status:** Approved by the Project Owner on 2026-11-12 (plan approval). Steps are built one at a time on the owner's go-ahead.
**Milestone:** M9 (`../02-Planning/15-Development-Roadmap.md`). Work packages WP-10.1 and WP-10.2 (`../02-Planning/16-Work-Breakdown-Structure.md`).

## 1. Context

Modules 1 to 8, the UI modernization (DEC-026) and reschedule (DEC-027) are approved. M9 is the last milestone before the final steps in CLAUDE.md §5.4 (full build and tests, browser verification, `/code-review`, one PR `pms-Implementation` → `development`). M9 adds no features, no schema change and no BRD change. Code changes happen only if acceptance finds a defect that the owner decides to fix.

Already decided and not re-opened:
- 26 in-scope IDs after DEC-025 and DEC-027: 17 Must Have FR, FR-HIS-003, FR-SRC-002, FR-SRC-003 and FR-APT-004 (Should), NFR-PRF-001, NFR-SEC-001, NFR-SEC-002, SC-004 and SC-005.
- Test IDs and the acceptance map: `../02-Planning/14-Testing-Strategy.md` §4 (`TC-<Req ID>-<n>`).
- Browser: Chrome is the primary target (DEC-019, DEC-025 removed the Edge matrix). Browser version recorded per run (PDR-28). No browser automation (PDR-29).
- The owner skipped several manual M8 checks as a learning project (DEC-025 and the M8 record). M9 records those skips; it does not repeat them.
- Deferred and Won't Have IDs (NFR-REL-001, NFR-REL-002, SC-003, SC-006, and the six DEC-025 items) are listed in the record as not accepted in Phase 1, with the reason.

## 2. Scope

| In | IDs | WP |
|---|---|---|
| Acceptance record: one row per in-scope ID with scenario, automated evidence (test class and name) or manual step, and result | All 26 | WP-10.1 |
| Coverage check: each in-scope ID appears in at least one test name or comment | All 26 | WP-10.1 |
| Full unit and integration run on the final code; counts recorded exactly | All 26 | WP-10.1 |
| Short manual Chrome pass for what tests cannot show: prescription and export print views with Save as PDF, CSV opening in a spreadsheet, sign-in and sign-out, reschedule and past-slot display | SC-004, SC-005, FR-APT-004, NFR-SEC-001 | WP-10.1 |
| Acceptance record sign-off, defects, skipped items and known limits | All 26 | WP-10.2 |
| Matrix updates: `../01-Brainstorming/12-Requirement-Traceability-Matrix.md` and `../02-Planning/18-Planning-Traceability-Matrix.md` (status to Accepted) | All 26 | WP-10.2 |

Out of scope: new features; the optional UI recommendations from the DEC-026 review (listed as known limits, not built); older Low items from earlier modules (listed, not fixed); the final PR and its merge (CLAUDE.md §5.4 steps 7 and 8, which need their own go-ahead).

## 3. Decisions for the Project Owner

Recommendations are shown. Steps 2 to 5 follow these.

| # | Question | Recommendation |
|---|---|---|
| D-M9-1 | Evidence standard | Automated tests are the evidence for functional rules, since they already carry IDs. The manual pass covers only what a browser shows (print and PDF, spreadsheet open, layout). About 15 manual checks |
| D-M9-2 | Manual pass depth | One Chrome pass. The owner reports Pass or Fail per line with the Chrome version. Edge, timing and volume are not repeated |
| D-M9-3 | Defect handling | As `../02-Planning/14-Testing-Strategy.md` §6. High blocks acceptance and is fixed on `pms-Implementation` with a test. Medium and Low are logged, and the owner decides fix or accept |
| D-M9-4 | Where the record lives | `19-Module-9-Acceptance-Record.md` in this folder (script and record in one file) |
| D-M9-5 | Test data for the manual pass | Synthetic only, entered through the UI: a few patients, visits, one appointment, and one export whose free text has a comma, a quote and a leading `=`. The owner decides whether to remove it afterwards |
| D-M9-6 | Open items from earlier modules | Listed in the record as known limits, each with an owner decision (accept or schedule). Not fixed in M9 |
| D-M9-7 | What "accepted" means | All 26 IDs pass, or a failure has an owner-approved disposition. The owner signs with a date |

## 4. Steps

Each step is one owner go-ahead. Commit and push to `pms-Implementation` in the format of CLAUDE.md §5.2, with the `Requirements:` line and the Co-Authored-By line.

1. **Plan file (this document).** Commit `docs(docs)`.
2. **Coverage check and automated run (WP-10.1).** Stop the app. Run `dotnet build PatientManagement.sln --no-incremental` (0 warnings, 0 errors), then `dotnet test PatientManagement.sln --no-build`. Search `tests/` for each of the 26 IDs. Record counts and any gap. A missing test is added only if a gap is found, in its own test-only commit.
3. **Acceptance record (WP-10.1).** Draft `19-Module-9-Acceptance-Record.md`: a table of 26 rows (ID, scenario, automated evidence, manual step number or "automated only", result left blank for manual rows). Manual Chrome checklist: sign in and protected-URL redirect; sign out; register a patient with the duplicate warning; search and profile; schedule, check in, reschedule, cancel; consultation with a vitals warning, complaints, diagnosis and two medicines; prescription print view and Save as PDF (SC-004); visit history with a date filter; recent patients on the home page; export CSV opens in a spreadsheet and export print view Save as PDF (SC-005); page loads recorded in M8 (NFR-PRF-001). Commit `docs(docs)`.
4. **Owner runs the manual pass.** The app is started. The owner reports Pass or Fail per line with the Chrome version. Each failure is logged with ID, steps, expected and actual (`14-Testing-Strategy.md` §6). A High defect is fixed in its own commit with a test, and only the affected lines are re-run.
5. **Close-out (WP-10.2).** Results are filled in exactly as reported. Defects, skipped items and known limits (D-M9-6) are listed. The 26 IDs are marked Accepted in both matrices, the roadmap and WBS show M9 status, and the sign-off block is added. The owner signs off. Commit and push.

## 5. Verification

| Check | How | Pass rule |
|---|---|---|
| Build | `dotnet build PatientManagement.sln --no-incremental` (app stopped) | 0 errors, 0 warnings |
| Tests | `dotnet test PatientManagement.sln --no-build` | All pass; counts recorded (last run: unit 382, integration 304) |
| Coverage | Search of `tests/` for each of the 26 IDs | Every ID found; gaps listed and closed |
| Manual pass | Owner's Pass or Fail per line, with Chrome version | Every manual line has a result |
| No sensitive data | Search the record and this plan | No patient data, passwords or connection strings |

## 6. Risks

| Risk | Handling |
|---|---|
| An ID has no test with its name | Coverage step finds it; a test is added first |
| Manual pass finds a High defect late | Fix on `pms-Implementation` with a test; re-run only the affected lines |
| Scope creep from the UI recommendations | Recorded as known limits; acting on one needs an owner decision |
| Skipped M8 checks read as passed | The record states each skip and the owner decision behind it |

## 7. Completion criteria

- Steps 1 to 5 committed and pushed to `pms-Implementation`, each building and passing its checks.
- 26 of 26 in-scope IDs are Accepted, or have an owner-approved disposition.
- Matrices, roadmap and WBS are updated.
- Owner sign-off is recorded.

Next, after M9: the final steps in CLAUDE.md §5.4 (steps 4 to 7). No PR is opened until the owner confirms.

## 8. Links

- `../02-Planning/14-Testing-Strategy.md` §4 and §6
- `../02-Planning/15-Development-Roadmap.md` (M9)
- `../02-Planning/16-Work-Breakdown-Structure.md` (WP-10.1, WP-10.2)
- `../02-Planning/17-Planning-Decisions-and-Open-Items.md`: PDR-28, PDR-29
- `16-Module-8-Review-Package.md` (skipped checks and timing record)
- `17-UI-Modernization-Review-Package.md` (open UI items)
