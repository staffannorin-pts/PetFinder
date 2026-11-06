using Dapper;
using PetFinder.Api.Data;
using PetFinder.Api.Domain;

namespace PetFinder.Api.Repositories;

public sealed class PetReportRepository(IDbConnectionFactory connectionFactory) : IPetReportRepository
{
    private const string InsertSql = """
        INSERT INTO PetReports
        (
            Id,
            ReporterType,
            AnimalDescription,
            LocationName,
            Latitude,
            Longitude,
            PetName,
            ImagePath,
            CreatedUtc
        )
        VALUES
        (
            @Id,
            @ReporterType,
            @AnimalDescription,
            @LocationName,
            @Latitude,
            @Longitude,
            @PetName,
            @ImagePath,
            @CreatedUtc
        );
        """;

    private const string SelectByIdSql = """
        SELECT
            Id,
            ReporterType,
            AnimalDescription,
            LocationName,
            Latitude,
            Longitude,
            PetName,
            ImagePath,
            CreatedUtc
        FROM PetReports
        WHERE Id = @Id;
        """;

    private const string SearchSql = """
        SELECT
            Id,
            ReporterType,
            AnimalDescription,
            LocationName,
            Latitude,
            Longitude,
            PetName,
            ImagePath,
            CreatedUtc
        FROM PetReports
        WHERE
            Latitude BETWEEN @MinLatitude AND @MaxLatitude
            AND Longitude BETWEEN @MinLongitude AND @MaxLongitude
        ORDER BY CreatedUtc DESC
        LIMIT 200;
        """;

    public async Task CreateAsync(PetReport report, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(InsertSql, report, cancellationToken: cancellationToken));
    }

    public async Task<PetReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<PetReport>(
            new CommandDefinition(SelectByIdSql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<PetReport>> SearchAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken)
    {
        var latDegrees = radiusKm / 111d;
        var lonDegrees = radiusKm / (111d * Math.Max(Math.Cos(latitude * Math.PI / 180d), 0.01d));

        var parameters = new
        {
            MinLatitude = latitude - latDegrees,
            MaxLatitude = latitude + latDegrees,
            MinLongitude = longitude - lonDegrees,
            MaxLongitude = longitude + lonDegrees
        };

        using var connection = connectionFactory.CreateConnection();
        var reports = await connection.QueryAsync<PetReport>(
            new CommandDefinition(SearchSql, parameters, cancellationToken: cancellationToken));

        return reports.ToList();
    }
}
