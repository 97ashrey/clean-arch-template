using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.InvoiceAddresses.Commands;

public record UpdateInvoiceAddressCommand : ApplicationRequest<InvoiceAddress>
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required AddressCommand Address { get; init; }

    public record AddressCommand
    {
        public required string Street { get; init; }
        public required string City { get; init; }
        public required string ZipCode { get; init; }
        public required string Country { get; init; }
        public required string Number { get; init; }
    }
}

internal class UpdateInvoiceAddressCommandValidator : AbstractValidator<UpdateInvoiceAddressCommand>
{
    public UpdateInvoiceAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address.Street).NotEmpty();
        RuleFor(x => x.Address.City).NotEmpty();
        RuleFor(x => x.Address.ZipCode).NotEmpty();
        RuleFor(x => x.Address.Country).NotEmpty();
        RuleFor(x => x.Address.Number).NotEmpty();
    }
}

internal class UpdateInvoiceAddressCommandHandler : IApplicationRequestHandler<UpdateInvoiceAddressCommand, InvoiceAddress>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateInvoiceAddressCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ValueResult<InvoiceAddress, ApplicationError>> Handle(UpdateInvoiceAddressCommand request, CancellationToken cancellationToken)
    {
        var invoiceAddress = await _dbContext.InvoiceAdresses
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (invoiceAddress is null)
        {
            return new NotFoundError()
            {
                Message = $"Invoice address with Id {request.Id} not found."
            };
        }

        return await Address
            .CreateNew(
                request.Address.Country,
                request.Address.City,
                request.Address.ZipCode,
                request.Address.Street,
                request.Address.Number)
            .MapError<ApplicationError>(error => error.ToAppValidationError())
            .Bind(address =>
                invoiceAddress.ChangeName(request.Name)
                    .MapToValueResult(address)
                    .MapError<ApplicationError>(error => error.ToAppValidationError())
            )
            .Tap(address =>
            {
                invoiceAddress.ChangeAddress(address);
            })
            .TapAsync(async _ =>
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            })
            .MapAsync(_ => invoiceAddress);
    }
}