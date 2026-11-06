using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PetFinder.Contracts.Reports;

namespace PetFinder.Blazor.Services;

public sealed class PetReportsApiClient(HttpClient httpClient, IOptions<ApiClientOptions> options)
{
    private readonly ApiClientOptions _options = options.Value;

    public async Task<CreateReportResult> CreateAsync(CreatePetReportDto request, IBrowserFile image, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(request.ReporterType.ToString()), nameof(CreatePetReportDto.ReporterType));
        content.Add(new StringContent(request.AnimalDescription), nameof(CreatePetReportDto.AnimalDescription));
        content.Add(new StringContent(request.LocationName), nameof(CreatePetReportDto.LocationName));
        content.Add(new StringContent(request.Latitude.ToString(CultureInfo.InvariantCulture)), nameof(CreatePetReportDto.Latitude));
        content.Add(new StringContent(request.Longitude.ToString(CultureInfo.InvariantCulture)), nameof(CreatePetReportDto.Longitude));

        if (!string.IsNullOrWhiteSpace(request.PetName))
        {
            content.Add(new StringContent(request.PetName), nameof(CreatePetReportDto.PetName));
        }

        await using var stream = image.OpenReadStream(_options.MaxUploadBytes, cancellationToken);
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
        content.Add(streamContent, "Image", image.Name);

        var response = await httpClient.PostAsync("api/reports", content, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<PetReportDto>(cancellationToken);
            return new CreateReportResult
            {
                Success = true,
                Report = created
            };
        }

        var problem = await TryReadProblemDetailsAsync(response, cancellationToken);
        return new CreateReportResult
        {
            Success = false,
            Error = problem?.Detail ?? $"API request failed ({(int)response.StatusCode})."
        };
    }

    private static async Task<ProblemDetails?> TryReadProblemDetailsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType is null ||
            !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(stream, cancellationToken: cancellationToken);
    }
}
