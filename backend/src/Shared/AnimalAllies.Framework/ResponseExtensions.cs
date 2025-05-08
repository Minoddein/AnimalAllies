using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Framework;

public static class ResponseExtensions
{
    public static ActionResult ToResponse(this Error error)
    {
        int statusCode = GetStatusCodeForErrorType(error.Type);

        Envelope envelope = Envelope.Error(error);

        return new ObjectResult(envelope) { StatusCode = statusCode };
    }

    public static ActionResult ToResponse(this ErrorList errors)
    {
        if (!errors.Any())
        {
            return new ObjectResult(Envelope.Error(errors)) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        List<ErrorType> distinctErrorTypes =
        [
            .. errors
                .Select(x => x.Type)
                .Distinct()
        ];

        int statusCode = distinctErrorTypes.Count > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeForErrorType(distinctErrorTypes.First());

        Envelope envelope = Envelope.Error(errors);

        return new ObjectResult(envelope) { StatusCode = StatusCodes.Status400BadRequest };
    }

    private static int GetStatusCodeForErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
}