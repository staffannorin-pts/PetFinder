namespace PetFinder.Contracts.Reports;

public sealed class PetReportDto
{
    public Guid Id { get; set; }

    public ReporterType ReporterType { get; set; }

    public string AnimalDescription { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? PetName { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; set; }
}
