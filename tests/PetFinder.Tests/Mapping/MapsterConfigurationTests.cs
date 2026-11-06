using Mapster;
using PetFinder.Api.Domain;
using PetFinder.Api.Mapping;
using PetFinder.Contracts.Reports;

namespace PetFinder.Tests.Mapping;

public sealed class MapsterConfigurationTests
{
    [Fact]
    public void RegisterMappings_MapsImagePathToImageUrl()
    {
        var config = MapsterConfiguration.RegisterMappings();

        var source = new PetReport
        {
            Id = Guid.NewGuid(),
            ReporterType = ReporterType.Owner,
            AnimalDescription = "Golden retriever",
            LocationName = "Gothenburg",
            Latitude = 57.7,
            Longitude = 11.97,
            PetName = "Max",
            ImagePath = "/images/max.jpg",
            CreatedUtc = DateTimeOffset.UtcNow
        };

        var result = source.Adapt<PetReportDto>(config);

        Assert.Equal(source.ImagePath, result.ImageUrl);
    }
}
