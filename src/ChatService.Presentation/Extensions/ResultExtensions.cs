using ChatService.Contract.Abstractions.Shared;

namespace ChatService.Presentation.Extensions;

public static class ResultExtensions
{
    public static IResult HandlerFailure(this Result result)
    {
        return result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                Results.Problem(
                    statusCode: GetStatusCode(result.Error.ErrorType),
                    title: GetTitle(result.Error.ErrorType),
                    type: GetType(result.Error.ErrorType),
                    extensions: new Dictionary<string, object?>
                    {
                        {"errors", new [] {validationResult.Errors}}
                    }),
            _ =>
                Results.Problem(
                    statusCode: GetStatusCode(result.Error.ErrorType),
                    title: GetTitle(result.Error.ErrorType),
                    type: GetType(result.Error.ErrorType),
                    extensions: new Dictionary<string, object?>
                    {
                        {"errors", new [] {result.Error}}
                    })
        };
        
        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest, 
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        
        static string GetTitle(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "Bad Request",
                ErrorType.NotFound => "Not Found",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.Conflict => "Conflict",
                _ => "Server Error"
            };
        
        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                ErrorType.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            };
    }
    
}