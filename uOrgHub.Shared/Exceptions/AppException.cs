namespace uOrgHub.Shared.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message) => StatusCode = statusCode;
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with ID '{id}' was not found.", 404) { }
}

public class ValidationException : AppException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Validation failed.", 422) => Errors = errors;
}