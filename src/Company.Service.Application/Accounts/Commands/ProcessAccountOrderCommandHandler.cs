using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Accounts.Commands;

public record ProcessAccountOrderCommand : ApplicationRequest<AccountOrder>
{
    public Guid AccountOrderId { get; init; }
}

internal class ProcessAccountOrderCommandHandler : IApplicationRequestHandler<ProcessAccountOrderCommand, AccountOrder>
{
    private readonly IApplicationDbContext _context;

    public ProcessAccountOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async ValueTask<Result<AccountOrder, ApplicationError>> Handle(ProcessAccountOrderCommand request, CancellationToken cancellationToken)
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

        return await accountOrder.StartProcessing()
            .MatchAsync<Result<AccountOrder, ApplicationError>>(
                async () =>
                {
                    // publish integration event

                    await _context.SaveChangesAsync(cancellationToken);

                    return accountOrder;
                },
                async error =>
                {
                    return new BadRequestError()
                    {
                        Message = error.Message
                    };
                }
            );
    }
}