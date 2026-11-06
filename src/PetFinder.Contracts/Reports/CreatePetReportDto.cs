namespace PetFinder.Contracts.Reports;

public sealed class CreatePetReportDto
{
    public ReporterType ReporterType { get; set; }

    public string AnimalDescription { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? PetName { get; set; }
}
