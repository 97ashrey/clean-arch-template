using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.InvoiceAddresses.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentAssertions;

namespace Company.Service.Application.UnitTests.InvoiceAdresses.Queries;

public class GetInvoiceAddressByIdQueryHandlerTests : DbContextTestBase
{
    private readonly GetInvoiceAddressByIdQueryHandler _sut;

    public GetInvoiceAddressByIdQueryHandlerTests()
    {
        _sut = new GetInvoiceAddressByIdQueryHandler(DbContext);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsInvoiceAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "HomeStreet",
                number: "20/34"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetInvoiceAddressByIdQuery { Id = invoiceAddress.Id };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(invoiceAddress.Id);
        result.Value!.Name.Should().Be("Home");
        result.Value!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetInvoiceAddressByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_WithValidIdAndMatchingTenantId_ReturnsInvoiceAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Work",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "WorkStreet",
                number: "10/17"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetInvoiceAddressByIdQuery
        {
            Id = invoiceAddress.Id,
            TenantId = tenantId
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(invoiceAddress.Id);
        result.Value!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_WithValidIdButDifferentTenantId_ReturnsNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "HomeStreet",
                number: "20/34"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetInvoiceAddressByIdQuery
        {
            Id = invoiceAddress.Id,
            TenantId = differentTenantId
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_WithEmptyId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetInvoiceAddressByIdQuery { Id = Guid.Empty };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_WithMultipleAddresses_ReturnsOnlyRequestedAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var address1 = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "HomeStreet",
                number: "20/34"
            ).Value!
        ).Value!;

        var address2 = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Work",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "WorkStreet",
                number: "10/17"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.AddRange(address1, address2);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetInvoiceAddressByIdQuery { Id = address1.Id };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(address1.Id);
        result.Value!.Name.Should().Be("Home");
        result.Value!.Should().NotBeEquivalentTo(address2);
    }
}