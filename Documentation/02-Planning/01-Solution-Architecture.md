# 01 — Solution Architecture

**Module:** 2 — Planning (Phase 1)
**Source of truth:** `BRD/Doc_BRD.md` (Phase 1, amended 2026-11-10)
**Architecture direction:** ASP.NET Core MVC, 3-tier (Presentation, Business, Data), SQL Server (requester direction)
**Status:** Proposed for review. Nothing here is built yet.

---

## 1. Context

- One browser-based application used by one general physician (single user, BRD: Users).
- Runs on localhost against a local SQL Server (BRD: Performance, Scalability).
- No external integrations (OOS-004), no offline mode (OOS-006), no mobile client (OOS-007), no multi-clinic or multi-user model (OOS-001, OOS-009).
- Not deployed. TLS is required only when the app runs beyond localhost (BRD: Security).

## 2. Technology Baseline

| Concern | Choice | Basis |
|---|---|---|
| Presentation | ASP.NET Core MVC, Razor Views, ViewModels, Controllers | Requester direction |
| Business | C# Services, Service Interfaces, DTOs, Validators | Requester direction |
| Data access | Entity Framework Core, Repositories | Requester direction |
| Database | SQL Server (localhost) | Requester direction; BRD: Performance (local SQL Server) |
| Authentication | Cookie authentication with hashed passwords | NFR-SEC-001, NFR-SEC-002; see `12-Authentication-Design.md` |
| Target .NET version | Latest LTS at M1 start; local SQL Server version (PDR-31, decided 2026-11-10) | Recorded in repository README |
| PDF generation | **None.** Printable HTML views; browser Print and Save as PDF (PDR-18, decided 2026-11-10) | No third-party dependency |
| Validation | Hand-written validators in the Business layer, returning errors and warnings | AD-11. No validation library is chosen |

## 3. Architecture Decisions (proposed)

| ID | Decision | Rationale | Req IDs |
|---|---|---|---|
| AD-01 | Three layers as separate class library projects: `PatientManagement.Web` (Presentation), `PatientManagement.Business` (Business), `PatientManagement.Data` (Data). Test projects per layer. | Matches requester direction; keeps dependency direction explicit | All |
| AD-02 | Dependency direction: Web → Business → Data. Web references Data only for dependency-injection registration of the DbContext and repositories. | Controllers never touch EF Core types | All |
| AD-03 | EF Core entities live only in `PatientManagement.Data`. Business exchanges DTOs. Web uses ViewModels. No entity crosses into Web. | Stops persistence shape leaking into views | All |
| AD-04 | Repositories are thin wrappers over the EF Core `DbContext`, one per aggregate root. No generic repository. EF Core is used for all data access, including document views. Dapper is not used (PDR-32, decided 2026-11-10). | Avoids over-abstraction for a single-user app | FR-PAT-*, FR-APT-*, FR-HIS-*, FR-RX-* |
| AD-05 | Server-rendered MVC pages with forms. No SPA framework. Minimal JavaScript for confirmation dialogs and the 30-minute slot picker. | Simple, fast data entry (NFR-USB-001); fewer moving parts | NFR-USB-001, NFR-CMP-001 |
| AD-06 | Cookie authentication, single user, 24-hour sliding idle timeout. Passwords hashed, never stored in plain text. | NFR-SEC-001 | NFR-SEC-001, NFR-SEC-002 |
| AD-07 | Single-site: store local clinic date and time. No UTC conversion. | Localhost, single location | FR-HIS-001, FR-HIS-003, FR-APT-002 |
| AD-08 | No `ClinicId`, `TenantId` or user-scoping columns. | Multi-clinic is OOS-009; avoids building it | OOS-009 |
| AD-09 | Patient code (`PT-000123`) is generated from the database identity and stored in a unique column. | BRD: unique system-generated identifier with prefix | FR-PAT-001 |
| AD-10 | Vitals stored in their own table, zero or one row per consultation. Missing vitals means no row. | Vitals can be missing and added later (FR-VIT-001) | FR-VIT-001 |
| AD-11 | Validation returns two lists: **blocking errors** and **warnings**. A warning needs an explicit confirmation to save. Warnings never throw. | Warn-and-allow rules (FR-VIT-001, FR-PAT-001) | FR-VIT-001, FR-PAT-001 |
| AD-12 | No hard delete. Appointments are archived (flag). Consultations and prescriptions are not deleted by the app. | BRD: archive, not hard delete | FR-APT-003 |
| AD-13 | Prescription immutability is enforced in the Business layer and recorded by an `IsPrinted` flag in the database. | FR-RX-002: cannot be edited after printing | FR-RX-002 |
| AD-14 | No database check constraints on vital-sign ranges. Ranges are warnings, not rules. | Saving is never blocked by range (FR-VIT-001) | FR-VIT-001 |
| AD-15 | Schema managed by EF Core migrations. | Repeatable local setup | All data requirements |

## 4. Quality Attribute Approach

| Attribute | Approach | Req IDs |
|---|---|---|
| Performance | Indexes on search and date columns (see `07-Database-Design.md`). Paged lists. No caching in Phase 1. Measured on localhost with 1,000 seeded patients. | NFR-PRF-001, NFR-PRF-002, NFR-SCL-001 |
| Usability | Short forms, keyboard-friendly, one consultation screen for vitals, complaints, diagnosis and medicines. | NFR-USB-001, SC-001 |
| Security | Global authorisation (all pages except login and forgot password). Anti-forgery tokens on POST. Secure password hashing. | NFR-SEC-001 |
| Encryption | At rest: out of Phase 1 scope (DEC-017; localhost-only learning project). In transit: HTTPS required beyond localhost; plain HTTP on localhost. Passwords hashed. | NFR-SEC-002 |
| Compatibility | Standard HTML and CSS. Test on latest Chrome and Edge (desktop). Safari not required (DEC-019). | NFR-CMP-001 |
| Maintainability | Layer rules enforced by project references. Services are unit-testable. | Process (CON-017) |

## 5. Deployment View (Phase 1)

```
Developer machine (localhost)
├── Browser (Chrome | Edge, desktop)
│     └── HTTP  http://localhost:<port>            (HTTPS redirect configurable; see PDR-21)
├── PatientManagement.Web (ASP.NET Core MVC host, Kestrel)
│     └── PatientManagement.Business (services, DTOs, validators)
│           └── PatientManagement.Data (EF Core, repositories)
└── SQL Server (local instance)  — database: PatientManagement
```

No cloud, container, load balancer or background worker is in Phase 1. Reminders and alerts are out of scope (OOS-010), so there are no background jobs.

## 6. Constraints Applied

| Constraint | How the architecture applies it |
|---|---|
| CON-002 Single user | One user account, no roles (see `12-Authentication-Design.md`) |
| CON-003 Single clinic | No clinic partitioning (AD-08) |
| CON-004 / CON-005 Browser only | MVC pages; no native client |
| CON-006 No offline | Server-side state; no offline cache |
| CON-007 / CON-016 Consultation flow | One consultation screen (SCR-11) |
| CON-008 Printable prescription | Printable HTML view SCR-12 with Print button (PDR-16, PDR-18) |
| CON-009 CSV and PDF export | CSV from export service (PDR-17); PDF via printable HTML view and browser Save as PDF (PDR-18) |
| CON-010 / CON-011 Performance | Indexing and measurement plan (`14-Testing-Strategy.md`) |
| CON-012 / CON-013 Security | AD-06, HTTPS rule (PDR-21) |
| CON-017 BRD is the source of truth | Every design element maps to a requirement ID (`18-Planning-Traceability-Matrix.md`) |

## 7. Explicitly Not in the Architecture

- Roles or permissions beyond the single user.
- Audit logging or edit history (OOS-013).
- Backup and recovery procedures (OOS-011).
- Reminders, notifications, billing, lab or pharmacy integration, analytics, AI features (OOS-002 to OOS-010).
- Public API. Phase 1 has no API surface beyond the MVC routes and file downloads.
