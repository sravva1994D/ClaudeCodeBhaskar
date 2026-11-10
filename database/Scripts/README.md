# Database Scripts

Scripts for the local SQL Server database. Schema changes are made with EF Core migrations, not with these scripts (CLAUDE.md §6).

| Order | Script | Purpose | When to run |
|---|---|---|---|
| 1 | `01-create-database.sql` | Creates the `PatientManagement` database if missing | Once per machine, before the first migration |

Run a script with `sqlcmd` (installed with SQL Server tools):

```
sqlcmd -S localhost -E -b -i database/Scripts/01-create-database.sql
```

`-E` uses Windows authentication. If your instance has a name (for example `localhost\SQLEXPRESS`), replace `localhost` with it. See `Documentation/04-Implementation/01-Setup-Guide.md`.

No patient data, passwords or connection strings are stored in these scripts.
