using PatientManagement.Business.DTOs;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.DTOs;

// Unit tests for the patient mapping (FR-PAT-003, FR-PAT-004). Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class PatientMapperTests
{
    private static Patient ExistingPatient() => new()
    {
        Id = 123,
        PatientCode = "PT-000123",
        FullName = "Test Anna Example",
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = "5550101",
        Email = "anna@example.test",
        Address = "Test address"
    };

    [Fact]
    public void FR_PAT_003_profile_dto_keeps_every_field_and_adds_the_age()
    {
        var dto = PatientMapper.ToDto(ExistingPatient(), age: 36);

        Assert.Equal(123, dto.Id);
        Assert.Equal("PT-000123", dto.PatientCode);
        Assert.Equal("Test Anna Example", dto.FullName);
        Assert.Equal(new DateOnly(1990, 5, 1), dto.DateOfBirth);
        Assert.Equal(36, dto.Age);
        Assert.Equal("Female", dto.Gender);
        Assert.Equal("5550101", dto.Phone);
        Assert.Equal("anna@example.test", dto.Email);
        Assert.Equal("Test address", dto.Address);
    }

    [Fact]
    public void FR_PAT_004_search_row_has_only_the_fields_shown_in_the_results()
    {
        var summary = PatientMapper.ToSummary(ExistingPatient());

        var properties = typeof(PatientSummaryDto).GetProperties().Select(p => p.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "DateOfBirth", "FullName", "Id", "PatientCode", "Phone" }, properties);
        Assert.Equal("PT-000123", summary.PatientCode);
        Assert.Equal("5550101", summary.Phone);
    }

    [Fact]
    public void FR_PAT_001_new_entity_takes_the_entered_values_with_surrounding_spaces_removed()
    {
        var input = new PatientInput(
            FullName: "  Test Anna Example  ",
            DateOfBirth: new DateOnly(1990, 5, 1),
            Gender: "Female",
            Phone: " 5550101 ",
            Email: " anna@example.test ",
            Address: " Test address ");

        var patient = PatientMapper.ToNewEntity(input);

        Assert.Equal("Test Anna Example", patient.FullName);
        Assert.Equal("5550101", patient.Phone);
        Assert.Equal("anna@example.test", patient.Email);
        Assert.Equal("Test address", patient.Address);
        Assert.Equal(0, patient.Id);
        Assert.Equal(string.Empty, patient.PatientCode);
    }

    [Fact]
    public void FR_PAT_002_applying_input_changes_the_values_but_not_the_identity_or_code()
    {
        var patient = ExistingPatient();
        var input = new PatientInput(
            FullName: "Test Anna Changed",
            DateOfBirth: new DateOnly(1991, 6, 2),
            Gender: "Other",
            Phone: "5550199",
            Email: "changed@example.test",
            Address: "Changed address");

        PatientMapper.ApplyInput(input, patient);

        Assert.Equal(123, patient.Id);
        Assert.Equal("PT-000123", patient.PatientCode);
        Assert.Equal("Test Anna Changed", patient.FullName);
        Assert.Equal(new DateOnly(1991, 6, 2), patient.DateOfBirth);
        Assert.Equal("Other", patient.Gender);
        Assert.Equal("5550199", patient.Phone);
        Assert.Equal("changed@example.test", patient.Email);
        Assert.Equal("Changed address", patient.Address);
    }
}
