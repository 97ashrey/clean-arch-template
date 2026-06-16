using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Accounts.Commands;

public record CompleteAccountOrderCommand : ApplicationRequest<AccountOrder>
{
    public Guid AccountOrderId { get; init; }
}

internal class CompleteAccountOrderCommandHandler : IApplicationRequestHandler<CompleteAccountOrderCommand, AccountOrder>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CompleteAccountOrderCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async ValueTask<ValueResult<AccountOrder, ApplicationError>> Handle(CompleteAccountOrderCommand request, CancellationToken cancellationToken)
    {
        var accountOrder = await _context.AccountOrders
            .FirstOrDefaultAsync(ao => ao.Id == request.AccountOrderId, cancellationToken);

        if (accountOrder is null)
        {
            return new NotFoundError()
            {
                Message = $"AccountOrder with Id {request.AccountOrderId} not found!"
            };
        }

        return await Account.CreateNew(
                accountOrder.TenantId,
                accountOrder.AccountDetails.Name,
                accountOrder.AccountDetails.Email,
                accountOrder.AccountDetails.Tier,
                accountOrder.AccountDetails.InvoiceAddressId
            )
            .MapError<ApplicationError>(error =>
                new ValidationError()
                {
                    Message = "Validation failed!.",
                    Failures = error.Failures.Select(f => new ValidationFailure(f.PropertyName, f.Errors)).ToArray()
                }
            )
            .Tap(account => _context.Accounts.Add(account))
            .Bind(account =>
                accountOrder
                    .Complete(account, _timeProvider.GetUtcNowDateTime())
                    .MapToValueResult(accountOrder)
                    .MapError<ApplicationError>(error => new BadRequestError() { Message = error.Message })
            )
            .TapAsync(async order =>
            {
                // publish integration event

                await _context.SaveChangesAsync(cancellationToken);
            });
    }
}