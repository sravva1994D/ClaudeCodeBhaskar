using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for the ClinicProfile table: a single row with Id = 1 (PDR-15; database CHECK), and a blank header
// is stored (the BRD does not say when the header must be complete). Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ClinicProfilePersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_RX_002_a_blank_clinic_profile_can_be_saved()
    {
        await using (var context = fixture.CreateContext())
        {
            context.ClinicProfiles.Add(new ClinicProfile { Id = 1 });
            await context.SaveChangesAsync();
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.ClinicProfiles.SingleAsync();
        Assert.Equal(1, saved.Id);
        Assert.Null(saved.ClinicName);
    }

    [Fact]
    public async Task FR_RX_002_the_clinic_profile_holds_only_row_one()
    {
        await using var context = fixture.CreateContext();
        context.ClinicProfiles.Add(new ClinicProfile { Id = 2 });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task FR_RX_002_header_fields_keep_their_lengths()
    {
        await using var context = fixture.CreateContext();
        context.ClinicProfiles.Add(new ClinicProfile { Id = 1, Phone = new string('5', 21) });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private async Task Clear() => await fixture.ClearTablesAsync("ClinicProfile");
}
