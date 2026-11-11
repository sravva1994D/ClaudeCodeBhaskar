using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

public sealed class PatientService(
    IPatientRepository patients,
    AgeCalculator ages,
    IConsultationRepository consultations,
    TimeProvider timeProvider) : IPatientService
{
    public const string NotFoundCode = "Patient.NotFound";
    public const string NotFoundMessage = "The patient was not found.";
    public const string DuplicateCode = "Patient.Duplicate";

    public async Task<Result<PatientDto>> RegisterAsync(
        PatientInput input, bool confirmWarnings, CancellationToken cancellationToken = default)
    {
        var errors = PatientValidator.Validate(input, Today());
        if (errors.Count > 0)
        {
            return Result<PatientDto>.Fail(errors.ToArray());
        }

        if (!confirmWarnings)
        {
            var duplicates = await FindDuplicatesAsync(input, excludeId: null, cancellationToken);
            if (duplicates.Count > 0)
            {
                return Result<PatientDto>.ConfirmationRequired(DuplicateWarnings(duplicates));
            }
        }

        var patient = PatientMapper.ToNewEntity(input);
        await patients.AddAsync(patient, cancellationToken);

        return Result<PatientDto>.Ok(ToDto(patient));
    }

    public async Task<Result<PatientDto>> UpdateAsync(
        int id, PatientInput input, bool confirmWarnings, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        var errors = PatientValidator.Validate(input, Today());
        if (errors.Count > 0)
        {
            return Result<PatientDto>.Fail(errors.ToArray());
        }

        if (!confirmWarnings)
        {
            var duplicates = await FindDuplicatesAsync(input, excludeId: id, cancellationToken);
            if (duplicates.Count > 0)
            {
                return Result<PatientDto>.ConfirmationRequired(DuplicateWarnings(duplicates));
            }
        }

        PatientMapper.ApplyInput(input, patient);
        await patients.UpdateAsync(patient, cancellationToken);

        return Result<PatientDto>.Ok(ToDto(patient));
    }

    public async Task<Result<PatientDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        // The profile shows the last visit (D-M4-8). Register and update results do not show it.
        var lastVisit = await consultations.GetLatestVisitTimeAsync(id, cancellationToken);

        return Result<PatientDto>.Ok(ToDto(patient, lastVisit));
    }

    public async Task<Result<IReadOnlyList<PatientSummaryDto>>> SearchAsync(
        string? term, CancellationToken cancellationToken = default)
    {
        var errors = PatientValidator.ValidateSearchTerm(term);
        if (errors.Count > 0)
        {
            return Result<IReadOnlyList<PatientSummaryDto>>.Fail(errors.ToArray());
        }

        var found = await patients.SearchAsync(term!.Trim(), cancellationToken);
        IReadOnlyList<PatientSummaryDto> summaries = found.Select(PatientMapper.ToSummary).ToList();

        return Result<IReadOnlyList<PatientSummaryDto>>.Ok(summaries);
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    private PatientDto ToDto(Patient patient, DateTime? lastVisit = null) =>
        PatientMapper.ToDto(patient, ages.CurrentAge(patient.DateOfBirth), lastVisit);

    private Task<IReadOnlyList<Patient>> FindDuplicatesAsync(
        PatientInput input, int? excludeId, CancellationToken cancellationToken) =>
        patients.FindPossibleDuplicatesAsync(
            input.FullName!.Trim(), input.DateOfBirth!.Value, input.Phone!.Trim(), excludeId, cancellationToken);

    private static IReadOnlyList<ResultMessage> DuplicateWarnings(IReadOnlyList<Patient> duplicates) =>
        duplicates
            .Select(patient => new ResultMessage(
                DuplicateCode,
                $"Possible duplicate: {patient.PatientCode} {patient.FullName}, born {patient.DateOfBirth:yyyy-MM-dd}, phone {patient.Phone}.",
                "Duplicate"))
            .ToList();

    private static Result<PatientDto> NotFound() =>
        Result<PatientDto>.Fail(new ResultMessage(NotFoundCode, NotFoundMessage));
}
