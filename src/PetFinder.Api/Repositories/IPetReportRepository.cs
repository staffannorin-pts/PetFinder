using PetFinder.Api.Domain;

namespace PetFinder.Api.Repositories;

public interface IPetReportRepository
{
    Task CreateAsync(PetReport report, CancellationToken cancellationToken);

    Task<PetReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PetReport>> SearchAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken);
}
