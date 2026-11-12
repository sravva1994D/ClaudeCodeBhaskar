# 12 — Module 6 Implementation Plan (Visit Date Filter and Recent Patients)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Covers:** Implementation item 7 (History filter) and item 8 (Recent patients) in `Documentation/04-Implementation/02-Developer-Guide.md`
**Milestone:** M6 (roadmap `Documentation/02-Planning/15-Development-Roadmap.md`)
**Work packages (`16-WBS`):** WP-7.1 (date filter), WP-7.2 (recent patients)
**Status:** Plan approved in chat on 2026-11-12. The decisions in §4 (D-M6-1 to D-M6-7) are accepted as recommended. Implementation proceeds one step at a time on the Project Owner's go-ahead.
**Source rules:** BRD (FR-HIS-003, FR-SRC-002); CLAUDE.md §4, §6, §7, §8, §10; PDR-11; `Documentation/04-Implementation/11-Module-5-Plan.md` (format)

---

## 1. Scope

| In scope | Requirement IDs | Work packages |
|---|---|---|
| Date filter on visit history SCR-07: single date or From–To range, inclusive, with a Clear link | FR-HIS-003 | WP-7.1 |
| Recent patients on the home page SCR-03: five most recently visited patients, latest visit first, consultations only | FR-SRC-002 | WP-7.2 |

Out of scope: export (FR-EXP-001/002, M7), search timing and browser matrix (M8). No BRD change. No schema change and no migration: the index `IX_Consultations_Patient_Visit` already covers both queries.

## 2. Requirement mapping

| Requirement | Acceptance criteria (`10-Acceptance-Criteria.md`) | Test IDs (`14-Testing-Strategy.md` §4) |
|---|---|---|
| FR-HIS-003 | AC-1 filter by date; AC-2 only that range | TC-FR-HIS-003-1 (single date); TC-FR-HIS-003-2 (range boundaries inclusive) |
| FR-SRC-002 | AC-1 recently accessed patients shown | TC-FR-SRC-002-1 (five patients, latest visit first); TC-FR-SRC-002-2 (patients with no visit excluded, PDR-11) |

Every test that verifies a requirement carries its ID in the name (CLAUDE §4.2, §10).

## 3. Design points

| Point | Design | Source |
|---|---|---|
| Filter form | GET form with two date inputs, From and To. From alone is a single date; To empty means the same day | Route 13, `09-Controller-Design.md` |
| Inclusive range | Lower bound From 00:00; upper bound To + 1 day, exclusive, in the repository query | FR-HIS-003 AC-2; AD-07 (local time, no UTC) |
| Invalid range | From later than To is refused by the service with "From date must not be after To date". Nothing is filtered; typed values stay | CLAUDE §8 (rules in Business) |
| Empty results | "No visits in this date range" plus a Clear link, distinct from "No visits yet" | `05-` SCR-07 |
| Recent patients | Group by patient over consultations, latest VisitDateTime, top 5. Patients with no visit are not listed | PDR-11; `11-Repository-Design` §3 |
| Recent list content | Name (link to profile), patient code, last visit date and time | `05-` SCR-03 |
| Layers | Controllers call one service each; services hold the rules; repositories stay thin; entities stay in Data | AD-02, AD-03 |
| Clock | Home date comes from the injected `TimeProvider`, as today | CLAUDE §10 |

## 4. Decisions for the Project Owner

Each has a recommendation. Approval of this plan accepts them unless you say otherwise.

| # | Question | Recommendation |
|---|---|---|
| D-M6-1 | Filter form | GET form on SCR-07 with From and To date inputs; single date by filling From only |
| D-M6-2 | Inclusive range | Whole To day included (`VisitDateTime < To + 1 day`) |
| D-M6-3 | Invalid range | From after To: error message, no filtered list |
| D-M6-4 | Empty result | "No visits in this date range" with Clear link |
| D-M6-5 | Recent list content | Name, patient code, last visit date and time; max 5; no-visit patients excluded |
| D-M6-6 | Empty recent list | "No recent patients yet" |
| D-M6-7 | Date format | Dates shown as yyyy-MM-dd. The new filter parses dates in invariant form. The Appointments redirects (Module 5 review item 8) are not changed by this module |

## 5. Steps

Each step builds with 0 warnings, passes its tests, and is committed and pushed to `development` on the Project Owner's go-ahead.

1. **Repositories (WP-7.1, WP-7.2).** Extend `IConsultationRepository.ListByPatientAsync` with optional from and to (null keeps current callers). Add a recent-patients query: group by patient, latest visit, top N. Add a patient lookup by ids. Data integration tests: single date; inclusive boundaries (a visit at 23:59 on the To day is included, one at 00:00 the next day is not); five patients ordered; a patient with no visit is excluded.
2. **Services.** `ListVisitsAsync` takes optional from and to and refuses From after To (D-M6-3). `PatientService.GetRecentAsync(5)` returns summaries with the last visit time. Unit tests: TC-FR-HIS-003-1, -2; TC-FR-SRC-002-1, -2; invalid range.
3. **Visit history screen (WP-7.1).** `VisitsController.Index` takes from and to. View model and `Visits/Index.cshtml` get the filter form, Clear link and empty-result message. Web tests: single date, range, invalid range, no results, unfiltered list unchanged.
4. **Home screen (WP-7.2).** `HomeController` loads recent patients. `Home/Index.cshtml` gets a "Recent patients" section. Web tests: five latest shown in order, sixth omitted, no-visit patient absent, link to profile.
5. **Close-out.** Full unit and integration run. Update `18-Planning-Traceability-Matrix.md` and `12-Requirement-Traceability-Matrix.md` for FR-HIS-003 and FR-SRC-002. Write `13-Module-6-Review-Package.md` with Chrome browser checks. Clear Module 4 review §6 items 3 and 4 (both now delivered).

## 6. Verification

- `dotnet build PatientManagement.sln --no-incremental`: 0 errors, 0 warnings. Unit and integration suites green. Stop the app before building.
- Browser (Chrome), with several visits over several days: single date; range; boundary day; From after To; empty range; Clear. Home shows five latest patients in order and updates after a new consultation; a patient with no visit does not appear.

## 7. Risks

| Risk | Handling |
|---|---|
| Time-of-day boundary on the To date | Exclusive upper bound at To + 1 day; boundary tests at 23:59 and 00:00 |
| Culture-dependent date parsing | Date inputs post ISO dates; binding tested in invariant form |
| Recent query cost on 1,000 patients | Covered by the existing index; timing is checked in M8 |

## 8. Completion criteria

FR-HIS-003 and FR-SRC-002 pass their tests, which carry the IDs in their names. Both matrices are updated. The Project Owner approves the Module 6 review package.
