using Mapster;
using PetFinder.Api.Domain;
using PetFinder.Api.Repositories;
using PetFinder.Contracts.Reports;

namespace PetFinder.Api.Services;

public sealed class PetReportService(IPetReportRepository repository, TypeAdapterConfig typeAdapterConfig) : IPetReportService
{
    public async Task<PetReportDto> CreateAsync(CreatePetReportDto request, string imagePath, CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request, imagePath);

        var report = request.Adapt<PetReport>(typeAdapterConfig);
        report.Id = Guid.NewGuid();
        report.CreatedUtc = DateTimeOffset.UtcNow;
        report.ImagePath = imagePath;

        if (report.ReporterType == ReporterType.Observer)
        {
            report.PetName = null;
        }

        await repository.CreateAsync(report, cancellationToken);

        return report.Adapt<PetReportDto>(typeAdapterConfig);
    }

    public async Task<PetReportDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await repository.GetByIdAsync(id, cancellationToken);
        return report?.Adapt<PetReportDto>(typeAdapterConfig);
    }

    public async Task<IReadOnlyCollection<PetReportDto>> SearchAsync(PetReportSearchRequest request, CancellationToken cancellationToken)
    {
        ValidateSearchRequest(request);

        var reports = await repository.SearchAsync(request.Latitude, request.Longitude, request.RadiusKm, cancellationToken);
        return reports.Adapt<IReadOnlyCollection<PetReportDto>>(typeAdapterConfig);
    }

    private static void ValidateCreateRequest(CreatePetReportDto request, string imagePath)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.AnimalDescription))
        {
            errors[nameof(request.AnimalDescription)] = ["AnimalDescription is required."];
        }

        if (string.IsNullOrWhiteSpace(request.LocationName))
        {
            errors[nameof(request.LocationName)] = ["LocationName is required."];
        }

        if (request.Latitude is < -90 or > 90)
        {
            errors[nameof(request.Latitude)] = ["Latitude must be between -90 and 90."];
        }

        if (request.Longitude is < -180 or > 180)
        {
            errors[nameof(request.Longitude)] = ["Longitude must be between -180 and 180."];
        }

        if (request.ReporterType == ReporterType.Owner && string.IsNullOrWhiteSpace(request.PetName))
        {
            errors[nameof(request.PetName)] = ["PetName is required for owner reports."];
        }

        if (string.IsNullOrWhiteSpace(imagePath))
        {
            errors[nameof(imagePath)] = ["ImagePath is required."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException("Validation failed.", errors);
        }
    }

    private static void ValidateSearchRequest(PetReportSearchRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Latitude is < -90 or > 90)
        {
            errors[nameof(request.Latitude)] = ["Latitude must be between -90 and 90."];
        }

        if (request.Longitude is < -180 or > 180)
        {
            errors[nameof(request.Longitude)] = ["Longitude must be between -180 and 180."];
        }

        if (request.RadiusKm <= 0 || request.RadiusKm > 100)
        {
            errors[nameof(request.RadiusKm)] = ["RadiusKm must be greater than 0 and less than or equal to 100."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException("Validation failed.", errors);
        }
    }
}
