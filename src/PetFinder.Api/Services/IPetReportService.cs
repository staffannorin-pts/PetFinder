using PetFinder.Contracts.Reports;

namespace PetFinder.Api.Services;

public interface IPetReportService
{
    Task<PetReportDto> CreateAsync(CreatePetReportDto request, string imagePath, CancellationToken cancellationToken);

    Task<PetReportDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PetReportDto>> SearchAsync(PetReportSearchRequest request, CancellationToken cancellationToken);
}
