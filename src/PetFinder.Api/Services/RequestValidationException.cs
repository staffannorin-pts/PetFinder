namespace PetFinder.Api.Services;

public sealed class RequestValidationException(string message, IReadOnlyDictionary<string, string[]> errors) : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
