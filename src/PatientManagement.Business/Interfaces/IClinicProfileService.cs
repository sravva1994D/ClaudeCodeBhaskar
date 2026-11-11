using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// The clinic header (Documentation/02-Planning/10-Service-Design.md §2.8; FR-RX-002, PDR-15).
public interface IClinicProfileService
{
    // The header. The single row is created blank on first read.
    Task<ClinicProfileDto> GetAsync(CancellationToken cancellationToken = default);

    Task<Result<ClinicProfileDto>> UpdateAsync(ClinicProfileInput input, CancellationToken cancellationToken = default);
}
