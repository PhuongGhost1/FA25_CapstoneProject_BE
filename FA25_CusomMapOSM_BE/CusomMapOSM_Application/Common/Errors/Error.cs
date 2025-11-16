namespace CusomMapOSM_Application.Common.Errors;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Problem(string code, string description) => new(code, description, ErrorType.Problem);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error ValidationError(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a new Failure error with a generic code and the provided message
    /// </summary>
    public static Error New(string message) => new("General.Error", message, ErrorType.Failure);
}
