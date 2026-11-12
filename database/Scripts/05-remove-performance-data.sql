/*
  05-remove-performance-data.sql
  Removes the synthetic seed-performance data added by 04-generate-performance-data.sql
  (Documentation/04-Implementation/16-Module-8-Plan.md; D-M8-1).

  Only patients named "Test Patient ..." with an e-mail on example.test are removed, together with
  their visits, vitals, prescriptions, prescription items and appointments. A real visit or appointment
  added to one of these test patients would be removed too, so do not add real records to them.

  Run:
    sqlcmd -S <your-instance> -E -b -i database/Scripts/05-remove-performance-data.sql -d PatientManagement
*/

SET QUOTED_IDENTIFIER ON;  -- required for DML on tables with filtered indexes
SET XACT_ABORT ON;
SET NOCOUNT ON;

IF DB_NAME() <> N'PatientManagement'
BEGIN
    THROW 50000, N'This script only runs against the PatientManagement development database.', 1;
END

BEGIN TRANSACTION;

DECLARE @Marked TABLE ([PatientId] int NOT NULL PRIMARY KEY);

INSERT INTO @Marked ([PatientId])
SELECT [Id] FROM [Patients]
WHERE [Email] LIKE N'%@example.test' AND [FullName] LIKE N'Test Patient %';

DELETE pi
FROM [PrescriptionItems] AS pi
INNER JOIN [Prescriptions] AS rx ON rx.[Id] = pi.[PrescriptionId]
INNER JOIN [Consultations] AS c ON c.[Id] = rx.[ConsultationId]
WHERE c.[PatientId] IN (SELECT [PatientId] FROM @Marked);

DELETE rx
FROM [Prescriptions] AS rx
INNER JOIN [Consultations] AS c ON c.[Id] = rx.[ConsultationId]
WHERE c.[PatientId] IN (SELECT [PatientId] FROM @Marked);

DELETE v
FROM [VitalSigns] AS v
INNER JOIN [Consultations] AS c ON c.[Id] = v.[ConsultationId]
WHERE c.[PatientId] IN (SELECT [PatientId] FROM @Marked);

DELETE FROM [Consultations] WHERE [PatientId] IN (SELECT [PatientId] FROM @Marked);
DELETE FROM [Appointments] WHERE [PatientId] IN (SELECT [PatientId] FROM @Marked);
DELETE FROM [Patients] WHERE [Id] IN (SELECT [PatientId] FROM @Marked);

COMMIT TRANSACTION;

SELECT (SELECT COUNT(*) FROM [Patients] WHERE [Email] LIKE N'%@example.test') AS [SyntheticPatientsLeft];
GO
