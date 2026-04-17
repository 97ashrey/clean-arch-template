using Company.Service.Application.Accounts.Commands;
using Company.Service.Application.Accounts.Queries;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Entities;
using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Company.Service.RestApi.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ApiControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<Account>>> GetAccountById(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAccountByIdQuery() { Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<Account>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpGet("orders/{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<AccountOrder>>> GetAccountOrderById(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAccountOrderByIdQuery() { Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<AccountOrder>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpGet("orders")]
        public async Task<Results<InternalServerError<ProblemDetails>, Ok<PagedList<AccountOrder>>>> GetAccountOrders(
            [FromQuery] GetAccountOrdersQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, Ok<PagedList<AccountOrder>>>>(
                value => TypedResults.Ok(value),
                error => InternalServerErrorProblemResponse(error)
            );
        }

        [HttpPost("orders")]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<AccountOrder>>> CreateAccountOrder(
            [FromBody] CreateAccountOrderCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<AccountOrder>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    ValidationError ve => ValidationproblemResponse(ve),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpPut("orders/{accountOrderId:guid}/start-processing")]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, BadRequest<ProblemDetails>, Ok<AccountOrder>>> StartProcessingAccountOrder(
            [FromRoute] Guid accountOrderId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ProcessAccountOrderCommand() { AccountOrderId = accountOrderId }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, BadRequest<ProblemDetails>, Ok<AccountOrder>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    ValidationError ve => ValidationproblemResponse(ve),
                    BadRequestError be => BadRequestProblemResponse(be),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpPut("orders/{accountOrderId:guid}/complete")]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, BadRequest<ProblemDetails>, Ok<AccountOrder>>> CompleteAccountOrder(
            [FromRoute] Guid accountOrderId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new CompleteAccountOrderCommand() { AccountOrderId = accountOrderId }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, BadRequest<ProblemDetails>, Ok<AccountOrder>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    ValidationError ve => ValidationproblemResponse(ve),
                    BadRequestError be => BadRequestProblemResponse(be),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }
    }
}