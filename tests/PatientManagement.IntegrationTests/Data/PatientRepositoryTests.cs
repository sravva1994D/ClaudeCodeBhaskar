using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for patients against SQL Server. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PatientRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    [Fact]
    public async Task FR_PAT_004_search_by_name_part_finds_matching_patients()
    {
        await SeedAsync(("Test Anna Example", "5550101"), ("Test Ben Example", "5550102"));

        var results = await SearchAsync("Anna");

        Assert.Equal(["Test Anna Example"], results.Select(p => p.FullName));
    }

    [Fact]
    public async Task FR_PAT_004_search_by_phone_part_finds_matching_patients()
    {
        await SeedAsync(("Test Anna Example", "5550101"), ("Test Ben Example", "6660202"));

        var results = await SearchAsync("0202");

        Assert.Equal(["Test Ben Example"], results.Select(p => p.FullName));
    }

    [Fact]
    public async Task FR_PAT_004_search_ignores_case()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var results = await SearchAsync("ANNA");

        Assert.Single(results);
    }

    [Fact]
    public async Task FR_PAT_004_search_with_no_match_returns_nothing()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var results = await SearchAsync("Zara");

        Assert.Empty(results);
    }

    [Fact]
    public async Task FR_PAT_004_percent_in_the_term_is_literal_not_a_wildcard()
    {
        await SeedAsync(("Test Zed%Patient", "5550101"), ("Test Zed Patient", "5550102"));

        var results = await SearchAsync("%");

        Assert.Equal(["Test Zed%Patient"], results.Select(p => p.FullName));
    }

    [Fact]
    public async Task FR_PAT_004_underscore_in_the_term_is_literal_not_a_wildcard()
    {
        await SeedAsync(("Test A_B Patient", "5550101"), ("Test AXB Patient", "5550102"));

        var results = await SearchAsync("_");

        Assert.Equal(["Test A_B Patient"], results.Select(p => p.FullName));
    }

    [Fact]
    public async Task FR_PAT_004_backslash_in_the_term_is_literal()
    {
        await SeedAsync(("Test Back\\Slash Patient", "5550101"), ("Test Backslash Patient", "5550102"));

        var results = await SearchAsync("\\");

        Assert.Equal(["Test Back\\Slash Patient"], results.Select(p => p.FullName));
    }

    [Fact]
    public async Task FR_PAT_001_duplicate_check_matches_name_case_insensitively()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var duplicates = await DuplicatesAsync("TEST ANNA EXAMPLE", new DateOnly(1990, 5, 1), "5559999");

        Assert.Single(duplicates);
    }

    [Fact]
    public async Task FR_PAT_001_duplicate_check_matches_date_of_birth()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var duplicates = await DuplicatesAsync("Test Other Name", new DateOnly(1990, 5, 1), "5559999");

        Assert.Single(duplicates);
    }

    [Fact]
    public async Task FR_PAT_001_duplicate_check_matches_phone()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var duplicates = await DuplicatesAsync("Test Other Name", new DateOnly(1995, 3, 3), "5550101");

        Assert.Single(duplicates);
    }

    [Fact]
    public async Task FR_PAT_001_duplicate_check_ignores_unrelated_patients()
    {
        await SeedAsync(("Test Anna Example", "5550101"));

        var duplicates = await DuplicatesAsync("Test Other Name", new DateOnly(1995, 3, 3), "5559999");

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task FR_PAT_002_duplicate_check_excludes_the_patient_being_edited()
    {
        var id = (await SeedAsync(("Test Anna Example", "5550101"))).Single();

        await using var context = fixture.CreateContext();
        var repository = new PatientRepository(context);
        var duplicates = await repository.FindPossibleDuplicatesAsync(
            "Test Anna Example", new DateOnly(1990, 5, 1), "5550101", excludeId: id);

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task FR_PAT_001_add_saves_the_patient_and_sets_the_identity_and_code()
    {
        await using var context = fixture.CreateContext();
        var repository = new PatientRepository(context);
        var patient = NewPatient("Test Added Patient", "5550101");

        await repository.AddAsync(patient);

        Assert.True(patient.Id > 0);
        Assert.Equal($"PT-{patient.Id:D6}", patient.PatientCode);
    }

    [Fact]
    public async Task FR_PAT_002_update_saves_the_changes_and_keeps_the_patient_code()
    {
        var id = (await SeedAsync(("Test Anna Example", "5550101"))).Single();
        string originalCode;

        await using (var context = fixture.CreateContext())
        {
            var repository = new PatientRepository(context);
            var patient = await repository.GetByIdAsync(id);
            Assert.NotNull(patient);
            originalCode = patient.PatientCode;
            patient.Address = "Test updated address";
            await repository.UpdateAsync(patient);
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.Patients.SingleAsync(p => p.Id == id);
        Assert.Equal("Test updated address", saved.Address);
        Assert.Equal(originalCode, saved.PatientCode);
    }

    [Fact]
    public async Task FR_SRC_002_get_by_ids_returns_only_the_requested_patients()
    {
        var ids = await SeedAsync(("Test Anna Example", "5550101"), ("Test Ben Example", "5550102"), ("Test Cara Example", "5550103"));

        await using var context = fixture.CreateContext();
        var found = await new PatientRepository(context).GetByIdsAsync([ids[0], ids[2], 999999]);

        Assert.Equal(["Test Anna Example", "Test Cara Example"], found.Select(p => p.FullName).OrderBy(name => name));
    }

    private async Task<List<int>> SeedAsync(params (string FullName, string Phone)[] patients)
    {
        var ids = new List<int>();
        await using var context = fixture.CreateContext();
        foreach (var (fullName, phone) in patients)
        {
            var patient = NewPatient(fullName, phone);
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            ids.Add(patient.Id);
        }

        return ids;
    }

    private async Task<IReadOnlyList<Patient>> SearchAsync(string term)
    {
        await using var context = fixture.CreateContext();
        return await new PatientRepository(context).SearchAsync(term);
    }

    private async Task<IReadOnlyList<Patient>> DuplicatesAsync(string name, DateOnly dob, string phone)
    {
        await using var context = fixture.CreateContext();
        return await new PatientRepository(context).FindPossibleDuplicatesAsync(name, dob, phone);
    }

    private static Patient NewPatient(string fullName, string phone) => new()
    {
        FullName = fullName,
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = phone,
        Email = "patient@example.test",
        Address = "Test address"
    };
}
