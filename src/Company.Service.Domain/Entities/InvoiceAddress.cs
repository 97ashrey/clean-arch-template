//__EXAMPLE_START__
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Utils;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.Entities;

public class InvoiceAddress
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Address Address { get; private set; } = default!;

    private InvoiceAddress() { }

    public static ValueResult<InvoiceAddress, ValidationError> CreateNew(Guid tenantId, string name, Address address)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(tenantId, nameof(tenantId)),
            Validate.NotEmpty(name, nameof(name))
        ).MapToValueResult(new InvoiceAddress()
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
//__EXAMPLE_END__
