using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Application.IntegrationEvents.V1.Accounts;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class CreateAccountOrderCommandHandlerTests : DbContextTestBase
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;
    private readonly CreateAccountOrderCommandHandler _sut;

    public CreateAccountOrderCommandHandlerTests()
    {
        _fakeTimeProvider = new();
        _publishEndpoint = Substitute.For<MassTransit.IPublishEndpoint>();
        _sut = new(DbContext, _publishEndpoint, _fakeTimeProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesAndReturnsAccountOrder()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
        var tenantId = Guid.NewGuid();

        // Seed an InvoiceAddress since AccountDetails requires a valid InvoiceAddressId
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Default Invoice Address",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "12345",
                street: "Main St",
                number: "10"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CreateAccountOrderCommand
        {
            TenantId = tenantId,
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = invoiceAddress.Id
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Capture published event for later assertions
        AccountOrderCreatedEvent? capturedEvent = null;
        _publishEndpoint.Publish(
                Arg.Do<AccountOrderCreatedEvent>(e => capturedEvent = e),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(command.TenantId);
        result.Value.AccountDetails.Name.Should().Be(command.AccountDetails.Name);
        result.Value.AccountDetails.Email.Should().Be(command.AccountDetails.Email);
        result.Value.AccountDetails.Tier.Should().Be(command.AccountDetails.Tier);
        result.Value.AccountDetails.InvoiceAddressId.Should().Be(command.AccountDetails.InvoiceAddressId);
        result.Value.ContactInformation.FirstName.Should().Be(command.ContactInformation.FirstName);
        result.Value.ContactInformation.LastName.Should().Be(command.ContactInformation.LastName);
        result.Value.ContactInformation.Email.Should().Be(command.ContactInformation.Email);
        result.Value.ContactInformation.PhoneNumber.Should().Be(command.ContactInformation.PhoneNumber);
        result.Value.Status.Should().Be(AccountOrderStatus.Pending);
        result.Value.CreatedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);

        // Verify it was persisted
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.AccountOrders.FirstOrDefault(o => o.Id == result.Value!.Id);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(command.TenantId);
        persisted.AccountDetails.Name.Should().Be(command.AccountDetails.Name);
        persisted.AccountDetails.Email.Should().Be(command.AccountDetails.Email);
        persisted.AccountDetails.Tier.Should().Be(command.AccountDetails.Tier);
        persisted.AccountDetails.InvoiceAddressId.Should().Be(command.AccountDetails.InvoiceAddressId);
        persisted.ContactInformation.FirstName.Should().Be(command.ContactInformation.FirstName);
        persisted.ContactInformation.LastName.Should().Be(command.ContactInformation.LastName);
        persisted.ContactInformation.Email.Should().Be(command.ContactInformation.Email);
        persisted.ContactInformation.PhoneNumber.Should().Be(command.ContactInformation.PhoneNumber);
        persisted.Status.Should().Be(AccountOrderStatus.Pending);
        persisted.CreatedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);
        persisted.AccountId.Should().BeNull();
        persisted.CompletedDate.Should().BeNull();

        // Verify integration event was published with all fields correctly mapped
        capturedEvent.Should().NotBeNull();
        capturedEvent!.AccountOrderId.Should().Be(persisted.Id);
        capturedEvent.TenantId.Should().Be(persisted.TenantId);
        capturedEvent.AccountDetails.Name.Should().Be(persisted.AccountDetails.Name);
        capturedEvent.AccountDetails.Email.Should().Be(persisted.AccountDetails.Email);
        capturedEvent.AccountDetails.Tier.Should().Be((IntegrationEvents.V1.Shared.AccountTier)persisted.AccountDetails.Tier);
        capturedEvent.ContactInformation.FirstName.Should().Be(persisted.ContactInformation.FirstName);
        capturedEvent.ContactInformation.LastName.Should().Be(persisted.ContactInformation.LastName);
        capturedEvent.ContactInformation.Email.Should().Be(persisted.ContactInformation.Email);
        capturedEvent.ContactInformation.PhoneNumber.Should().Be(persisted.ContactInformation.PhoneNumber);
        capturedEvent.CreatedDate.Should().Be(persisted.CreatedDate);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ReturnsValidationError()
    {
        // Arrange — single invalid field is enough to prove mapping occurs
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.Empty,
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
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
