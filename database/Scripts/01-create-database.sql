/*
  01-create-database.sql
  Creates the local PatientManagement database if it does not exist.
  No tables are created here. Schema is created by EF Core migrations
  (CLAUDE.md §6). Run once per machine. Safe to run again.

  Run from a command prompt (Windows authentication, default local instance):
    sqlcmd -S localhost -E -b -i database/Scripts/01-create-database.sql
*/

IF DB_ID(N'PatientManagement') IS NULL
BEGIN
    CREATE DATABASE [PatientManagement];
    PRINT N'Database PatientManagement created.';
END
ELSE
BEGIN
    PRINT N'Database PatientManagement already exists. No change made.';
END
GO
