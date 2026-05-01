using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Company.Service.RestApi.Common.Middleware;

internal class HandleUnauthorizedMiddleware
{
    private const string ERROR_HEADER = "WWW-Authenticate";
    private const string BEARER = "Bearer";

    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public HandleUnauthorizedMiddleware(RequestDelegate next, ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            // JwtBearer token validation pipeline writes all errors to this header
            context.Response.Headers.TryGetValue(ERROR_HEADER, out StringValues authError);

            // "Bearer" means that no token was provided, in that case we should display nothing
            authError = authError == BEARER ? default : authError;

            var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                context,
                detail: authError,
                statusCode: context.Response.StatusCode);

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            // For 403, there are no errors to be displayed, so just return a proper body
            var problemDetails = _problemDetailsFactory.CreateProblemDetails(context, statusCode: context.Response.StatusCode);

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

internal static class HandleUnauthorizedMiddlewareExtensions
{
    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseHandleUnauthorized()
        {
            return builder.UseMiddleware<HandleUnauthorizedMiddleware>();
        }
    }
}