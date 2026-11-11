using System.Globalization;
using PatientManagement.Business.Common;

namespace PatientManagement.Business.Validators;

// Vital sign rules (Documentation/02-Planning/10-Service-Design.md §2.4 vitals table; 13-Validation-Strategy §3.4).
// Three results, kept apart so the service can apply AD-11:
//   Errors: block the save (partial entry, impossible values).
//   Warnings: out-of-range values; the save waits for "Confirm and save" (FR-VIT-001 AC-5).
//   Notices: no vitals at all; the save continues without a confirm step (PDR-19).
// Ranges are warnings, never database checks (AD-14). Values are never changed here.
public static class VitalsValidator
{
    public const decimal TemperatureMin = 34.0m;
    public const decimal TemperatureMax = 42.0m;
    public const int SystolicMin = 70;
    public const int SystolicMax = 250;
    public const int DiastolicMin = 40;
    public const int DiastolicMax = 150;
    public const int PulseMin = 30;
    public const int PulseMax = 220;

    // Upper limit for any entered value. Beyond this the number cannot be a vital sign and would not fit the stored column.
    private const decimal TemperatureEntryMax = 99.9m;
    private const int ValueEntryMax = 999;

    public static bool IsEmpty(VitalsInput input) =>
        input.TemperatureC is null && input.SystolicMmHg is null && input.DiastolicMmHg is null && input.PulseBpm is null;

    public static IReadOnlyList<ResultMessage> Errors(VitalsInput input)
    {
        if (IsEmpty(input))
        {
            return [];
        }

        var errors = new List<ResultMessage>();

        // D-M4-4: all four vitals or none. Blood pressure therefore needs both numbers.
        var missing = new List<string>();
        if (input.TemperatureC is null) missing.Add("temperature");
        if (input.SystolicMmHg is null) missing.Add("systolic pressure");
        if (input.DiastolicMmHg is null) missing.Add("diastolic pressure");
        if (input.PulseBpm is null) missing.Add("pulse");

        if (missing.Count > 0)
        {
            errors.Add(new ResultMessage("Vitals.Incomplete",
                $"Enter all four vitals, or leave all four empty. Missing: {string.Join(", ", missing)}.",
                FirstMissingField(input)));

            return errors;
        }

        if (input.TemperatureC is { } temperature)
        {
            if (temperature <= 0 || temperature > TemperatureEntryMax)
            {
                errors.Add(new ResultMessage("Vitals.Temperature.Invalid",
                    $"Enter a temperature between 0.1 and {TemperatureEntryMax.ToString("0.0", CultureInfo.InvariantCulture)} °C.", "TemperatureC"));
            }
            else if (Math.Round(temperature, 1) != temperature)
            {
                // The database stores one decimal place (decimal(4,1)); a second decimal would be lost silently.
                errors.Add(new ResultMessage("Vitals.Temperature.Precision",
                    "Enter the temperature to one decimal place, for example 37.5.", "TemperatureC"));
            }
        }

        AddEntryError(input.SystolicMmHg, "Systolic pressure", "SystolicMmHg", errors);
        AddEntryError(input.DiastolicMmHg, "Diastolic pressure", "DiastolicMmHg", errors);
        AddEntryError(input.PulseBpm, "Pulse", "PulseBpm", errors);

        return errors;
    }

    // Only meaningful when Errors is empty. Each out-of-range value gives one warning.
    public static IReadOnlyList<ResultMessage> Warnings(VitalsInput input)
    {
        var warnings = new List<ResultMessage>();

        if (input.TemperatureC is { } temperature && (temperature < TemperatureMin || temperature > TemperatureMax))
        {
            warnings.Add(new ResultMessage("Vitals.Range.OutOfRange",
                $"Temperature {temperature.ToString("0.0", CultureInfo.InvariantCulture)} °C is outside the expected range {TemperatureMin.ToString("0.0", CultureInfo.InvariantCulture)}–{TemperatureMax.ToString("0.0", CultureInfo.InvariantCulture)} °C. Confirm to save.",
                "TemperatureC"));
        }

        AddRangeWarning(input.SystolicMmHg, SystolicMin, SystolicMax, "Systolic pressure", "mmHg", "SystolicMmHg", warnings);
        AddRangeWarning(input.DiastolicMmHg, DiastolicMin, DiastolicMax, "Diastolic pressure", "mmHg", "DiastolicMmHg", warnings);
        AddRangeWarning(input.PulseBpm, PulseMin, PulseMax, "Pulse", "bpm", "PulseBpm", warnings);

        return warnings;
    }

    // The notice shown when no vitals were entered. It does not stop the save (PDR-19).
    public static IReadOnlyList<ResultMessage> Notices(VitalsInput input) =>
        IsEmpty(input)
            ? [new ResultMessage("Vitals.Missing", "No vitals recorded. You can add them later.")]
            : [];

    private static void AddEntryError(int? value, string label, string field, List<ResultMessage> errors)
    {
        if (value is { } number && (number <= 0 || number > ValueEntryMax))
        {
            errors.Add(new ResultMessage("Vitals.Value.Invalid",
                $"Enter a {label.ToLowerInvariant()} between 1 and {ValueEntryMax}.", field));
        }
    }

    private static void AddRangeWarning(
        int? value, int min, int max, string label, string unit, string field, List<ResultMessage> warnings)
    {
        if (value is { } number && (number < min || number > max))
        {
            warnings.Add(new ResultMessage("Vitals.Range.OutOfRange",
                $"{label} {number} {unit} is outside the expected range {min}–{max} {unit}. Confirm to save.",
                field));
        }
    }

    private static string? FirstMissingField(VitalsInput input)
    {
        if (input.TemperatureC is null) return "TemperatureC";
        if (input.SystolicMmHg is null) return "SystolicMmHg";
        if (input.DiastolicMmHg is null) return "DiastolicMmHg";
        return "PulseBpm";
    }
}

// The vitals as entered on the form. Each value is optional until the validator checks the set.
public sealed record VitalsInput(decimal? TemperatureC, int? SystolicMmHg, int? DiastolicMmHg, int? PulseBpm);
