namespace PetFinder.Contracts.Reports;

public sealed class PetReportSearchResultDto
{
    public IReadOnlyCollection<PetReportDto> Reports { get; set; } = [];
}
