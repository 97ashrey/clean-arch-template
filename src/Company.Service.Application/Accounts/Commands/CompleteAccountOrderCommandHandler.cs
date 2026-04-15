using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
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

    public CompleteAccountOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async ValueTask<Result<AccountOrder, ApplicationError>> Handle(CompleteAccountOrderCommand request, CancellationToken cancellationToken)
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
                accountOrder.AccountName,
                accountOrder.ContactInformation.Email,
                accountOrder.Tier,
                accountOrder.InvoiceAddressId
            )
            .MapError<ApplicationError>(error => new ValidationError()
                {
                    Message = "Validation failed!.",
                    Failures = error.Failures.Select(f => new ValidationFailure(f.PropertyName, f.Errors)).ToArray()
                })
            .Tap(account => _context.Accounts.Add(account))
            .Bind(account => 
                accountOrder
                    .Complete(account, DateTime.UtcNow)
                    .MapToValueResult(accountOrder)
                    .MapError<ApplicationError>(error => new BadRequestError() { Message = error.Message })
            )
            .MatchAsync<Result<AccountOrder, ApplicationError>>(
                async order =>
                {
                    // publish integration event
                    
                    await _context.SaveChangesAsync(cancellationToken);

                    return order;
                },
                async error => error
            );
    }
}
