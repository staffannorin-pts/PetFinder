namespace PetFinder.Api.Storage;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string BasePath { get; set; } = "storage/images";

    public long MaxBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}
