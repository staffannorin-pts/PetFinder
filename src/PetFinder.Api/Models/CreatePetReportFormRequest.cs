using System.ComponentModel.DataAnnotations;
using PetFinder.Contracts.Reports;

namespace PetFinder.Api.Models;

public sealed class CreatePetReportFormRequest
{
    [Required]
    public ReporterType ReporterType { get; set; }

    [Required]
    public string AnimalDescription { get; set; } = string.Empty;

    [Required]
    public string LocationName { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public string? PetName { get; set; }

    [Required]
    public IFormFile? Image { get; set; }
}
