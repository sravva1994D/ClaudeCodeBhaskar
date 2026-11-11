using PatientManagement.Business.Validators;

namespace PatientManagement.UnitTests.Validators;

// Unit tests for the vitals rules (FR-VIT-001; PDR-19; D-M4-4; AD-14). Boundaries are tested on both sides.
// Test IDs follow CLAUDE.md §4.2 and §10.
public class VitalsValidatorTests
{
    private static VitalsInput Normal() => new(36.8m, 120, 80, 72);

    [Fact]
    public void FR_VIT_001_complete_normal_vitals_have_no_errors_warnings_or_notices()
    {
        var input = Normal();

        Assert.Empty(VitalsValidator.Errors(input));
        Assert.Empty(VitalsValidator.Warnings(input));
        Assert.Empty(VitalsValidator.Notices(input));
    }

    [Theory]
    [InlineData(33.9, true)]
    [InlineData(34.0, false)]
    [InlineData(42.0, false)]
    [InlineData(42.1, true)]
    public void FR_VIT_001_temperature_warns_only_outside_34_to_42(decimal temperature, bool warns)
    {
        var warnings = VitalsValidator.Warnings(new VitalsInput(temperature, 120, 80, 72));

        Assert.Equal(warns, warnings.Any(w => w.Field == "TemperatureC"));
    }

    [Theory]
    [InlineData(69, true)]
    [InlineData(70, false)]
    [InlineData(250, false)]
    [InlineData(251, true)]
    public void FR_VIT_001_systolic_warns_only_outside_70_to_250(int systolic, bool warns)
    {
        var warnings = VitalsValidator.Warnings(new VitalsInput(36.8m, systolic, 80, 72));

        Assert.Equal(warns, warnings.Any(w => w.Field == "SystolicMmHg"));
    }

    [Theory]
    [InlineData(39, true)]
    [InlineData(40, false)]
    [InlineData(150, false)]
    [InlineData(151, true)]
    public void FR_VIT_001_diastolic_warns_only_outside_40_to_150(int diastolic, bool warns)
    {
        var warnings = VitalsValidator.Warnings(new VitalsInput(36.8m, 120, diastolic, 72));

        Assert.Equal(warns, warnings.Any(w => w.Field == "DiastolicMmHg"));
    }

    [Theory]
    [InlineData(29, true)]
    [InlineData(30, false)]
    [InlineData(220, false)]
    [InlineData(221, true)]
    public void FR_VIT_001_pulse_warns_only_outside_30_to_220(int pulse, bool warns)
    {
        var warnings = VitalsValidator.Warnings(new VitalsInput(36.8m, 120, 80, pulse));

        Assert.Equal(warns, warnings.Any(w => w.Field == "PulseBpm"));
    }

    [Fact]
    public void FR_VIT_001_each_out_of_range_value_gives_its_own_warning()
    {
        var warnings = VitalsValidator.Warnings(new VitalsInput(43.0m, 260, 160, 240));

        Assert.Equal(4, warnings.Count);
        Assert.All(warnings, w => Assert.Equal("Vitals.Range.OutOfRange", w.Code));
        Assert.Equal(["TemperatureC", "SystolicMmHg", "DiastolicMmHg", "PulseBpm"], warnings.Select(w => w.Field));
    }

    [Fact]
    public void FR_VIT_001_out_of_range_warnings_show_the_entered_value_and_units()
    {
        // TC-FR-VIT-001-4: units are shown in the message, and the value is shown as entered.
        var temperature = Assert.Single(VitalsValidator.Warnings(new VitalsInput(43.0m, 120, 80, 72)));
        var systolic = Assert.Single(VitalsValidator.Warnings(new VitalsInput(36.8m, 260, 80, 72)));

        Assert.Contains("43.0 °C", temperature.Message);
        Assert.Contains("260 mmHg", systolic.Message);
        Assert.Contains("Confirm to save.", systolic.Message);
    }

    [Fact]
    public void FR_VIT_001_missing_vitals_give_a_notice_not_a_warning_or_error()
    {
        // TC-FR-VIT-001-2: nothing entered is saved; the notice is shown and no confirm is needed (PDR-19).
        var input = new VitalsInput(null, null, null, null);

        Assert.Empty(VitalsValidator.Errors(input));
        Assert.Empty(VitalsValidator.Warnings(input));
        Assert.Equal(["Vitals.Missing"], VitalsValidator.Notices(input).Select(n => n.Code));
    }

    [Fact]
    public void FR_VIT_001_a_partial_set_is_refused_and_names_the_missing_fields()
    {
        var error = Assert.Single(VitalsValidator.Errors(new VitalsInput(36.8m, 120, null, 72)));

        Assert.Equal("Vitals.Incomplete", error.Code);
        Assert.Contains("diastolic pressure", error.Message);
        Assert.Equal("DiastolicMmHg", error.Field);
    }

    [Fact]
    public void FR_VIT_001_blood_pressure_needs_both_numbers_when_only_systolic_is_entered()
    {
        Assert.Equal(["Vitals.Incomplete"], VitalsValidator.Errors(new VitalsInput(36.8m, 120, null, 72)).Select(e => e.Code));
    }

    [Fact]
    public void FR_VIT_001_blood_pressure_needs_both_numbers_when_only_diastolic_is_entered()
    {
        Assert.Equal(["Vitals.Incomplete"], VitalsValidator.Errors(new VitalsInput(36.8m, null, 80, 72)).Select(e => e.Code));
    }

    [Fact]
    public void FR_VIT_001_temperature_with_one_decimal_place_is_accepted()
    {
        Assert.Empty(VitalsValidator.Errors(new VitalsInput(37.5m, 120, 80, 72)));
    }

    [Fact]
    public void FR_VIT_001_temperature_with_two_decimal_places_is_refused()
    {
        var error = Assert.Single(VitalsValidator.Errors(new VitalsInput(37.25m, 120, 80, 72)));

        Assert.Equal("Vitals.Temperature.Precision", error.Code);
        Assert.Equal("TemperatureC", error.Field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void FR_VIT_001_a_temperature_that_cannot_be_a_body_temperature_is_refused(decimal temperature)
    {
        Assert.Equal(["Vitals.Temperature.Invalid"],
            VitalsValidator.Errors(new VitalsInput(temperature, 120, 80, 72)).Select(e => e.Code));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1000)]
    public void FR_VIT_001_a_pressure_or_pulse_that_cannot_be_a_value_is_refused(int value)
    {
        Assert.Equal(["Vitals.Value.Invalid"],
            VitalsValidator.Errors(new VitalsInput(36.8m, value, 80, 72)).Select(e => e.Code));
    }

    [Fact]
    public void FR_VIT_001_out_of_range_values_are_not_errors()
    {
        // AD-14: a value outside the expected range is a warning. Only impossible values are refused.
        Assert.Empty(VitalsValidator.Errors(new VitalsInput(43.0m, 260, 160, 240)));
    }
}
