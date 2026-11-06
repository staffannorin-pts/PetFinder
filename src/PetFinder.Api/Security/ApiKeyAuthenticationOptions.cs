namespace PetFinder.Api.Security;

public sealed class ApiKeyAuthenticationOptions
{
    public const string SectionName = "ApiKeyAuthentication";
    public const string SchemeName = "ApiKey";

    public string HeaderName { get; set; } = "X-API-Key";

    public string[] Keys { get; set; } = [];
}
