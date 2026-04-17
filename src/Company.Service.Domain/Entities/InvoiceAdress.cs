using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.Entities;

public class InvoiceAdress
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Address Address { get; private set; } = default!;

    private InvoiceAdress() { }

    internal InvoiceAdress(Guid id, Guid tenantId, string name, Address address)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Address = address;
    }

    public static Result<InvoiceAdress, ValidationError> CreateNew(Guid tenantId, string name, Address address)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(tenantId, nameof(tenantId)),
            Validate.NotEmpty(name, nameof(name))
        ).MapToValueResult(new InvoiceAdress()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Address = address
        });
    }

    public Result<ValidationError> ChangeName(string name)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(name, nameof(name))
        ).Bind(() =>
        {
            Name = name;
            return new();
        });
    }

    public void ChangeAddress(Address address)
    {
        Address = address;
    }
}