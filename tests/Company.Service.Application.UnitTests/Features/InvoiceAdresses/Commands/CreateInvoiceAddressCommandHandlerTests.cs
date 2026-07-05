//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.InvoiceAddresses.Commands;
using Company.Service.Application.IntegrationEvents.V1.InvoiceAddresses;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Company.Service.Application.UnitTests.Features.InvoiceAdresses.Commands;

public class CreateInvoiceAddressCommandHandlerTests : DbContextTestBase
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;
    private readonly CreateInvoiceAddressCommandHandler _sut;

    public CreateInvoiceAddressCommandHandlerTests()
    {
        _fakeTimeProvider = new();
        _publishEndpoint = Substitute.For<MassTransit.IPublishEndpoint>();
        _sut = new(DbContext, _publishEndpoint, _fakeTimeProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesAndReturnsInvoiceAddress()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
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

        // Capture published event
        InvoiceAddressCreatedEvent? capturedEvent = null;
        _publishEndpoint.Publish(
                Arg.Do<InvoiceAddressCreatedEvent>(e => capturedEvent = e),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(command.TenantId);
        result.Value!.Name.Should().Be(command.Name);
        result.Value!.Address.Street.Should().Be(command.Address.Street);
        result.Value!.Address.City.Should().Be(command.Address.City);
        result.Value!.Address.ZipCode.Should().Be(command.Address.ZipCode);
        result.Value!.Address.Country.Should().Be(command.Address.Country);
        result.Value!.Address.Number.Should().Be(command.Address.Number);

        // Verify it was persisted to the database — all fields
        DbContext.ChangeTracker.Clear();
        var persistedAddress = DbContext.InvoiceAdresses.FirstOrDefault(a => a.Id == result.Value!.Id);
        persistedAddress.Should().NotBeNull();
        persistedAddress!.TenantId.Should().Be(command.TenantId);
        persistedAddress.Name.Should().Be(command.Name);
        persistedAddress.Address.Street.Should().Be(command.Address.Street);
        persistedAddress.Address.City.Should().Be(command.Address.City);
        persistedAddress.Address.ZipCode.Should().Be(command.Address.ZipCode);
        persistedAddress.Address.Country.Should().Be(command.Address.Country);
        persistedAddress.Address.Number.Should().Be(command.Address.Number);

        // Verify integration event was published with all fields correctly mapped
        capturedEvent.Should().NotBeNull();
        capturedEvent!.InvoiceAddressId.Should().Be(persistedAddress.Id);
        capturedEvent.TenantId.Should().Be(persistedAddress.TenantId);
        capturedEvent.Name.Should().Be(persistedAddress.Name);
        capturedEvent.Address.Country.Should().Be(persistedAddress.Address.Country);
        capturedEvent.Address.City.Should().Be(persistedAddress.Address.City);
        capturedEvent.Address.ZipCode.Should().Be(persistedAddress.Address.ZipCode);
        capturedEvent.Address.Street.Should().Be(persistedAddress.Address.Street);
        capturedEvent.Address.Number.Should().Be(persistedAddress.Address.Number);
        capturedEvent.CreatedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);
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
//__EXAMPLE_END__