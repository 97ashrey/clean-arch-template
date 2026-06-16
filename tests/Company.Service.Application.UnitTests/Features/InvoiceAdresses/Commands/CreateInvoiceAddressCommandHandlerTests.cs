using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.InvoiceAddresses.Commands;
using FluentAssertions;

namespace Company.Service.Application.UnitTests.Features.InvoiceAdresses.Commands;

public class CreateInvoiceAddressCommandHandlerTests : DbContextTestBase
{
    private readonly CreateInvoiceAddressCommandHandler _sut;

    public CreateInvoiceAddressCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesAndReturnsInvoiceAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = tenantId,
            Name = "Home",
            Address = new()
            {
                Street = "Main Street",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(tenantId);
        result.Value!.Name.Should().Be("Home");
        result.Value!.Address.Street.Should().Be("Main Street");
        result.Value!.Address.City.Should().Be("New York");
        result.Value!.Address.ZipCode.Should().Be("10001");
        result.Value!.Address.Country.Should().Be("USA");
        result.Value!.Address.Number.Should().Be("123");

        // Verify it was persisted to the database
        DbContext.ChangeTracker.Clear();
        var persistedAddress = DbContext.InvoiceAdresses.FirstOrDefault(a => a.Id == result.Value!.Id);
        persistedAddress.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidDataInCommand_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.Empty,
            Name = "Home",
            Address = new()
            {
                Street = "Main Street",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<ValidationError>();
    }
}