using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Subscriptions.Queries;
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

        [HttpGet("{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<Subscription>>> GetSubscriptionById(
            [FromRoute] Guid accountId, [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSubscriptionByIdQuery() { AccountId = accountId, Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<Subscription>>>(
                value => TypedResults.Ok(value.ToV1()),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpGet]
        public async Task<Results<InternalServerError<ProblemDetails>, Ok<PagedResponse<Subscription>>>> GetSubscriptions(
            [FromRoute] Guid accountId, [FromQuery] GetSubscriptionsRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request.ToQuery(accountId), cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, Ok<PagedResponse<Subscription>>>>(
                value => TypedResults.Ok(value.ToPagedResponse(subscription => subscription.ToV1())),
                error => InternalServerErrorProblemResponse(error)
            );
        }
    }
}