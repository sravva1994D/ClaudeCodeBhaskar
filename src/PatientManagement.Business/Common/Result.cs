namespace PatientManagement.Business.Common;

/// <summary>
/// A message from a validator or service: a stable code, a readable text and an optional field name.
/// </summary>
public sealed record ResultMessage(string Code, string Message, string? Field = null);

/// <summary>
/// Outcome of a service call (AD-11).
/// Errors block the save. Warnings are shown, and where the BRD requires it, the save waits for confirmation.
/// </summary>
public sealed class Result<T>
{
    private Result(T? value, IReadOnlyList<ResultMessage> errors, IReadOnlyList<ResultMessage> warnings, bool requiresConfirmation)
    {
        Value = value;
        Errors = errors;
        Warnings = warnings;
        RequiresConfirmation = requiresConfirmation;
    }

    public T? Value { get; }

    public IReadOnlyList<ResultMessage> Errors { get; }

    public IReadOnlyList<ResultMessage> Warnings { get; }

    /// <summary>True when the work was done. False when there are errors or confirmation is still needed.</summary>
    public bool IsSuccess => Errors.Count == 0 && !RequiresConfirmation;

    /// <summary>True when warnings must be confirmed before the work is saved. Nothing has been saved.</summary>
    public bool RequiresConfirmation { get; }

    public static Result<T> Ok(T value) => new(value, [], [], false);

    /// <summary>Success with warnings that were shown and accepted (for example, a missing-vitals notice).</summary>
    public static Result<T> Ok(T value, IEnumerable<ResultMessage> warnings) => new(value, [], warnings.ToArray(), false);

    public static Result<T> Fail(params ResultMessage[] errors) => new(default, errors, [], false);

    /// <summary>Warnings that the user must confirm. Nothing has been saved.</summary>
    public static Result<T> ConfirmationRequired(IEnumerable<ResultMessage> warnings) => new(default, [], warnings.ToArray(), true);
}
