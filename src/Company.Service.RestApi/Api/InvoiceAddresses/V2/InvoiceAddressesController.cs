//__EXAMPLE_START__
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.InvoiceAddresses.Queries;
using Company.Service.RestApi.Api.InvoiceAddresses.V2.Contracts;
using Company.Service.RestApi.Common.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Company.Service.RestApi.Api.InvoiceAddresses.V2
{
    [Route("api/v{version:apiVersion}/invoice-adresses")]
    [ApiController]
    public class InvoiceAddressesController : ApiControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAddress>>> GetInvoiceAddressById(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInvoiceAddressByIdQuery() { Id = id }, cancellationToken);

            return result.Match<Results<InternalServerError<ProblemDetails>, NotFound<ProblemDetails>, Ok<InvoiceAddress>>>(
                value => TypedResults.Ok(value.ToV2()),
                error => error switch
                {
                    NotFoundError nf => NotFoundProblemResponse(nf),
                    _ => InternalServerErrorProblemResponse(error)
                }
            );
        }
    }
}
//__EXAMPLE_END__
