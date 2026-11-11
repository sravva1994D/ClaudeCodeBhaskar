/*
  03-generate-test-patients.sql
  Replaces the patients in the TEST database with 1,000 synthetic patients, for the search timing check
  (Documentation/04-Implementation/05-Module-2-Plan.md, step 9; NFR-PRF-002; decision D-M2-5).
  Synthetic names, phone numbers and addresses only. Never run this against the development database.
  The script stops unless the database name ends in _Test.

  Run:
    sqlcmd -S <your-instance> -E -b -i database/Scripts/03-generate-test-patients.sql -d PatientManagement_Test
*/

IF DB_NAME() NOT LIKE N'%[_]Test'
BEGIN
    THROW 50000, N'This script only runs against a database whose name ends in _Test.', 1;
END

-- Test data only. Patients are the only table with rows in Phase 1 test data.
DELETE FROM [Patients];

DECLARE @i int = 1;

WHILE @i <= 1000
BEGIN
    INSERT INTO [Patients] ([FullName], [DateOfBirth], [Gender], [Phone], [Email], [Address])
    VALUES (
        CONCAT(N'Test Patient ', RIGHT(N'0000' + CAST(@i AS nvarchar(4)), 4), N' ',
               CHOOSE(1 + @i % 10, N'Anderson', N'Brown', N'Clark', N'Davis', N'Evans',
                                   N'Foster', N'Green', N'Harris', N'Ibrahim', N'Jones')),
        DATEADD(DAY, (@i * 37) % 20000, CAST(N'1950-01-01' AS date)),
        CHOOSE(1 + @i % 3, N'Male', N'Female', N'Other'),
        CONCAT(N'55', RIGHT(N'00000000' + CAST(@i AS nvarchar(8)), 8)),
        CONCAT(N'test.patient', @i, N'@example.test'),
        CONCAT(N'Test address ', @i));

    SET @i += 1;
END

SELECT COUNT(*) AS [SyntheticPatients] FROM [Patients];
GO
