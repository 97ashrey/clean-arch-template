using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.InvoiceAddresses.Queries;

public record GetInvoiceAddressByIdQuery : ApplicationRequest<InvoiceAdress>
{
    public required Guid Id { get; init; }

    public Guid? TenantId { get; init; }
}

internal class GetInvoiceAddressByIdQueryHandler : IApplicationRequestHandler<GetInvoiceAddressByIdQuery, InvoiceAdress>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceAddressByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<InvoiceAdress, ApplicationError>> Handle(GetInvoiceAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InvoiceAdresses
            .AsNoTracking()
            .Where(a => a.Id == request.Id);

        if (request.TenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == request.TenantId);
        }

        var address = await query.FirstOrDefaultAsync(cancellationToken);

        if (address is null)
        {
            return new NotFoundError()
            {
                Message = $"Invoice address with Id {request.Id} not found."
            };
        }

        return address;
    }
}