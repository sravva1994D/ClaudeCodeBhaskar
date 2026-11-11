using NSubstitute;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the clinic header (FR-RX-002, PDR-15, D-M5-6). Repositories are substitutes. Synthetic values only.
// Test IDs follow CLAUDE.md §4.2 and §10.
public class ClinicProfileServiceTests
{
    private readonly IClinicProfileRepository clinicProfiles = Substitute.For<IClinicProfileRepository>();
    private readonly ClinicProfileService service;

    public ClinicProfileServiceTests()
    {
        service = new ClinicProfileService(clinicProfiles);
    }

    [Fact]
    public async Task FR_RX_002_the_first_read_creates_a_blank_profile()
    {
        clinicProfiles.GetOrCreateAsync(Arg.Any<CancellationToken>()).Returns(new ClinicProfile { Id = 1 });

        var profile = await service.GetAsync();

        Assert.False(profile.IsComplete);
        await clinicProfiles.Received(1).GetOrCreateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_update_saves_the_header_trimmed_and_blank_fields_as_no_text()
    {
        var result = await service.UpdateAsync(new ClinicProfileInput(
            "  Test Clinic  ", "Test address", "   ", "Test Doctor", "TEST-1"));

        Assert.True(result.IsSuccess);
        await clinicProfiles.Received(1).UpsertAsync(
            Arg.Is<ClinicProfile>(saved => saved.ClinicName == "Test Clinic" && saved.Phone == null && saved.DoctorName == "Test Doctor"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_header_field_longer_than_its_column_is_refused_and_nothing_is_saved()
    {
        var result = await service.UpdateAsync(new ClinicProfileInput(null, null, new string('5', 21), null, null));

        var error = Assert.Single(result.Errors);
        Assert.Equal("ClinicProfile.Phone.TooLong", error.Code);
        Assert.Equal("Phone", error.Field);
        await clinicProfiles.DidNotReceive().UpsertAsync(Arg.Any<ClinicProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_blank_header_is_saved_and_reported_as_incomplete()
    {
        var result = await service.UpdateAsync(new ClinicProfileInput(null, null, null, null, null));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsComplete);
        await clinicProfiles.Received(1).UpsertAsync(Arg.Any<ClinicProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_complete_header_is_reported_as_complete()
    {
        var result = await service.UpdateAsync(new ClinicProfileInput(
            "Test Clinic", "Test address", "5550199", "Test Doctor", "TEST-1"));

        Assert.True(result.Value!.IsComplete);
    }
}
