using System.Threading.RateLimiting;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using PetFinder.Api.Data;
using PetFinder.Api.ExceptionHandling;
using PetFinder.Api.Logging;
using PetFinder.Api.Mapping;
using PetFinder.Api.Repositories;
using PetFinder.Api.Security;
using PetFinder.Api.Services;
using PetFinder.Api.Storage;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PetFinder.Api");
});

builder.Services.Configure<RequestBodyLoggingOptions>(builder.Configuration.GetSection(RequestBodyLoggingOptions.SectionName));
builder.Services.Configure<ApiKeyAuthenticationOptions>(builder.Configuration.GetSection(ApiKeyAuthenticationOptions.SectionName));
builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));

builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.SchemeName,
        _ => { });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.SchemeName)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(MapsterConfiguration.RegisterMappings());
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<IPetReportRepository, PetReportRepository>();
builder.Services.AddScoped<IPetReportService, PetReportService>();
builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.Request.Headers.TryGetValue("X-API-Key", out var value)
            ? value.ToString()
            : "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.OnRejected = async (context, _) =>
    {
        var problem = Results.Problem(
            title: "Too Many Requests",
            detail: "Rate limit exceeded. Retry later.",
            statusCode: StatusCodes.Status429TooManyRequests,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = context.HttpContext.TraceIdentifier
            });

        await problem.ExecuteAsync(context.HttpContext);
    };
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

var imageStorageOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ImageStorageOptions>>().Value;
var imageStoragePath = Path.IsPathRooted(imageStorageOptions.BasePath)
    ? imageStorageOptions.BasePath
    : Path.Combine(app.Environment.ContentRootPath, imageStorageOptions.BasePath);

Directory.CreateDirectory(imageStoragePath);

using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await databaseInitializer.InitializeAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<SafeRequestBodyLoggingMiddleware>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageStoragePath),
    RequestPath = "/images"
});
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
