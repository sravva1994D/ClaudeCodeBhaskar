# Patient Management System (Phase 1)

A web-based patient management application for one general physician. Phase 1 is a localhost learning project. It is not deployed.

## Status

Phase 1 is implemented and accepted (Module 9, 2026-11-12). Scope and status: `Documentation/01-Brainstorming/12-Requirement-Traceability-Matrix.md`.

Target versions (PDR-31): .NET 10 SDK (LTS, `10.0.401`) and SQL Server 2022 Express (`16.0.1210.3`) on the verification machine. Details: `Documentation/04-Implementation/01-Setup-Guide.md`.

## Quick Start

```
dotnet restore PatientManagement.sln
dotnet build PatientManagement.sln
dotnet test PatientManagement.sln
sqlcmd -S "localhost\SQLEXPRESS" -E -b -i database/Scripts/01-create-database.sql
dotnet run --project src/PatientManagement.Web --launch-profile http
```

Then open `http://localhost:5092`.

## Documentation

| Document | Purpose |
|---|---|
| `BRD/Doc_BRD.md` | Business requirements (source of truth) |
| `CLAUDE.md` | Operating rules, Git workflow, standards |
| `Documentation/01-Brainstorming/` | Requirements analysis (Module 1) |
| `Documentation/02-Planning/` | Architecture, design and planning (Module 2) |
| `Documentation/03-Worktree/` | Git workflow, branching, commit, review, release and rollback standards (Module 3) |
| `Documentation/04-Implementation/` | Setup guide and developer guide (Module 4) |
| `Reference/` | Training plan |

Setup: `Documentation/04-Implementation/01-Setup-Guide.md`.
Development rules: `Documentation/04-Implementation/02-Developer-Guide.md`.

## Branch

All work is on `development` (CLAUDE.md §5). `main` receives the final release.
