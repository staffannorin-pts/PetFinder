namespace PetFinder.Blazor.Services;

public sealed class ApiClientOptions
{
    public const string SectionName = "ApiClient";

    public string BaseUrl { get; set; } = "https://localhost:5001/";

    public string ApiKey { get; set; } = string.Empty;

    public long MaxUploadBytes { get; set; } = 5 * 1024 * 1024;
}
