namespace PetFinder.Contracts.Reports;

public sealed class PetReportSearchRequest
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double RadiusKm { get; set; } = 5;
}
