using Company.Service.Application.Common.Types.Errors;
using Company.Service.RestApi.Api.Subscriptions.V1.Contracts;
using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Company.Service.RestApi.Api.Subscriptions.V1
{
    [Route("api/v{version:apiVersion}/accounts/{accountId:guid}/subscriptions")]
    [ApiController]
    public class SubscriptionsController : ApiControllerBase
    {
        [HttpPost]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<Subscription>>> CreateSubscription(
            [FromRoute] Guid accountId, [FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request.ToCommand(accountId), cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<Subscription>>>(
                value => TypedResults.Ok(value.ToV1()),
                error => error switch
                {
                    ValidationError ve => ValidationproblemResponse(ve),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }
    }
}