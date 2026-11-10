# 02 — Developer Guide

**Project:** Patient Management System, Phase 1
**Applies to:** developers working in `ClaudeCodeBhaskar`
**Status:** Skeleton, 2026-11-10. Rules here follow `CLAUDE.md` and the planning documents

---

## 1. Solution Layout

```
PatientManagement.slnx
├── src/
│   ├── PatientManagement.Web/            Presentation (ASP.NET Core MVC)
│   │   ├── Controllers/                  Controllers (no business rules, no EF Core)
│   │   ├── Views/                        Razor views
│   │   ├── ViewModels/                   View models
│   │   ├── Program.cs                    Composition root (DI, pipeline)
│   │   ├── appsettings*.json             Configuration (no secrets)
│   │   └── wwwroot/                      Static files (Bootstrap and jQuery from the template)
│   ├── PatientManagement.Business/       Business layer
│   │   ├── Common/                       Result and message types (AD-11)
│   │   ├── Interfaces/                   Service interfaces
│   │   ├── Services/                     Service implementations
│   │   ├── DTOs/                         Data transfer objects
│   │   ├── Validators/                   Validators (errors and warnings)
│   │   └── BusinessServiceCollectionExtensions.cs
│   └── PatientManagement.Data/           Data layer
│       ├── Context/                      DbContext and data registration
│       ├── Entities/                     EF Core entities
│       ├── Repositories/                 Repositories (thin, one per aggregate)
│       └── Migrations/                   EF Core migrations
├── tests/
│   ├── PatientManagement.UnitTests/      Unit tests (xUnit, NSubstitute)
│   └── PatientManagement.IntegrationTests/  Web and data integration tests
├── database/
│   └── Scripts/                          SQL scripts (database creation only)
├── Documentation/                        Requirements, planning and implementation documents
├── BRD/                                  Business requirements (source of truth)
├── Directory.Build.props                 Shared settings and the Pkcs security pin
└── CLAUDE.md                             Operating rules for this repository
```

## 2. Dependency Rules (AD-02)

| From | May reference | Must not reference |
|---|---|---|
| `PatientManagement.Web` | `PatientManagement.Business`; `PatientManagement.Data` for `AddDataLayer` only | Entities or DbContext in controllers |
| `PatientManagement.Business` | `PatientManagement.Data` | `Microsoft.AspNetCore.*` |
| `PatientManagement.Data` | EF Core only | `PatientManagement.Business`, `PatientManagement.Web` |
| Test projects | The project they test | — |

Enforce by project references. A reference that breaks these rules is a defect, even if it builds.

## 3. Where Things Go

| You are adding | Put it in | Notes |
|---|---|---|
| A page or form | `Web/Controllers` (action), `Web/Views` (view), `Web/ViewModels` (model) | Map ViewModel to DTO in the controller, call one service |
| A business rule | `Business/Services` (rule), `Business/Validators` (checks) | Return `Result<T>`. Never throw for a validation outcome |
| A service contract | `Business/Interfaces` | One interface per service |
| A DTO | `Business/DTOs` | Business does not expose entities |
| An entity | `Data/Entities` | Add a `DbSet` only in the context that needs it |
| Data access | `Data/Repositories` | Thin wrapper over the DbContext. No generic repository (AD-04) |
| A schema change | `Data/Migrations` (generated) | See §5 |
| A registration | The layer's `*ServiceCollectionExtensions.cs` | Keep `Program.cs` short |

## 4. Dependency Injection

- `Program.cs` calls `AddBusinessLayer()` and `AddDataLayer(configuration)`. Each layer registers its own services.
- Register a new service in its layer's extension method, with the lifetime that fits:
  - Services and repositories: `Scoped` (one per HTTP request).
  - The DbContext is registered with `AddDbContext` (scoped).
- Do not use static service locators. Pass dependencies through constructors (CLAUDE.md §8).
- Pass `CancellationToken` through async calls.

## 5. Entity Framework Core

- EF Core only. Dapper is not used (PDR-32).
- The DbContext is `PatientManagementDbContext` in `PatientManagement.Data/Context`.
- No `DbSet` is declared yet. Add one with the entity it serves.
- Schema changes are made with migrations and committed with the change that needs them (CLAUDE.md §6).

Migration commands, for when the first entity exists. Install the matching tool first:

```
dotnet tool install --global dotnet-ef --version 10.0.14
dotnet ef migrations add <MigrationName> --project src/PatientManagement.Data --startup-project src/PatientManagement.Web
dotnet ef database update --project src/PatientManagement.Data --startup-project src/PatientManagement.Web
```

- `--project` is where the migration files go. `--startup-project` supplies the connection string.
- Name migrations for what they change, for example `AddPatients`.
- Check the generated SQL with `dotnet ef migrations script` before applying it.
- Not verified on this machine yet (no entities exist to migrate).

Constraints (from the planning documents):
- No check constraints on vital-sign ranges. Ranges are warnings (AD-14).
- No hard deletes. Appointments are archived (PDR-09).

## 6. Configuration Management

| Setting | Where | Committed? |
|---|---|---|
| Logging levels | `appsettings.json`, `appsettings.Development.json` | Yes |
| Allowed hosts | `appsettings.json` | Yes |
| Connection string (default) | `appsettings.json` (`ConnectionStrings:PatientManagement`) | Yes. Windows authentication, no password |
| Connection string (personal) | User secrets (`dotnet user-secrets`) | No |
| Passwords, keys, certificates | User secrets or environment variables | Never |
| Local launch URLs | `Properties/launchSettings.json` | Yes (change locally only if needed) |

Read configuration through `IConfiguration` or the options pattern. Do not hard-code values in code.

User secrets for this project are set with `--project src/PatientManagement.Web`. The project's `UserSecretsId` is in `PatientManagement.Web.csproj`.

## 7. Logging

- Use `ILogger<T>` injected through the constructor. Do not use `Console.WriteLine`.
- Use message templates with named placeholders: `_logger.LogInformation("Saved patient {PatientCode}", code);`
- Never log patient names, dates of birth, addresses, phone numbers, email addresses, passwords or connection strings (CLAUDE.md §8).
- Log levels: Information for normal events, Warning for recoverable problems, Error for failures with an exception.

## 8. Validation and Results (AD-11)

- Validators live in `Business/Validators`. They return errors and warnings, not exceptions.
- `Result<T>` in `Business/Common` has three outcomes: success, errors (blocked), or confirmation required (warnings that must be confirmed).
- Browser-side checks are a convenience. The server-side validator is the rule.

## 9. Testing

| Test project | What goes in | Tools |
|---|---|---|
| `PatientManagement.UnitTests` | Services, validators, mapping, `Result` | xUnit, NSubstitute |
| `PatientManagement.IntegrationTests` | Web pages and actions, data access against SQL Server | xUnit, `WebApplicationFactory<Program>` |

Rules (CLAUDE.md §10):
- Every test that verifies a requirement includes its ID in the name or a comment, for example `FR_PAT_001_...`.
- Infrastructure tests say so in a comment and name no requirement ID (see `ResultTests`, `HomePageTests`).
- Test data is synthetic. Never copy real patient data.
- Tests are deterministic. Inject the clock when time matters.
- Do not skip a failing test to get a build green.

Commands:

```
dotnet test PatientManagement.slnx
dotnet test tests/PatientManagement.UnitTests
dotnet test tests/PatientManagement.IntegrationTests
```

## 10. Code Standards

- Nullable reference types are on for all projects (`Directory.Build.props`).
- Use file-scoped namespaces, as in the existing files.
- Primary constructors are used where they make the class shorter (for example the DbContext).
- Async for all I/O. Pass `CancellationToken`.
- No commented-out code. No TODO without a requirement ID or decision reference.
- Follow existing patterns before adding new ones.

## 11. Commits and Workflow

- Work directly in `development`. Commit and push regularly (CLAUDE.md §5.4).
- Commit message format and types: `Documentation/03-Worktree/03-Commit-Standards.md`.
- Before pushing: `dotnet build` and `dotnet test` must pass.
- Commit only the files you changed. Check `git status` first.
- Never commit `bin/`, `obj/`, `.vs/`, user secrets or local `appsettings` overrides.

## 12. Things Not To Do

- Do not add a PDF library. Printable output is an HTML view with browser Print and Save as PDF (PDR-18).
- Do not add Dapper, a second ORM or a second database.
- Do not add a mobile or API surface (OOS-007; Phase 1 is MVC pages).
- Do not implement BRD features ahead of their work package (`Documentation/02-Planning/16-`).
- Do not put connection strings with passwords, or any secret, in source control.
