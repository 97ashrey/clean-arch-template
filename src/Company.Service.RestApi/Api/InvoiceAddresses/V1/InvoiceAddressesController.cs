using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.InvoiceAddresses.Queries;
using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.Api.InvoiceAddresses.V1
{
    [Route("api/v{version:apiVersion}/invoice-addresses")]
    [ApiController]
    public class InvoiceAddressesController : ApiControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAddress>>> GetInvoiceAddressById(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInvoiceAddressByIdQuery() { Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAddress>>>(
                value => TypedResults.Ok(value.ToV1()),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }

        [HttpGet]
        public async Task<Results<InternalServerError<ProblemDetails>, Ok<PagedResponse<InvoiceAddress>>>> GetInvoiceAddresses(
            [FromQuery] GetInvoiceAddressesRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request.ToQuery(), cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, Ok<PagedResponse<InvoiceAddress>>>>(
                value => TypedResults.Ok(value.ToPagedResponse(address => address.ToV1())),
                error => InternalServerErrorProblemResponse(error)
            );
        }

        [HttpPost]
        public async Task<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<InvoiceAddress>>> CreateInvoiceAddress(
            [FromBody] CreateInvoiceAddressRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(request.ToCommand(), cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, BadRequest<ValidationProblemDetails>, Ok<InvoiceAddress>>>(
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