using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Queries;

public class GetAccountByIdQueryHandlerTests : DbContextTestBase
{
    private readonly GetAccountByIdQueryHandler _sut;

    public GetAccountByIdQueryHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetAccountByIdQuery { Id = account.Id };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(account.Id);
        result.Value.TenantId.Should().Be(account.TenantId);
        result.Value.Name.Should().Be(account.Name);
        result.Value.Email.Should().Be(account.Email);
        result.Value.Tier.Should().Be(account.Tier);
        result.Value.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
        result.Value.Status.Should().Be(account.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetAccountByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Account with Id {query.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithEmptyId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetAccountByIdQuery { Id = Guid.Empty };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Account with Id {query.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithValidIdAndMatchingTenantId_ReturnsAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetAccountByIdQuery
        {
            Id = account.Id,
            TenantId = tenantId
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(account.Id);
        result.Value.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_WithValidIdButDifferentTenantId_ReturnsNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetAccountByIdQuery
        {
            Id = account.Id,
            TenantId = differentTenantId
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Account with Id {query.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithMultipleAccounts_ReturnsOnlyRequestedAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        var account1 = Account.CreateNew(
            tenantId: tenantId,
            name: "Account 1",
            email: "account1@example.com",
            tier: AccountTier.Individual,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        var account2 = Account.CreateNew(
            tenantId: tenantId,
            name: "Account 2",
            email: "account2@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.AddRange(account1, account2);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetAccountByIdQuery { Id = account1.Id };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(account1.Id);
        result.Value.Name.Should().Be(account1.Name);
    }
}