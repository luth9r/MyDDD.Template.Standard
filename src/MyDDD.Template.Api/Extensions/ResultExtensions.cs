using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't convert success result to problem details");
        }

        return result switch
        {
            IValidationResult validationResult => Results.Problem(
                title: "Validation Error",
                detail: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    { "errors", validationResult.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Message).ToArray())
                    },
                }),

            _ => Results.Problem(
                title: GetTitle(result.Error.Type),
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error.Type),
                extensions: new Dictionary<string, object?>
                {
                    { "code", result.Error.Code },
                }),
        };
    }

    private static string GetTitle(ErrorType type)
    {
        return type switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Validation => "Validation Error",
            _ => "Bad Request",
        };
    }

    private static int GetStatusCode(ErrorType type)
    {
        return type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorised => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest,
        };
    }
}
