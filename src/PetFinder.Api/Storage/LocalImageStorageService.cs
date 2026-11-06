using Microsoft.Extensions.Options;

namespace PetFinder.Api.Storage;

public sealed class LocalImageStorageService(IOptions<ImageStorageOptions> options, IWebHostEnvironment environment) : IImageStorageService
{
    private readonly ImageStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(IFormFile image, CancellationToken cancellationToken)
    {
        Validate(image);

        var extension = ResolveExtension(image.ContentType);
        var fileName = $"{Guid.NewGuid():N}{extension}";

        var storageRoot = Path.IsPathRooted(_options.BasePath)
            ? _options.BasePath
            : Path.Combine(environment.ContentRootPath, _options.BasePath);

        Directory.CreateDirectory(storageRoot);

        var fullPath = Path.Combine(storageRoot, fileName);
        await using var output = File.Create(fullPath);
        await using var input = image.OpenReadStream();
        await input.CopyToAsync(output, cancellationToken);

        return $"/images/{fileName}";
    }

    private void Validate(IFormFile image)
    {
        if (image.Length <= 0)
        {
            throw new InvalidDataException("Uploaded image is empty.");
        }

        if (image.Length > _options.MaxBytes)
        {
            throw new InvalidDataException($"Image exceeds max size {_options.MaxBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(image.ContentType) ||
            !_options.AllowedContentTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Unsupported image content type.");
        }
    }

    private static string ResolveExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".bin"
        };
    }
}
