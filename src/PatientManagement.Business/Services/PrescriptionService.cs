using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

// Prescription rules (FR-RX-001, FR-RX-002; PDR-13, PDR-14; AD-13). The service decides; the repository's conditional writes
// only backstop races. Printed prescriptions cannot change (D-M5-4).
public sealed class PrescriptionService(
    IPrescriptionRepository prescriptions,
    IConsultationRepository consultations,
    IPatientRepository patients,
    IClinicProfileRepository clinicProfiles,
    TimeProvider timeProvider) : IPrescriptionService
{
    public const string PrintedCode = VisitErrors.PrintedCode;
    public const string PrintedMessage = "This prescription has been printed, so it can no longer be changed.";
    public const string NoMedicineCode = "Prescription.NoMedicine";
    public const string NoMedicineMessage = "Add at least one medicine before printing.";

    public async Task<Result<PrescriptionDto>> SaveItemsAsync(
        int consultationId, IReadOnlyList<MedicineLineInput> lines, CancellationToken cancellationToken = default)
    {
        if (await consultations.GetByIdAsync(consultationId, cancellationToken: cancellationToken) is null)
        {
            return Result<PrescriptionDto>.Fail(NotFound());
        }

        var existing = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);
        if (existing is { IsPrinted: true })
        {
            return Result<PrescriptionDto>.Fail(Printed());
        }

        var errors = MedicineLineValidator.Validate(lines);
        if (errors.Count > 0)
        {
            return Result<PrescriptionDto>.Fail(errors.ToArray());
        }

        var cleaned = MedicineLineValidator.Clean(lines);
        var prescriptionId = existing?.Id ?? await EnsurePrescriptionAsync(consultationId, cancellationToken);
        var items = cleaned.Select((line, index) => PrescriptionMapper.ToItem(line, index + 1)).ToList();

        // Refused when the prescription was printed after the read above (AD-13).
        if (!await prescriptions.ReplaceItemsAsync(prescriptionId, items, cancellationToken))
        {
            return Result<PrescriptionDto>.Fail(Printed());
        }

        var saved = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);

        return Result<PrescriptionDto>.Ok(PrescriptionMapper.ToDto(saved!));
    }

    public async Task<Result<PrescriptionPrintDto>> MarkPrintedAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        var context = await LoadPrintContextAsync(consultationId, cancellationToken);
        if (context is null)
        {
            return Result<PrescriptionPrintDto>.Fail(NotFound());
        }

        var (visit, patient) = context.Value;
        var prescription = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);
        if (prescription is null || prescription.Items.Count == 0)
        {
            return Result<PrescriptionPrintDto>.Fail(NoMedicine());
        }

        if (!prescription.IsPrinted)
        {
            // The flag is set once (PDR-14). The mark is refused when the lines were removed after the read above.
            // A refused mark is an error only when the prescription is still unprinted; if another request printed it first, that is fine.
            var marked = await prescriptions.MarkPrintedAsync(prescription.Id, Now(), cancellationToken);
            prescription = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);
            if (prescription is null || (!marked && !prescription.IsPrinted))
            {
                return Result<PrescriptionPrintDto>.Fail(NoMedicine());
            }
        }

        return Result<PrescriptionPrintDto>.Ok(
            PrescriptionMapper.ToPrint(visit, patient, prescription, await ClinicAsync(cancellationToken)));
    }

    public async Task<PrescriptionDto?> GetAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);

        return prescription is null ? null : PrescriptionMapper.ToDto(prescription);
    }

    public async Task<Result<PrescriptionPrintDto>> GetPrintModelAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        var context = await LoadPrintContextAsync(consultationId, cancellationToken);
        if (context is null)
        {
            return Result<PrescriptionPrintDto>.Fail(NotFound());
        }

        var (visit, patient) = context.Value;
        var prescription = await prescriptions.GetByConsultationAsync(consultationId, cancellationToken);
        if (prescription is null || prescription.Items.Count == 0)
        {
            return Result<PrescriptionPrintDto>.Fail(NoMedicine());
        }

        return Result<PrescriptionPrintDto>.Ok(
            PrescriptionMapper.ToPrint(visit, patient, prescription, await ClinicAsync(cancellationToken)));
    }

    private async Task<(Consultation Visit, Patient Patient)?> LoadPrintContextAsync(int consultationId, CancellationToken cancellationToken)
    {
        var visit = await consultations.GetByIdAsync(consultationId, includeVitals: true, cancellationToken);
        if (visit is null)
        {
            return null;
        }

        var patient = await patients.GetByIdAsync(visit.PatientId, cancellationToken);

        return patient is null ? null : (visit, patient);
    }

    private async Task<int> EnsurePrescriptionAsync(int consultationId, CancellationToken cancellationToken)
    {
        var prescription = new Prescription { ConsultationId = consultationId };
        try
        {
            await prescriptions.AddAsync(prescription, cancellationToken);
            return prescription.Id;
        }
        catch (PrescriptionAlreadyExistsException)
        {
            // Another first save won the race. Its prescription is the one to fill (FR-RX-002 AC-3).
            return (await prescriptions.GetByConsultationAsync(consultationId, cancellationToken))!.Id;
        }
    }

    private async Task<ClinicProfileDto> ClinicAsync(CancellationToken cancellationToken) =>
        ClinicProfileMapper.ToDto(await clinicProfiles.GetAsync(cancellationToken));

    private DateTime Now() => timeProvider.GetLocalNow().DateTime;

    private static ResultMessage NotFound() => new(VisitErrors.NotFoundCode, VisitErrors.NotFoundMessage);

    private static ResultMessage Printed() => new(PrintedCode, PrintedMessage);

    private static ResultMessage NoMedicine() => new(NoMedicineCode, NoMedicineMessage);
}
