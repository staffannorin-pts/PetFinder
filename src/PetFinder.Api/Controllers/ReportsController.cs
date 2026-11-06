using Microsoft.AspNetCore.Mvc;
using PetFinder.Api.Models;
using PetFinder.Api.Services;
using PetFinder.Api.Storage;
using PetFinder.Contracts.Reports;

namespace PetFinder.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IPetReportService service, IImageStorageService imageStorageService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PetReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PetReportDto>> Create([FromForm] CreatePetReportFormRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null)
        {
            var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Image)] = ["Image is required."]
            })
            {
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest
            };

            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return BadRequest(problemDetails);
        }

        var createRequest = new CreatePetReportDto
        {
            ReporterType = request.ReporterType,
            AnimalDescription = request.AnimalDescription,
            LocationName = request.LocationName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PetName = request.PetName
        };

        string imagePath;
        try
        {
            imagePath = await imageStorageService.SaveAsync(request.Image, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            var validation = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Image)] = [exception.Message]
            })
            {
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest
            };

            validation.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return BadRequest(validation);
        }

        var created = await service.CreateAsync(createRequest, imagePath, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PetReportDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var report = await service.GetByIdAsync(id, cancellationToken);
        if (report is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = "Report was not found.",
                Status = StatusCodes.Status404NotFound,
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            });
        }

        return Ok(report);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PetReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<PetReportDto>>> Search([FromQuery] PetReportSearchQuery query, CancellationToken cancellationToken)
    {
        var searchRequest = new PetReportSearchRequest
        {
            Latitude = query.Latitude,
            Longitude = query.Longitude,
            RadiusKm = query.RadiusKm
        };

        var reports = await service.SearchAsync(searchRequest, cancellationToken);
        return Ok(reports);
    }
}
