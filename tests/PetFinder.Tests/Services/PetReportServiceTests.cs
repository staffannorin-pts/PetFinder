using Mapster;
using PetFinder.Api.Domain;
using PetFinder.Api.Mapping;
using PetFinder.Api.Repositories;
using PetFinder.Api.Services;
using PetFinder.Contracts.Reports;

namespace PetFinder.Tests.Services;

public sealed class PetReportServiceTests
{
    [Fact]
    public async Task CreateAsync_OwnerWithoutPetName_ThrowsValidationException()
    {
        var service = CreateService(out _);

        var request = new CreatePetReportDto
        {
            ReporterType = ReporterType.Owner,
            AnimalDescription = "Brown dog",
            LocationName = "Central Park",
            Latitude = 59.33,
            Longitude = 18.06
        };

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            service.CreateAsync(request, "/images/test.jpg", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Observer_ClearsPetName()
    {
        var service = CreateService(out var repository);

        var request = new CreatePetReportDto
        {
            ReporterType = ReporterType.Observer,
            AnimalDescription = "Black cat",
            LocationName = "Sodermalm",
            Latitude = 59.31,
            Longitude = 18.07,
            PetName = "ShouldBeRemoved"
        };

        var created = await service.CreateAsync(request, "/images/cat.jpg", CancellationToken.None);

        Assert.NotNull(created);
        Assert.Null(created.PetName);
        Assert.NotNull(repository.LastCreated);
        Assert.Null(repository.LastCreated!.PetName);
    }

    private static PetReportService CreateService(out InMemoryPetReportRepository repository)
    {
        repository = new InMemoryPetReportRepository();
        var config = MapsterConfiguration.RegisterMappings();
        return new PetReportService(repository, config);
    }

    private sealed class InMemoryPetReportRepository : IPetReportRepository
    {
        private readonly List<PetReport> _reports = [];

        public PetReport? LastCreated { get; private set; }

        public Task CreateAsync(PetReport report, CancellationToken cancellationToken)
        {
            LastCreated = report;
            _reports.Add(report);
            return Task.CompletedTask;
        }

        public Task<PetReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_reports.SingleOrDefault(r => r.Id == id));
        }

        public Task<IReadOnlyCollection<PetReport>> SearchAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PetReport>>(_reports);
        }
    }
}
