using PatientManagement.Business.Common;

namespace PatientManagement.UnitTests.Common;

// Infrastructure test for the shared Result type (AD-11). It verifies no BRD requirement.
public class ResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessWithValueAndNoMessages()
    {
        var result = Result<string>.Ok("saved");

        Assert.True(result.IsSuccess);
        Assert.False(result.RequiresConfirmation);
        Assert.Equal("saved", result.Value);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Ok_WithWarnings_IsSuccessfulAndKeepsWarnings()
    {
        var warning = new ResultMessage("VITALS_MISSING", "No vitals recorded.", "Vitals");

        var result = Result<string>.Ok("saved", [warning]);

        Assert.True(result.IsSuccess);
        Assert.Equal("saved", result.Value);
        Assert.Equal(warning, Assert.Single(result.Warnings));
    }

    [Fact]
    public void Fail_ReturnsErrorsAndNoValue()
    {
        var error = new ResultMessage("NAME_REQUIRED", "Name is required.", "FullName");

        var result = Result<string>.Fail(error);

        Assert.False(result.IsSuccess);
        Assert.False(result.RequiresConfirmation);
        Assert.Null(result.Value);
        Assert.Equal(error, Assert.Single(result.Errors));
    }

    [Fact]
    public void ConfirmationRequired_IsNotSuccessfulAndHasNoValue()
    {
        var warning = new ResultMessage("TEMPERATURE_OUT_OF_RANGE", "Temperature is outside the expected range.", "TemperatureC");

        var result = Result<string>.ConfirmationRequired([warning]);

        Assert.False(result.IsSuccess);
        Assert.True(result.RequiresConfirmation);
        Assert.Null(result.Value);
        Assert.Empty(result.Errors);
        Assert.Equal(warning, Assert.Single(result.Warnings));
    }
}
