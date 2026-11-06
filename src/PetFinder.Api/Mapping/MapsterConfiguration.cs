using Mapster;
using PetFinder.Api.Domain;
using PetFinder.Contracts.Reports;

namespace PetFinder.Api.Mapping;

public static class MapsterConfiguration
{
    public static TypeAdapterConfig RegisterMappings()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<PetReport, PetReportDto>()
            .Map(destination => destination.ImageUrl, source => source.ImagePath);

        config.NewConfig<CreatePetReportDto, PetReport>();

        return config;
    }
}
