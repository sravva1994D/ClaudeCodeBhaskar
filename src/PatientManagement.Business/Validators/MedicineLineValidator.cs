using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Validators;

// Medicine line rules (Documentation/02-Planning/13-Validation-Strategy.md §3.5; FR-RX-001; PDR-13; D-M5-2, D-M5-3).
// A line with every field empty is ignored. Any other line needs a medicine name. Lengths match the database columns.
// Errors name the line by its position in the submitted list, so the form can show them beside the right row.
public static class MedicineLineValidator
{
    public const int MaxLines = 20;
    public const int NameMaxLength = 200;
    public const int DosageMaxLength = 100;
    public const int FrequencyMaxLength = 100;
    public const int DurationMaxLength = 100;
    public const int InstructionsMaxLength = 500;

    public static IReadOnlyList<ResultMessage> Validate(IReadOnlyList<MedicineLineInput> lines)
    {
        var errors = new List<ResultMessage>();
        var used = 0;

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            if (IsBlank(line))
            {
                continue;
            }

            used++;
            var number = index + 1;
            var field = $"Lines[{index}]";

            if (Trim(line.Name) is null)
            {
                errors.Add(new ResultMessage("Medicine.Name.Required",
                    $"Line {number}: enter the medicine name.", $"{field}.Name"));
            }

            AddLengthError(line.Name, NameMaxLength, "medicine name", "Name", number, field, errors);
            AddLengthError(line.Dosage, DosageMaxLength, "dosage", "Dosage", number, field, errors);
            AddLengthError(line.Frequency, FrequencyMaxLength, "frequency", "Frequency", number, field, errors);
            AddLengthError(line.Duration, DurationMaxLength, "duration", "Duration", number, field, errors);
            AddLengthError(line.Instructions, InstructionsMaxLength, "instructions", "Instructions", number, field, errors);
        }

        if (used > MaxLines)
        {
            errors.Add(new ResultMessage("Medicine.Lines.TooMany",
                $"A prescription can have at most {MaxLines} medicines.", "Lines"));
        }

        return errors;
    }

    // The lines to save: blank lines dropped and text trimmed. Call only when Validate returns no errors.
    public static IReadOnlyList<MedicineLineInput> Clean(IReadOnlyList<MedicineLineInput> lines) =>
        lines
            .Where(line => !IsBlank(line))
            .Select(line => new MedicineLineInput(
                Trim(line.Name), Trim(line.Dosage), Trim(line.Frequency), Trim(line.Duration), Trim(line.Instructions)))
            .ToList();

    private static bool IsBlank(MedicineLineInput line) =>
        Trim(line.Name) is null && Trim(line.Dosage) is null && Trim(line.Frequency) is null
        && Trim(line.Duration) is null && Trim(line.Instructions) is null;

    private static string? Trim(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static void AddLengthError(
        string? text, int maxLength, string label, string field, int number, string prefix, List<ResultMessage> errors)
    {
        if (Trim(text) is { } value && value.Length > maxLength)
        {
            errors.Add(new ResultMessage("Medicine." + field + ".TooLong",
                $"Line {number}: the {label} must be at most {maxLength} characters.", $"{prefix}.{field}"));
        }
    }
}
