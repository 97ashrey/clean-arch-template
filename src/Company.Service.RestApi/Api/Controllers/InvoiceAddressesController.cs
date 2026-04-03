using Company.Service.Application.InvoiceAddresses.Commands;
using Company.Service.Application.InvoiceAddresses.Queries;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Entities;
using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Company.Service.RestApi.Api.Controllers
{
    [Route("api/invoice-addresses")]
    [ApiController]
    public class InvoiceAddressesController : ApiControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAdress>>> GetInvoiceAddressById(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInvoiceAddressByIdQuery() { Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAdress>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpGet]
        public async Task<Results<InternalServerError<ProblemDetails>, Ok<PagedList<InvoiceAdress>>>> GetInvoiceAddresses(
            [FromQuery] GetInvoiceAddressesQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, Ok<PagedList<InvoiceAdress>>>>(
                value => TypedResults.Ok(value),
                error => InternalServerErrorProblemResponse(error)
            );
        }

        [HttpPost]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<InvoiceAdress>>> CreateInvoiceAddress(
            [FromBody] CreateInvoiceAddressCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<InvoiceAdress>>>(
                value => TypedResults.Ok(value),
                error => error switch
                {
                    ValidationError ve => ValidationproblemResponse(ve),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

    }
}
