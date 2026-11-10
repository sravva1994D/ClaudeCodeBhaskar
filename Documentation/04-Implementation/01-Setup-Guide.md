# 01 — Setup Guide

**Project:** Patient Management System, Phase 1 (localhost learning project)
**Applies to:** a Windows developer machine with SQL Server installed locally
**Status:** Skeleton setup verified on 2026-11-10

This guide gets the solution building, testing and running on your machine. It covers the skeleton only. No BRD feature is implemented yet.

---

## 1. Prerequisites

| Item | Version used | How to check | Notes |
|---|---|---|---|
| .NET SDK | 10.0.401 (LTS) | `dotnet --version` | Install the .NET 10 SDK. Other SDKs can stay installed |
| SQL Server | 16.0.1210.3 (SQL Server 2022, Express) | `sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT SERVERPROPERTY('ProductVersion')"` | Instance name `localhost\SQLEXPRESS` used here. Your instance name may differ |
| sqlcmd | Shipped with SQL Server tools | `sqlcmd -?` | Used to run database scripts |
| Git | Any recent version | `git --version` | Required to get the code |
| dotnet-ef | 10.0.14 (optional now) | `dotnet ef --version` | Needed only when migrations are added (see Developer Guide) |

Target versions are set by decision PDR-31: latest LTS .NET at the start of implementation, and the local SQL Server version. Both are recorded here.

## 2. Get the Code

```
git clone https://github.com/sravva1994D/ClaudeCodeBhaskar.git
cd ClaudeCodeBhaskar
git switch development
git pull --ff-only origin development
```

All work happens on `development` (CLAUDE.md §5). Do not create other branches.

## 3. Build and Test

From the repository root:

```
dotnet restore PatientManagement.slnx
dotnet build PatientManagement.slnx
dotnet test PatientManagement.slnx
```

Expected result:
- Build: 0 warnings, 0 errors.
- Tests: 6 passed (4 unit, 2 integration). The integration tests start the web app in memory and do not need SQL Server.

## 4. Configure the Connection String

The connection string is in `src/PatientManagement.Web/appsettings.json` under `ConnectionStrings:PatientManagement`. It uses Windows authentication and holds no password.

If your SQL Server instance has a different name, override it on your machine with user secrets. Do not edit the committed file for a personal setting.

```
dotnet user-secrets set "ConnectionStrings:PatientManagement" "Server=localhost\SQLEXPRESS;Database=PatientManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True" --project src/PatientManagement.Web
```

Replace `localhost\SQLEXPRESS` with your instance name (for example `(localdb)\MSSQLLocalDB` or `localhost`). Secrets are stored outside the repository (CLAUDE.md §9).

Check which secrets are set (values are not shown by this command):

```
dotnet user-secrets list --project src/PatientManagement.Web
```

## 5. Create the Database

Run once per machine:

```
sqlcmd -S "localhost\SQLEXPRESS" -E -b -i database/Scripts/01-create-database.sql
```

Output on first run: `Database PatientManagement created.` Running it again prints `already exists. No change made.`

Schema is created by EF Core migrations, which arrive with the feature work. The script creates the empty database only.

## 6. Run the Web App

```
dotnet run --project src/PatientManagement.Web --launch-profile http
```

Open `http://localhost:5092` in a browser. The home page shows "Patient Management System" and the line "Phase 1 solution skeleton. No BRD features are implemented yet."

The `http` profile sets `ASPNETCORE_ENVIRONMENT=Development`, which loads `appsettings.Development.json`. Without a profile, the app runs in Production mode.

## 7. Logging

- Logs go to the console through the built-in logging provider.
- Levels are set in `appsettings.json` and, for Development, `appsettings.Development.json`.
- Entity Framework SQL statements are shown at Information level only when the Development setting allows it. The Development file sets them to Warning, to keep the console readable.
- On startup the app logs: `PatientManagement.Web starting in <environment> environment.`

No patient data or secrets are written to logs (CLAUDE.md §8).

## 8. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `Connection string 'PatientManagement' is not configured` | Connection string missing | Check `appsettings.json` or set the user secret (§4) |
| `Server is not found or not accessible` | Wrong instance name, or SQL Server not running | `Get-Service MSSQL*` in PowerShell; check the instance name with `sqlcmd -L` |
| `Cannot open database "PatientManagement"` | Script in §5 not run | Run `01-create-database.sql` (§5) |
| Port 5092 in use | Another app uses the port | Change `applicationUrl` in `Properties/launchSettings.json` on your machine only |
| Build warnings about `System.Security.Cryptography.Pkcs` | Transitive package version | Pinned in `Directory.Build.props`. Do not remove the pin without checking advisories |
| `dotnet ef` not found | Tool not installed | Install it when migrations are needed (Developer Guide §5) |

## 9. What Was Verified (2026-11-10)

| Check | Result |
|---|---|
| `dotnet build PatientManagement.slnx` | Succeeded, 0 warnings, 0 errors |
| `dotnet test PatientManagement.slnx` | 4 unit tests and 2 integration tests passed |
| Web app start (Development and Production) | Starts, logs startup line, serves home page (HTTP 200) |
| `01-create-database.sql` on `localhost\SQLEXPRESS` | Creates database; second run makes no change |
| EF Core migration tooling | Not verified yet. No entities exist, so there is nothing to migrate |
