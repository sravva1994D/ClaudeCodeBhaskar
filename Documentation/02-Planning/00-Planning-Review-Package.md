# 00 — Planning Review Package (Module 2: Planning)

**Project:** Patient Management System — Phase 1 (localhost learning project)
**Source of truth:** `BRD/Doc_BRD.md` (Phase 1, amended 2026-11-10)
**Architecture direction:** ASP.NET Core MVC, 3-tier (Presentation, Business, Data), EF Core, SQL Server
**Status:** Awaiting review. Nothing is built. No Git operations have been performed.

---

## 1. What Is in This Package

| # | Artifact | File | Status |
|---|---|---|---|
| 00 | Planning Review Package | `00-Planning-Review-Package.md` (this file) | Review |
| 01 | Solution Architecture | `01-Solution-Architecture.md` | Proposed |
| 02 | Three-Tier Architecture Diagram | `02-Three-Tier-Architecture-Diagram.md` | Proposed |
| 03 | Layer Responsibilities | `03-Layer-Responsibilities.md` | Proposed |
| 04 | Navigation Flow | `04-Navigation-Flow.md` | Proposed |
| 05 | Screen Catalog | `05-Screen-Catalog.md` | Proposed |
| 06 | Entity Design | `06-Entity-Design.md` | Proposed |
| 07 | Database Design | `07-Database-Design.md` | Proposed |
| 08 | ERD | `08-ERD.md` | Proposed |
| 09 | Controller Design | `09-Controller-Design.md` | Proposed |
| 10 | Service Design | `10-Service-Design.md` | Proposed |
| 11 | Repository Design | `11-Repository-Design.md` | Proposed |
| 12 | Authentication Design | `12-Authentication-Design.md` | Proposed |
| 13 | Validation Strategy | `13-Validation-Strategy.md` | Proposed |
| 14 | Testing Strategy | `14-Testing-Strategy.md` | Proposed |
| 15 | Development Roadmap | `15-Development-Roadmap.md` | Proposed |
| 16 | Work Breakdown Structure | `16-Work-Breakdown-Structure.md` | Proposed |
| 17 | Planning Decisions and Open Items | `17-Planning-Decisions-and-Open-Items.md` | **32 decisions, all decided 2026-11-10** |
| 18 | Planning Traceability Matrix | `18-Planning-Traceability-Matrix.md` | Proposed |
| 19 | Appointment Status Lifecycle | `19-Appointment-Status-Lifecycle.md` | Resolved 2026-11-10 (workflow and transition table) |

All files are in `Documentation/02-Planning/`. Files are uncommitted, as per DEC-008.

## 2. Coverage at a Glance

| Measure | Count |
|---|---|
| In-scope requirement IDs traced (FR 20, NFR 7, SC 4) | 31 |
| Deferred IDs, not designed (NFR-REL-001, NFR-REL-002, SC-003, SC-006) | 4 |
| Screens | 14 (SCR-01 to SCR-14) |
| Controller routes | 29 |
| Services | 8 |
| Repository interfaces | 7 |
| Tables | 8 |
| Work packages | 43 across 10 groups |
| Planning decisions | 32, all decided 2026-11-10 |

Every in-scope requirement has at least one screen or component, one test, one work package and one milestone (`18-Planning-Traceability-Matrix.md` §5).

## 3. Architecture in Brief

| Layer | Project | Contents | Depends on |
|---|---|---|---|
| Presentation | `PatientManagement.Web` | Controllers, Razor views, ViewModels, cookie authentication | Business (and Data for registration only) |
| Business | `PatientManagement.Business` | Services, interfaces, DTOs, validators, mappers | Data |
| Data | `PatientManagement.Data` | EF Core DbContext, entities, repositories, migrations | SQL Server |

Design rules:
- Controllers hold no business rules. Services hold them.
- Entities never leave the Data layer. Services return DTOs.
- Warnings (for example, out-of-range vitals) do not block save when the BRD says so. They require confirmation.
- No hard deletes. Appointments are archived.
- No audit, multi-user, billing or reminder features (BRD Out of Scope).

## 4. Decisions Requested from Reviewer

### 4.1 Blocking decisions (all decided 2026-11-10; answers recorded in `17-`)

These come from `17-Planning-Decisions-and-Open-Items.md`. The BRD does not state them, so none has been assumed.

| PDR | Decision needed | Milestone | Owner |
|---|---|---|---|
| PDR-01 | Allowed values for Gender | M2 | Project Owner |
| PDR-02 | Password complexity rules and maximum length | M1 | Project Owner |
| PDR-05 | How the single user account is created | M1 | Project Owner |
| PDR-06 | Slot grid: working hours and start times | M3 | Project Owner |
| PDR-07 | Does a Cancelled or No-show appointment free its slot? | M3 | Project Owner |
| PDR-08 | Allowed status changes | M3 | Resolved 2026-11-10 (`19-`) |
| PDR-09 | What triggers archiving an appointment, and is archived data viewable | M3 | Project Owner |
| PDR-11 | Recent patients: count consultations only? | M6 | Project Owner |
| PDR-13 | Required medicine fields; empty prescription allowed? | M5 | Project Owner |
| PDR-14 | What counts as printed; reprint allowed? | M5 | Project Owner |
| PDR-15 | Where clinic header data is entered; logo storage | M5 | Project Owner |
| PDR-16 | Print method | M5 | Decided: printable HTML view (PDR-18) |
| PDR-17 | CSV layout | M7 | Project Owner |
| PDR-20 | "Idle timeout": sliding or fixed 24 hours | M1 | Project Owner |
| PDR-21 | Encryption mechanism at rest; plain HTTP on localhost | M1 | Project Owner |
| PDR-22 | Format rules for phone, email; date of birth not in future | M2 | Project Owner |
| PDR-18 | PDF generation | M5, M7 | Decided: no PDF library (printable HTML view) |
| PDR-32 | Dapper vs EF Core data access | M1 | Decided: EF Core only (see §4.3) |
| PDR-31 | .NET and SQL Server versions | M1 | Decided: latest LTS .NET at M1 start |

### 4.2 Non-blocking decisions (all decided 2026-11-10; answers recorded in `17-`)

The list below names the items. The answers are in the register.

- PDR-03 generic message on password reset mismatch.
- PDR-04 use the password hasher only, not the full Identity framework.
- PDR-10 exact duplicate match.
- PDR-12 complaints as one text block.
- PDR-19 missing vitals as a notice only.
- PDR-23 case-insensitive contains-match search.
- PDR-24 visit time set by the system.
- PDR-25 no patient delete or archive.
- PDR-26 one consultation per appointment.
- PDR-27 resolved 2026-11-10: an appointment reaches Completed only after In Consultation.
- PDR-28 to PDR-30 record test versions, use a standard .NET test stack, and time pages with browser timing.

### 4.3 New open item from the 2026-11-10 instruction

- **PDR-32 (M1): Dapper vs EF Core. Decided 2026-11-10:** EF Core for all data access, including document views. Dapper is not used. The architecture is unchanged.

## 5. Things Reviewer Should Check

| # | Check | Where |
|---|---|---|
| 1 | Clinic profile screen (SCR-14) and ClinicProfile table are a design addition needed for the prescription header. Confirm this is an acceptable reading of FR-RX-002 | `05-`, `06-` §2.8 |
| 2 | Home page (SCR-03) combines three requirements without adding features. Confirm | `05-` |
| 3 | Consultation entity is one record with optional vitals row. Confirm | `06-` §2.4 to §2.5 |
| 4 | Prescription printing requires a print action that marks the prescription printed | `09-` rows 23–24 |
| 5 | Appointment overlap rule uses the decided PDR-06 and PDR-07 values | `10-` §2.3 |
| 6 | No database range check on vitals (warnings only) | `07-` §2, `13-` §3.4 |
| 7 | Single-user, no roles, no ClinicId | `01-` AD-08 |
| 8 | Completed needs a started consultation (PDR-27, confirmed 2026-11-10) | `19-` §4 |

## 6. Module 1 Artifacts That Need Refreshing

These are inconsistent with the amended BRD. They were found during planning. They have not been changed. Listed in `17-` §5.

| Artifact | Issue |
|---|---|
| `01-Brainstorming/09-Constraints-Register.md` (CON-001, CON-014) | CON-001 refers to OOS-001 to OOS-010. CON-014 requires backups (deferred) |
| `01-Brainstorming/05-Functional-Requirements.md` (FR-VIT-001 note) | Says ranges are to be supplied. They now appear in the BRD |
| `01-Brainstorming/06-Non-Functional-Requirements.md` (NFR-SCL-001, NFR-PRF-001) | Conditions and volume now in the BRD |
| `01-Brainstorming/12-Requirement-Traceability-Matrix.md` | Many statuses still "Blocked"; the open questions are answered |
| `01-Brainstorming/00-Review-Summary.md` | Says 28 open questions, all open |
| `01-Brainstorming/01-BRD-Summary.md` (§5) | Lists 10 out-of-scope items, not 15 |

Recommendation: refresh these in WP-1.2 before Module 2 is approved. This is quick and keeps the trace clean.

## 7. Process Notes

- No code, no project files, no branch and no commit have been created. Documentation is local and uncommitted (DEC-008).
- BRD has not been changed in this step. If any PDR changes BRD wording, the BRD is amended first, committed to `development` after approval (DEC-011, CON-017).
- Priorities (MoSCoW) were approved by the Project Owner on 2026-11-10 (A-006, DEC-006). The roadmap uses them.
- Assumptions register (`01-Brainstorming/07-Assumptions-Log.md`) is unchanged. No new assumptions were made in planning.

## 8. Next Steps After Review

1. Reviewer checks the items in §5. All PDRs are decided.
2. Refresh the Module 1 artifacts listed in §6 (WP-1.2).
3. BRD amended on 2026-11-10 for decided wording, the appointment lifecycle and approved priorities (WP-1.3 done).
4. Start Module 4 (implementation) directly on `development`, after M0 is closed and the approval gate is passed for each commit (see `Documentation/03-Worktree/`). No Git action until the Project Owner says so.

## 9. Not Done in This Step

- No implementation, no code scaffold, no database.
- No Git commit, branch, push, PR or merge.
- No BRD change.
- No effort estimates or dates. The roadmap is sequence only.
