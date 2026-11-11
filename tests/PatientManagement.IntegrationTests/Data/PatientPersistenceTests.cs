using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for the Patients table on SQL Server. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PatientPersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await fixture.ClearTablesAsync("Appointments", "Patients");

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("Appointments", "Patients");

    [Fact]
    public async Task FR_PAT_001_code_has_the_pt_format_with_six_digits()
    {
        var id = await AddPatientAsync("Test Patient One");

        await using var context = fixture.CreateContext();
        var patient = await context.Patients.SingleAsync(p => p.Id == id);

        Assert.Equal($"PT-{id:D6}", patient.PatientCode);
    }

    [Fact]
    public async Task FR_PAT_001_each_patient_gets_a_different_code()
    {
        await AddPatientAsync("Test Patient One");
        await AddPatientAsync("Test Patient Two");

        await using var context = fixture.CreateContext();
        var codes = await context.Patients.Select(p => p.PatientCode).ToListAsync();

        Assert.Equal(2, codes.Distinct().Count());
    }

    [Fact]
    public async Task FR_PAT_001_code_keeps_all_digits_beyond_999999()
    {
        await using (var context = fixture.CreateContext())
        {
            // Inserts an explicit identity value to test the rule without creating a million rows.
            await context.Database.ExecuteSqlRawAsync(
                "SET IDENTITY_INSERT [Patients] ON; " +
                "INSERT INTO [Patients] ([Id], [FullName], [DateOfBirth], [Gender], [Phone], [Email], [Address]) " +
                "VALUES (1000000, N'Test Patient Large', '1980-01-01', N'Other', N'5550100', N'large@example.test', N'Test address'); " +
                "SET IDENTITY_INSERT [Patients] OFF;");
        }

        await using var reader = fixture.CreateContext();
        var patient = await reader.Patients.SingleAsync(p => p.Id == 1000000);

        Assert.Equal("PT-1000000", patient.PatientCode);
    }

    [Fact]
    public async Task FR_PAT_001_gender_outside_the_allowed_list_is_refused_by_the_database()
    {
        await using var context = fixture.CreateContext();
        context.Patients.Add(NewPatient("Test Patient Gender", gender: "Unknown"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task FR_PAT_001_each_allowed_gender_is_accepted()
    {
        foreach (var gender in new[] { "Male", "Female", "Other" })
        {
            await using var context = fixture.CreateContext();
            context.Patients.Add(NewPatient($"Test {gender}", gender));
            await context.SaveChangesAsync();
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal(3, await reader.Patients.CountAsync());
    }

    private async Task<int> AddPatientAsync(string fullName)
    {
        await using var context = fixture.CreateContext();
        var patient = NewPatient(fullName);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    private static Patient NewPatient(string fullName, string gender = "Female") => new()
    {
        FullName = fullName,
        DateOfBirth = new DateOnly(1980, 1, 1),
        Gender = gender,
        Phone = "5550100",
        Email = "patient@example.test",
        Address = "Test address"
    };
}
