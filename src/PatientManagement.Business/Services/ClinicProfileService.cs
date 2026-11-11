using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

// Clinic header (FR-RX-002, PDR-15). The single row is created blank the first time it is read.
public sealed class ClinicProfileService(IClinicProfileRepository clinicProfiles) : IClinicProfileService
{
    public async Task<ClinicProfileDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await clinicProfiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            profile = new ClinicProfile();
            await clinicProfiles.UpsertAsync(profile, cancellationToken);
        }

        return ClinicProfileMapper.ToDto(profile);
    }

    public async Task<Result<ClinicProfileDto>> UpdateAsync(ClinicProfileInput input, CancellationToken cancellationToken = default)
    {
        var errors = ClinicProfileValidator.Validate(input);
        if (errors.Count > 0)
        {
            return Result<ClinicProfileDto>.Fail(errors.ToArray());
        }

        var profile = ClinicProfileMapper.ToEntity(input);
        await clinicProfiles.UpsertAsync(profile, cancellationToken);

        return Result<ClinicProfileDto>.Ok(ClinicProfileMapper.ToDto(profile));
    }
}
