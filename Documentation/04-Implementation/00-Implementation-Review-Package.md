# 00 — Implementation Review Package (Module 4: Solution Skeleton)

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Module:** 4 — Implementation (step 1: solution structure)
**Status:** Built, tested and verified locally. Review answers R-1 to R-7 applied (2026-11-10). Awaiting the Project Owner's review of this step.

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
PatientManagement.sln
.gitattributes
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

## 4. Decisions Applied (R-1 to R-7)

| # | Review item | Decision (2026-11-10) | What was done |
|---|---|---|---|
| R-1 | Rename `PMS.*` to `PatientManagement.*` | Yes | Planning, architecture, design, testing, commit-standard and README references renamed. Test project names in the testing strategy aligned with the real projects (`PatientManagement.UnitTests`, `PatientManagement.IntegrationTests`). Database name in the planning documents is `PatientManagement` |
| R-2 | `.slnx` or `.sln` | `.sln` | `PatientManagement.slnx` replaced by `PatientManagement.sln` (format 12, Visual Studio 17). All five projects listed |
| R-3 | Keep EF Core at 10.0.14 | Yes | `Microsoft.EntityFrameworkCore.SqlServer` and `.Design` stay at `10.0.14`. No upgrade during implementation unless a specific issue requires it |
| R-4 | Keep package pins | Yes | Every `PackageReference` has an explicit version. Pkcs pin (`10.0.15`) kept in `Directory.Build.props` |
| R-5 | No logging library in Phase 1 | Yes | Built-in ASP.NET logging only |
| R-6 | Neutral connection string plus local overrides | Yes | Committed default in `appsettings.json` is `Server=localhost` (neutral sample). This machine's instance (`localhost\SQLEXPRESS`) is in user secrets, not in any committed file |
| R-7 | Placeholder home page | Yes | Kept. Polishing is deferred |
| — | `.gitattributes` for line endings | Yes | Added: text files `eol=lf`; binary markers for `.docx`, images, fonts |

Implementation order approved with these answers (`02-Developer-Guide.md` §13; `15-Development-Roadmap.md` §1A): Authentication, Patient Management, Appointments, Consultations, Prescriptions.

## 5. Other Decisions Made in This Step

| Item | What was done | Why |
|---|---|---|
| Target framework | `net10.0` on all projects | PDR-31: latest LTS |
| Restore feed issue | EF Core 10.0.15 packages depend on Abstractions 10.0.15, which is not on the feed. Pinned to 10.0.14 | R-3 keeps 10.0.14 |
| Security pin | `System.Security.Cryptography.Pkcs` 10.0.15 in `Directory.Build.props` | `9.0.11` (transitive) has known advisories GHSA-2fmv-rv2c-59x7 and GHSA-846f-f764-76mx |
| Validation | Hand-written validators; no FluentValidation | AD-11: no validation library chosen |
| Secrets | `UserSecretsId` set on the Web project | CLAUDE.md §9 |
| Authentication | Not added. `UseAuthorization` is in place | Authentication is the next implementation item |
| Template content | Privacy page removed. `Models` moved to `ViewModels` | Phase 1 has no privacy page |

## 6. Verification (after the R-1 to R-7 changes)

| Check | Command | Result |
|---|---|---|
| Build | `dotnet build PatientManagement.sln` | Succeeded. 0 warnings, 0 errors |
| Tests | `dotnet test PatientManagement.sln` | 4 unit tests passed; 2 integration tests passed |
| Run (Development) | `dotnet run --project src/PatientManagement.Web --launch-profile http` | Starts in Development. Logs the startup line. `http://localhost:5092` returns HTTP 200 and the placeholder page |
| Database script | `sqlcmd -S localhost\SQLEXPRESS -E -b -i database/Scripts/01-create-database.sql` (verified before R-6; the database exists) | Creates `PatientManagement`. Second run makes no change |
| Committed connection string | `git grep SQLEXPRESS` on committed code (excluding docs) | No match |
| Secret check | `dotnet user-secrets list` | Key present; value not shown |
| Line endings | Commit with `.gitattributes` | Checked in the commit output (see §8) |
| EF Core migration tooling | `dotnet ef` | Not verified. Nothing to migrate yet |

## 7. Not Done

- No BRD feature.
- No entities, DbSets or migrations.
- No authentication or password handling.
- No services, validators or repositories with rules.
- No EF Core tooling check.

## 8. Commits

Committed to `development`, in this order: the `.sln` and renames; `.gitattributes`; the neutral connection string; the governance and guide updates. Each commit is listed in chat with its message.
