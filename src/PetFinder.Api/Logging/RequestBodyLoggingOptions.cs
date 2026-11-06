namespace PetFinder.Api.Logging;

public sealed class RequestBodyLoggingOptions
{
    public const string SectionName = "RequestBodyLogging";

    public bool Enabled { get; set; } = true;

    public int MaxBodyLength { get; set; } = 2048;

    public string[] AllowedContentTypes { get; set; } = ["application/json", "text/plain"];

    public string[] AllowedPathPrefixes { get; set; } = ["/api"];
}
