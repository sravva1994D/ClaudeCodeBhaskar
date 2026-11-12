# UI Modernization Review Package

**Status:** Implemented and committed (cb4f917, 11f5fe4). Browser checks and the Module 9 acceptance passed (M15).
**Decision:** DEC-026 (2026-11-12, `01-Brainstorming/14-` and BRD decision log).
**Requirements:** no requirement wording changed. The screens below carry the existing requirement IDs (FR-PAT, FR-APT, FR-HIS, FR-EXP, FR-RX, FR-VIT, FR-CMP, FR-DIA, NFR-SEC).

## 1. What was approved and what was done

| Decision | Done | Notes |
|---|---|---|
| jQuery DataTables on list screens | Yes, on four lists (below) | Client-side on the rendered rows. No server paging, so no service or repository change |
| Page sizes 10 (default), 25, 50, 100; search; sorting; total count | Yes | DataTables shows "Showing X to Y of Z entries". Actions columns are not sortable |
| Palette (Bootstrap 5 colours) | Yes, as tokens in `site.css` | The approved values are the Bootstrap 5 defaults, so the look is unchanged. Tokens make them visible in one place |
| Page-header standard (title, short description, actions right) | Yes, on every major screen | Markup is the same on each screen (`.page-header` in `site.css`) |
| Breadcrumbs on major screens | Yes | Rendered by `_Layout` from `ViewData["Breadcrumbs"]`; the last item is not a link |
| dd-MMM-yyyy date display | Yes, on screen and in the prescription and export print views | See section 3 for the limits |

## 2. Screens

**DataTables (lists):**
- Patient search results (`Patients/Search`)
- Patient visit history (`Visits/Index`)
- Daily appointments (`Appointments/Daily`)
- Today's appointments on the home page (`Home/Index`). The recent-patients list is not converted: it is capped at five (PDR-11).

**Breadcrumbs:**
| Screen | Trail |
|---|---|
| Home | none (root) |
| Patient search | Home > Patient search |
| Register patient | Home > Patient search > Register patient |
| Patient profile | Home > Patient search > [name] |
| Edit patient | Home > Patient search > [name] > Edit |
| Visit history | Home > Patient search > [name] > Visit history |
| Visit detail | Home > Patient search > [name] > Visit history > Visit |
| Schedule appointment | Home > Patient search > [name] > Schedule appointment |
| Consultation | Home > Patient search > [name] > Consultation |
| Export | Home > Patient search > [name] > Export |
| Appointments (daily) | Home > Appointments |
| Clinic details | Home > Clinic details |
| Sign-in, forgot password | none (signed-out pages) |

**Not present in the application, so not changed:** list screens for Patients (all patients), Consultations, Medications and Prescriptions. The approval named them. No such screens exist, and creating them needs a BRD requirement. The approved DataTables standard applies to them if they are added later.

## 3. Date display

- **On screen and in print:** dd-MMM-yyyy (for example 12-Nov-2026), with time as HH:mm where a time is shown.
- **Sorting:** DataTables sorts day-month-year text wrongly, so each date cell in a DataTable carries a `data-order` in yyyy-MM-dd.
- **CSV export:** unchanged, ISO yyyy-MM-dd, as DEC-020 requires. The export form says so.
- **Date input fields:** still HTML5 `type="date"`. The value is sent as yyyy-MM-dd, which the browser needs. **The browser chooses how the field displays the date** (for example 11/12/2026 in Chrome, depending on the operating system's region). The application cannot force dd-MMM-yyyy inside a native date field. The date shown next to or below the field uses the approved format.

## 4. Code changes

- `Views/Shared/_Layout.cshtml`: DataTables CSS and scripts; breadcrumb rendering.
- `wwwroot/css/site.css`: palette tokens; page-header class; light page background.
- `wwwroot/js/site.js`: DataTables setup (`table.data-table`); the confirmation and busy behaviour from Phase 2 is unchanged.
- `wwwroot/lib/datatables/`: DataTables 2.1.8 (`dataTables.min.js`, `dataTables.bootstrap5.min.js`, `dataTables.bootstrap5.min.css`), downloaded from the DataTables CDN. The library is MIT licensed.
- `Presentation/StatusBadge.cs`: unchanged from Phase 2.
- Views under `Views/`: page headers, breadcrumbs, DataTables classes, dates. Print views: dates only.

No controller, service, repository, entity, migration, validation rule or query changed.

## 5. Tests

- Build: 0 warnings, 0 errors.
- Unit tests: 368 passed.
- Integration tests: 299 passed.
- Five integration assertions checked the old display format (for example `1990-05-01` in a profile). They were updated to the approved format (`01-May-1990`). The CSV assertions and the date-input value assertions still check ISO, which is correct.

## 6. Not checked here

- **Browser behaviour:** checked in Chrome 157.0.8103.79 (M15). Paging, sorting, search, page sizes, breadcrumb links and the header layout passed.
- **Accessibility:** keyboard use of DataTables controls and the breadcrumb is expected from Bootstrap and DataTables, but not verified.

## 7. Open items

1. Owner decision: whether to keep the date-input display as the browser's own format (current), or change the input to a text field with a calendar button (a larger change). **Accepted** as L1 (Project Owner, 2026-11-12).
2. The three missing list screens (all patients, consultations, medications and prescriptions) are not built. Decide whether they are needed; they would need a BRD requirement. **Accepted** as L2 (not built).
3. **Done** (M15, Chrome 157.0.8103.79): paging, sorting, search and breadcrumbs passed.
