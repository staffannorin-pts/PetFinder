using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PetFinder.Tests.Integration;

public sealed class SecurityAndRateLimitingTests : IClassFixture<PetFinderApiFactory>
{
    private readonly HttpClient _client;

    public SecurityAndRateLimitingTests(PetFinderApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task MissingApiKey_ReturnsUnauthorizedProblemDetails()
    {
        var response = await _client.GetAsync("/api/reports?latitude=59.3&longitude=18.0&radiusKm=5");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(details);
        Assert.Equal("Unauthorized", details.Title);
        Assert.False(string.IsNullOrWhiteSpace(details.Detail));
    }

    [Fact]
    public async Task ValidApiKey_AllowsAccess()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports?latitude=59.3&longitude=18.0&radiusKm=5");
        request.Headers.Add("X-API-Key", "integration-test-key");

        var response = await _client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RateLimitEventuallyReturns429()
    {
        HttpStatusCode finalStatus = HttpStatusCode.OK;

        for (var i = 0; i < 80; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports?latitude=59.3&longitude=18.0&radiusKm=5");
            request.Headers.Add("X-API-Key", "integration-test-key");

            var response = await _client.SendAsync(request);
            finalStatus = response.StatusCode;

            if (finalStatus == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, finalStatus);
    }
}
