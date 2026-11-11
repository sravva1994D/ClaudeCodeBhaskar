using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for the single clinic profile row (PDR-15). Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ClinicProfileRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_RX_002_no_profile_is_returned_before_it_is_saved()
    {
        await using var context = fixture.CreateContext();

        Assert.Null(await new ClinicProfileRepository(context).GetAsync());
    }

    [Fact]
    public async Task FR_RX_002_the_first_save_adds_row_one()
    {
        await UpsertAsync(new ClinicProfile { ClinicName = "Test Clinic" });

        await using var reader = fixture.CreateContext();
        var saved = await new ClinicProfileRepository(reader).GetAsync();
        Assert.NotNull(saved);
        Assert.Equal(1, saved!.Id);
        Assert.Equal("Test Clinic", saved.ClinicName);
    }

    [Fact]
    public async Task FR_RX_002_a_later_save_updates_the_same_row()
    {
        await UpsertAsync(new ClinicProfile { ClinicName = "Test Clinic", DoctorName = "Test Doctor" });
        await UpsertAsync(new ClinicProfile { ClinicName = "Test Clinic Renamed", Phone = "5550199" });

        await using var reader = fixture.CreateContext();
        var saved = await new ClinicProfileRepository(reader).GetAsync();
        Assert.Equal("Test Clinic Renamed", saved!.ClinicName);
        Assert.Equal("5550199", saved.Phone);
        Assert.Null(saved.DoctorName);
        Assert.Equal(1, await reader.ClinicProfiles.CountAsync());
    }

    private async Task UpsertAsync(ClinicProfile profile)
    {
        await using var context = fixture.CreateContext();
        await new ClinicProfileRepository(context).UpsertAsync(profile);
    }

    private async Task Clear() => await fixture.ClearTablesAsync("ClinicProfile");
}
