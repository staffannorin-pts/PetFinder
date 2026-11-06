namespace PetFinder.Api.Storage;

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile image, CancellationToken cancellationToken);
}
