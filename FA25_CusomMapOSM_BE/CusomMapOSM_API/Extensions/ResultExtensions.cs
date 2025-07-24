using CusomMapOSM_Application.Common.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CusomMapOSM_API.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetailsResult(this Error error)
    {
        return Results.Problem(new ProblemDetails
        {
            Title = error.Code,
            Status = error.GetStatusCode(),
            Detail = error.Description,
            Type = error.Code.ToString()
        });
    }

    internal static int GetStatusCode(this Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => (int)HttpStatusCode.BadRequest,
            ErrorType.Problem => (int)HttpStatusCode.BadRequest,
            ErrorType.NotFound => (int)HttpStatusCode.NotFound,
            ErrorType.Unauthorized => (int)HttpStatusCode.Unauthorized,
            ErrorType.Forbidden => (int)HttpStatusCode.Forbidden,
            ErrorType.Conflict => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}
