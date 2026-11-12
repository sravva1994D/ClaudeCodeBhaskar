# Database Scripts

Scripts for the local SQL Server database. Schema changes are made with EF Core migrations, not with these scripts (CLAUDE.md §6).

| Order | Script | Purpose | When to run |
|---|---|---|---|
| 1 | `01-create-database.sql` | Creates the `PatientManagement` database if missing | Once per machine, before the first migration |
| – | `04-generate-performance-data.sql` | Adds 1,000 synthetic patients with visits, vitals, prescriptions and appointments to the development database for the Module 8 timing checks (synthetic data only; `Documentation/04-Implementation/16-Module-8-Plan.md`) | Before the Module 8 timing run only |
| – | `05-remove-performance-data.sql` | Removes only the rows added by script 04 (patients named `Test Patient …` with an `@example.test` e-mail) | After the Module 8 timing run |

Run a script with `sqlcmd` (installed with SQL Server tools):

```
sqlcmd -S localhost -E -b -i database/Scripts/01-create-database.sql
```

`-E` uses Windows authentication. If your instance has a name (for example `localhost\SQLEXPRESS`), replace `localhost` with it. See `Documentation/04-Implementation/01-Setup-Guide.md`.

No patient data, passwords or connection strings are stored in these scripts.
