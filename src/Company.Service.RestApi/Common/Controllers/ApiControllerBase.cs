using Company.Service.Application.Common.Types.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Company.Service.RestApi.Common.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    private ILogger? _logger;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected ILogger Logger => _logger ??= HttpContext.RequestServices.GetRequiredService<ILogger<ApiControllerBase>>();

    // /// <summary>
    // /// Handles the result using <see cref="HandleApplicationError"/> handler. If you need to handle the error differently use <see cref="Result{TValue,ApplicationError}.Match"/>
    // /// </summary>
    // /// <typeparam name="TValue"></typeparam>
    // /// <param name="controller"></param>
    // /// <param name="result"></param>
    // /// <param name="success"></param>
    // /// <returns></returns>
    // protected IActionResult HandleResult<TValue>(Result<TValue, ApplicationError> result, Func<TValue, IActionResult> success)
    // {
    //     return result.Match(
    //             success,
    //             HandleApplicationError
    //         );
    // }

    // private ObjectResult HandleApplicationError(ApplicationError error)
    // {
    //     Logger.LogError("Request responded with error: {errorResponse}", error.ToString());

    //     return error switch
    //     {
    //         ValidationError validationError => ValidationProblem(validationError),
    //         BadRequestError badRequestError => Problem(detail: badRequestError.Message, statusCode: StatusCodes.Status400BadRequest),
    //         NotFoundError notFound => Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound),
    //         _ => InternalServerErrorProblem(error)
    //     };
    // }

    // private BadRequestObjectResult ValidationProblem(ValidationError error)
    // {
    //     var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
    //         HttpContext,
    //         detail: error.Message,
    //         title: "One or more validation errors occurred.",
    //         statusCode: StatusCodes.Status400BadRequest);

    //     problemDetails.Extensions.Add("validationErrors", error.Failures.ToDictionary(validationFailure => validationFailure.PropertyName, validationFailure => validationFailure.Errors));

    //     return BadRequest(problemDetails);
    // }

    // private ObjectResult InternalServerErrorProblem(ApplicationError error)
    // {
    //     var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
    //         HttpContext,
    //         detail: error.Message,
    //         statusCode: StatusCodes.Status500InternalServerError);

    //     problemDetails.Extensions.Add("errorId", error.Id);

    //     return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
    // }

    protected NotFound<ProblemDetails> NotFoundProblem(NotFoundError error)
    {
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status404NotFound,
            detail: error.Message);

        problemDetails.ApplyApplicationErrorCustomizations(error);

        return TypedResults.NotFound(problemDetails);
    }
        
    protected BadRequest<ProblemDetails> BadRequestProblem(BadRequestError error)
    {
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status400BadRequest,
            detail: error.Message);

        problemDetails.ApplyApplicationErrorCustomizations(error);

        return TypedResults.BadRequest(problemDetails);
    }

    protected BadRequest<ValidationProblemDetails> Validationproblem(ValidationError error)
    {
        var validationProblem = ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState, detail: error.Message, statusCode: StatusCodes.Status400BadRequest);

        validationProblem.Errors = error.Failures.ToDictionary(validationFailure => validationFailure.PropertyName, validationFailure => validationFailure.Errors);

        return TypedResults.BadRequest(validationProblem);
    }

    protected InternalServerError<ProblemDetails> InternalServerErrorProblem(ApplicationError error)
    {
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status500InternalServerError,
            detail: error.Message);
        
        problemDetails.ApplyApplicationErrorCustomizations(error);
        
        return TypedResults.InternalServerError(problemDetails);
    }
}

internal static class ProblemDetailsExtensions
{
    extension(ProblemDetails problemDetails)
    {
        public void ApplyApplicationErrorCustomizations(ApplicationError error)
        {
            problemDetails.Extensions.Add("errorId", error.Id);
        }
    }
}