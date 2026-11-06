using PetFinder.Contracts.Reports;

namespace PetFinder.Blazor.Services;

public sealed class CreateReportResult
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public PetReportDto? Report { get; set; }
}
