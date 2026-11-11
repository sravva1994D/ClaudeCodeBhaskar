using PatientManagement.Business.DTOs;
using PatientManagement.Business.Validators;

namespace PatientManagement.UnitTests.Validators;

// Unit tests for medicine line rules: blank lines, the name, column lengths and the line limit (FR-RX-001, PDR-13).
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class MedicineLineValidatorTests
{
    [Fact]
    public void FR_RX_001_a_name_alone_is_a_valid_line()
    {
        Assert.Empty(MedicineLineValidator.Validate([new MedicineLineInput("Test Medicine", null, null, null, null)]));
    }

    [Fact]
    public void FR_RX_001_a_line_with_every_field_empty_is_ignored()
    {
        var lines = new[]
        {
            new MedicineLineInput("Test Medicine", null, null, null, null),
            new MedicineLineInput("   ", "  ", null, "", null),
            new MedicineLineInput("Test Second", null, null, null, null)
        };

        Assert.Empty(MedicineLineValidator.Validate(lines));
        Assert.Equal(["Test Medicine", "Test Second"], MedicineLineValidator.Clean(lines).Select(line => line.Name));
    }

    [Fact]
    public void FR_RX_001_a_line_with_a_dosage_but_no_name_needs_a_name()
    {
        var error = Assert.Single(MedicineLineValidator.Validate([new MedicineLineInput(null, "Test dose", null, null, null)]));

        Assert.Equal("Medicine.Name.Required", error.Code);
        Assert.Equal("Lines[0].Name", error.Field);
    }

    [Fact]
    public void FR_RX_001_the_error_names_the_line_by_its_position_in_the_list()
    {
        var error = Assert.Single(MedicineLineValidator.Validate(
        [
            new MedicineLineInput(null, null, null, null, null),
            new MedicineLineInput(null, "Test dose", null, null, null)
        ]));

        Assert.Equal("Lines[1].Name", error.Field);
    }

    [Fact]
    public void FR_RX_001_a_name_of_200_characters_is_valid_and_201_is_refused()
    {
        Assert.Empty(MedicineLineValidator.Validate([new MedicineLineInput(new string('A', 200), null, null, null, null)]));

        var error = Assert.Single(MedicineLineValidator.Validate([new MedicineLineInput(new string('A', 201), null, null, null, null)]));
        Assert.Equal("Medicine.Name.TooLong", error.Code);
    }

    [Fact]
    public void FR_RX_001_a_dosage_of_101_characters_is_refused()
    {
        var error = Assert.Single(MedicineLineValidator.Validate(
            [new MedicineLineInput("Test Medicine", new string('D', 101), null, null, null)]));

        Assert.Equal("Medicine.Dosage.TooLong", error.Code);
        Assert.Equal("Lines[0].Dosage", error.Field);
    }

    [Fact]
    public void FR_RX_001_instructions_of_500_characters_are_valid_and_501_are_refused()
    {
        Assert.Empty(MedicineLineValidator.Validate([new MedicineLineInput("Test Medicine", null, null, null, new string('I', 500))]));

        var error = Assert.Single(MedicineLineValidator.Validate(
            [new MedicineLineInput("Test Medicine", null, null, null, new string('I', 501))]));
        Assert.Equal("Medicine.Instructions.TooLong", error.Code);
    }

    [Fact]
    public void FR_RX_001_more_than_twenty_medicines_are_refused()
    {
        var lines = Enumerable.Range(1, MedicineLineValidator.MaxLines + 1)
            .Select(number => new MedicineLineInput($"Test Medicine {number}", null, null, null, null))
            .ToList();

        var error = Assert.Single(MedicineLineValidator.Validate(lines));
        Assert.Equal("Medicine.Lines.TooMany", error.Code);
    }

    [Fact]
    public void FR_RX_001_twenty_medicines_are_allowed()
    {
        var lines = Enumerable.Range(1, MedicineLineValidator.MaxLines)
            .Select(number => new MedicineLineInput($"Test Medicine {number}", null, null, null, null))
            .ToList();

        Assert.Empty(MedicineLineValidator.Validate(lines));
    }
}
