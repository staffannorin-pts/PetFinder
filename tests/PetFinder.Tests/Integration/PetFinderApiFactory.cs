using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PetFinder.Tests.Integration;

public sealed class PetFinderApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"petfinder-test-{Guid.NewGuid():N}.db");
    private readonly string _imagePath = Path.Combine(Path.GetTempPath(), $"petfinder-images-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ApiKeyAuthentication:Keys:0"] = "integration-test-key",
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["ImageStorage:BasePath"] = _imagePath,
                ["RequestBodyLogging:Enabled"] = "false"
            };

            configurationBuilder.AddInMemoryCollection(settings);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        TryDeleteFile(_databasePath);
        TryDeleteDirectory(_imagePath);
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
