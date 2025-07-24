using FluentResults;

namespace CusomMapOSM_Commons;

public class FailureError : Error
{
    public FailureError(string message) : base(message)
    {

    }
    public FailureError(string message, Dictionary<string, object> errorsMetadata) : base(message)
    {
        WithMetadata(errorsMetadata);
    }
}
public class ValidationError : Error
{
    public ValidationError(string message, Dictionary<string, object> validationErrors) : base(message)
    {
        WithMetadata(validationErrors);
    }
    public ValidationError(string message) : base(message)
    {
    }
}

public class UnexpectedError : Error { }
public class ConflictError : Error
{
    public ConflictError(string message) : base(message)
    {

    }

    protected ConflictError()
    {
        base.Message = "Conflict";
    }
}
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
    }

    public NotFoundError()
    {
        base.Message = "Not Found";
    }
}
public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message)
    {
    }

    public UnauthorizedError()
    {
        base.Message = "Unauthorized";
    }
}
public class ForbiddenError : Error
{
    public ForbiddenError(string message) : base(message)
    {
    }

    public ForbiddenError()
    {
        base.Message = "Forbidden";
    }
}
