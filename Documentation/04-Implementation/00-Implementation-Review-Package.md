# 00 — Implementation Review Package (Module 4: Solution Skeleton)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Module:** 4 — Implementation (step 1: solution structure)
**Status:** Built and verified locally. Awaiting review. Committed to `development` after review is requested (CLAUDE.md §5.4).

---

## 1. Scope of This Step

| In scope | Out of scope (not built) |
|---|---|
| Three projects: Web, Business, Data | Any BRD feature (patients, appointments, consultations, prescriptions, history, search, export) |
| Test projects: unit and integration | Entities, DbSets and migrations |
| Dependency injection, DbContext, logging, configuration | Login, password hashing, authentication |
| Database creation script | Repositories, services and validators with rules |
| Setup guide and developer guide | Screens beyond a placeholder home page |

## 2. Structure

```
PatientManagement.slnx
Directory.Build.props
README.md
src/
  PatientManagement.Web/          Controllers, Views, ViewModels, Program.cs, appsettings
  PatientManagement.Business/     Common (Result), Interfaces, Services, DTOs, Validators
  PatientManagement.Data/         Context (DbContext, registration), Entities, Repositories, Migrations
tests/
  PatientManagement.UnitTests/           Common/ResultTests (4 tests)
  PatientManagement.IntegrationTests/    HomePageTests (2 tests)
database/
  Scripts/01-create-database.sql, Scripts/README.md
Documentation/04-Implementation/
  00-Implementation-Review-Package.md, 01-Setup-Guide.md, 02-Developer-Guide.md
```

Empty folders (`Entities`, `Repositories`, `Migrations`, `Interfaces`, `Services`, `DTOs`, `Validators`) hold a `.gitkeep` so Git tracks them.

## 3. Requirements Traceability

No BRD requirement is implemented in this step. The tests are infrastructure tests and say so in their comments. No requirement ID is claimed.

| Item | Basis |
|---|---|
| Three-tier layout and reference rules | `Documentation/02-Planning/01-` AD-01, AD-02; CLAUDE.md §6 |
| `DbContext` with no entities | AD-04 (EF Core, one context); PDR-32 (EF Core only) |
| Configuration and secrets | CLAUDE.md §9; `Documentation/04-Implementation/02-` §6 |
| `Result<T>` (errors and warnings) | AD-11 |
| Database created by script; schema by migrations | CLAUDE.md §6; `database/Scripts/README.md` |
| Target .NET 10 LTS, SQL Server 2022 (local) | PDR-31 (approved 2026-11-10) |

## 4. Decisions Applied and Deviations

| # | Item | What was done | Why | For review |
|---|---|---|---|---|
| 1 | Project names | `PatientManagement.Web`, `.Business`, `.Data`, `.UnitTests`, `.IntegrationTests` as you specified | Your instruction | The planning documents use `PMS.*` names. Decide whether to rename the planning references (R-1) |
| 2 | Solution file | `PatientManagement.slnx` (the .NET 10 SDK default format) | The SDK creates this format. `.sln` is also supported | Confirm, or request `.sln` (R-2) |
| 3 | Target framework | `net10.0` on all projects | PDR-31: latest LTS. .NET 10 is LTS | — |
| 4 | EF Core packages | `Microsoft.EntityFrameworkCore.SqlServer` and `.Design` pinned to `10.0.14` | The `10.0.15` packages depend on `Abstractions 10.0.15`, which is not on the NuGet feed. The feed has up to `10.0.14` | Revisit when the feed has a consistent set (R-3) |
| 5 | Security pin | `System.Security.Cryptography.Pkcs` `10.0.15` in `Directory.Build.props` | `9.0.11` (transitive) has known advisories GHSA-2fmv-rv2c-59x7 and GHSA-846f-f764-76mx | Confirm the approach (R-4) |
| 6 | Logging | Built-in `Microsoft.Extensions.Logging` only. No Serilog or other logging library | CLAUDE.md §2: no new framework without a decision | Confirm no logging library for Phase 1 (R-5) |
| 7 | Validation | Hand-written validators; no FluentValidation | AD-11: no validation library chosen | — |
| 8 | Connection string | Default `Server=localhost\SQLEXPRESS` (Windows authentication) in `appsettings.json` | The machine has SQL Server 2022 Express running. The default `localhost` instance is not installed here | Committed default is machine-specific. Confirm, or change to a neutral default with user secrets for each developer (R-6) |
| 9 | Secrets | `UserSecretsId` initialised on the Web project | CLAUDE.md §9: secrets outside source control | — |
| 10 | Home page | A placeholder page states "No BRD features are implemented yet" | Verifies the stack end to end without a feature | Confirm (R-7) |
| 11 | Authentication | Not added. `UseAuthorization` is in place; authentication arrives with the login feature | BRD Security (NFR-SEC-001) is a feature | — |
| 12 | Template content | Privacy page and its links removed. `Models` folder moved to `ViewModels` | Phase 1 has no privacy page. Namespaces follow the layers | — |
| 13 | Solution folders | Per your request: Controllers, Views, ViewModels (Web); Interfaces, Services, DTOs, Validators, Common (Business); Context, Entities, Repositories, Migrations (Data); UnitTests, IntegrationTests; Database Scripts | Your instruction | — |

## 5. Verification

| Check | Command | Result |
|---|---|---|
| Restore | `dotnet restore PatientManagement.slnx` | Succeeded |
| Build | `dotnet build PatientManagement.slnx` | Succeeded. 0 warnings, 0 errors |
| Tests | `dotnet test PatientManagement.slnx` | 4 unit tests passed; 2 integration tests passed |
| Run (Development) | `dotnet run --project src/PatientManagement.Web --launch-profile http` | Starts. Logs the startup line. `http://localhost:5092` returns HTTP 200 and the placeholder page |
| Database script | `sqlcmd -S "localhost\SQLEXPRESS" -E -b -i database/Scripts/01-create-database.sql` | Creates `PatientManagement`. Second run makes no change |
| EF Core migration tooling | `dotnet ef` | Not verified. Nothing to migrate yet |
| Secret scan (committed files) | Manual search for passwords and keys | None found |

## 6. Questions for Review

| ID | Question | Recommendation |
|---|---|---|
| R-1 | Rename planning references from `PMS.*` to `PatientManagement.*`? | Yes. Keep one name across the project. The rename is text-only |
| R-2 | Keep `.slnx` or use `.sln`? | Keep `.slnx`. It is the SDK default |
| R-3 | Keep EF Core pinned at 10.0.14 until the feed is consistent? | Yes |
| R-4 | Keep the Pkcs pin in `Directory.Build.props`? | Yes. Remove when the transitive dependency no longer needs it |
| R-5 | No logging library in Phase 1? | Yes. Built-in logging is enough for a learning project |
| R-6 | Keep `localhost\SQLEXPRESS` as the committed default? | Keep for this machine. A neutral default would need a per-developer override on every machine |
| R-7 | Is the placeholder home page acceptable for this step? | Yes |

## 7. Not Done

- No BRD feature.
- No entities, DbSets or migrations.
- No authentication or password handling.
- No services, validators or repositories with rules.
- No EF Core tooling check.
