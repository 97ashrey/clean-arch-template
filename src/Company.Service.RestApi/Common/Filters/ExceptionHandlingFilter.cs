using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Company.Service.RestApi.Common.Filters;

internal class ExceptionHandlerFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionHandlerFilter> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ExceptionHandlerFilter(ILogger<ExceptionHandlerFilter> logger, ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public void OnException(ExceptionContext context)
    {
        var errorId = Guid.NewGuid().ToString();

        using (_logger.BeginScope(new Dictionary<string, object>() { { "ErrorId", errorId } }))
        {
            _logger.LogError(context.Exception, "An unexpected error has occured.");

            var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                context.HttpContext,
                detail: "An unexpected error has occured.",
                statusCode: StatusCodes.Status500InternalServerError);

            problemDetails.Extensions.Add("errorId", errorId);

            context.Result = new JsonResult(problemDetails)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
    }
}