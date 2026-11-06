using Dapper;

namespace PetFinder.Api.Data;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS PetReports (
            Id TEXT PRIMARY KEY,
            ReporterType INTEGER NOT NULL,
            AnimalDescription TEXT NOT NULL,
            LocationName TEXT NOT NULL,
            Latitude REAL NOT NULL,
            Longitude REAL NOT NULL,
            PetName TEXT NULL,
            ImagePath TEXT NOT NULL,
            CreatedUtc TEXT NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_PetReports_CreatedUtc ON PetReports (CreatedUtc DESC);
        CREATE INDEX IF NOT EXISTS IX_PetReports_Location ON PetReports (Latitude, Longitude);
        """;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(CreateTableSql, cancellationToken: cancellationToken));
        logger.LogInformation("SQLite schema ensured for PetReports");
    }
}
