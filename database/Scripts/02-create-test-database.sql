/*
  02-create-test-database.sql
  Creates the PatientManagement_Test database used only by the integration tests.
  No tables are created here. The test fixture applies EF Core migrations to it
  (Documentation/04-Implementation/03-Module-1-Plan.md, section 3A).
  Run once per machine. Safe to run again.

  Run from a command prompt (Windows authentication; use your instance name):
    sqlcmd -S <your-instance> -E -b -i database/Scripts/02-create-test-database.sql
*/

IF DB_ID(N'PatientManagement_Test') IS NULL
BEGIN
    CREATE DATABASE [PatientManagement_Test];
    PRINT N'Database PatientManagement_Test created.';
END
ELSE
BEGIN
    PRINT N'Database PatientManagement_Test already exists. No change made.';
END
GO
