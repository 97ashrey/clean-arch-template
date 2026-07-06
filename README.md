# Clean Architecture Template

A .NET template for jump-starting production-ready backend services following **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** principles.

---

## Table of Contents

- [Installation](#installation)
- [Scaffolding a New Project](#scaffolding-a-new-project)
- [Running the Scaffolded Service](#running-the-scaffolded-service)
- [Architecture Overview](#architecture-overview)
- [What the Template Provides Out of the Box](#what-the-template-provides-out-of-the-box)
- [Walkthrough: The `InvoiceAddress` Feature](#walkthrough-the-invoiceaddress-feature)
  - [1. Domain Layer](#1-domain-layer-srcdomain)
  - [2. Application Layer](#2-application-layer-srcapplication)
  - [3. API Layer](#3-api-layer-srcrestapi)
  - [4. Persistence Layer](#4-persistence-layer-srcinfrastructuredata)
  - [5. Integration Events](#5-integration-events-srcapplicationintegrationevents)
- [Testing](#testing)
  - [Test Projects](#test-projects)
  - [What Each Layer Tests](#what-each-layer-tests)
  - [Running Tests](#running-tests)
- [Key Architectural Decisions](#key-architectural-decisions)
- [Adding a New Feature — Step by Step](#adding-a-new-feature--step-by-step)
- [Key Files to Reference](#key-files-to-reference)
- [Complete Vertical Slice: `InvoiceAddress`](#complete-vertical-slice-invoiceaddress)

---

## Installation

Install the template from the project root:

```bash
dotnet new install .
```

This registers the template with the short name `clean-arch`.

## Scaffolding a New Project

Create a new service from the template:

```bash
dotnet new clean-arch -n Acme.Billing --companyName Acme --serviceDomain Billing
```

| Parameter | Description |
|---|---|
| `-n`, `--name` | Solution and project name. Convention: `YourCompany.YourService` (e.g., `Acme.Billing`). |
| `--companyName` | **Required.** Your company name (the first part of `-n`). Replaces `CompanyNamePlaceholder` throughout the codebase. |
| `--serviceDomain` | **Required.** The domain/service name (the second part of `-n`). Replaces `ServiceDomainPlaceholder` throughout the codebase. |

> **📝 Note about example code** — The scaffolded project includes an `InvoiceAddress` feature (entity, value objects, commands, queries, controller, contracts, integration events, entity configuration, and tests). These files are **commented out** in the output and serve as a reference for agents or developers when implementing new features. Treat them as documentation-in-code — replace them with your actual domain logic as you build out the service.

After scaffolding, apply your first database migration:

```bash
cd Acme.Billing/src/Acme.Billing.DbDeploy
dotnet ef migrations add Initial
```

Then build and verify:

```bash
dotnet build
```

---

## Running the Scaffolded Service

The solution uses an **Aspire** host project to bootstrap all dependencies locally.

```bash
cd YourCompany.YourService/aspire/YourCompany.YourService.AppHost
dotnet run
```

This spins up (via Docker):

- **SQL Server** — database for the service
- **RabbitMQ** — message broker for integration events
- **Mock OAuth2 Server** — identity provider for local development
- **DbDeploy** — applies EF Core migrations and seeds data
- **RestApi** — the service's HTTP API

---

## Architecture Overview

```
Company.Service.sln
├── src/
│   ├── Domain                  # Core business logic & rules
│   ├── Application             # Use cases (CQRS commands/queries)
│   ├── Application.IntegrationEvents  # Cross-service event contracts
│   ├── Infrastructure          # External services (messaging, third party API integrations etc...)
│   ├── Infrastructure.Data     # EF Core DbContext & persistence
│   ├── RestApi                 # ASP.NET Web API entry point
│   └── DbDeploy                # Standalone migration runner
├── aspire/
│   └── Company.Service.AppHost # Aspire orchestrator for local dev
└── tests/
    ├── Domain.UnitTests        # Entity & value object behavior
    ├── Application.UnitTests   # Handler & validator logic
    ├── RestApi.IntegrationTests # Full HTTP + DB + messaging flow
    └── RestApi.UnitTests       # Controller/filter/middleware tests
```

### Layered Dependency Flow

```
RestApi → Application → Domain
                ↓
         Infrastructure.Data
         Infrastructure
```

Each layer depends **only** on the layer below it. The Domain layer has zero dependencies on infrastructure or the application.

---

## What the Template Provides Out of the Box

The template wires up production-ready infrastructure so you can focus on business logic. Each piece is configured in a single place and can be replaced or removed as needed.

| Capability | Configuration File | What It Does |
|---|---|---|
| **CQRS / Mediator** | [`Application/ConfigureServices.cs`](src/Company.Service.Application/ConfigureServices.cs) | Registers Mediator with pipeline behaviours for logging, validation, and exception handling. Each command/query is a separate handler — no monolithic service classes. |
| **FluentValidation** | [`Application/ConfigureServices.cs`](src/Company.Service.Application/ConfigureServices.cs) | Auto-registers all validators from the Application assembly. Validators run as a Mediator pipeline behaviour before every command/query. |
| **EF Core + SQL Server** | [`Infrastructure/ConfigureServices.cs`](src/Company.Service.Infrastructure/ConfigureServices.cs), [`ServiceDomainPlaceholderDbContext.cs`](src/Company.Service.Infrastructure.Data/Persistence/ServiceDomainPlaceholderDbContext.cs) | Configures the DbContext with SQL Server connection string. Entity configurations are auto-discovered via `ApplyConfigurationsFromAssembly`. The database is deployed via the standalone DbDeploy project. |
| **MassTransit (RabbitMQ / In-Memory)** | [`Infrastructure/Messaging/ConfigureMessagingServices.cs`](src/Company.Service.Infrastructure/Messaging/ConfigureMessagingServices.cs) | Configures MassTransit with outbox support. Uses RabbitMQ when a `MessagingBus` connection string is present, otherwise falls back to in-memory transport — ideal for local development without Docker. |
| **Integration Event Outbox** | [`ServiceDomainPlaceholderDbContext.cs`](src/Company.Service.Infrastructure.Data/Persistence/ServiceDomainPlaceholderDbContext.cs) (MassTransit outbox tables in `OnModelCreating`) | Events are published in the same transaction as the database write. If publishing fails, the outbox retries — no lost events. |
| **API Versioning** | [`RestApi/Program.cs`](src/Company.Service.RestApi/Program.cs) | URL-segment-based versioning (`/api/v1/...`, `/api/v2/...`). Controllers are versioned by namespace — adding a new version is as simple as creating a `V2` folder. |
| **Swagger / OpenAPI** | [`RestApi/Program.cs`](src/Company.Service.RestApi/Program.cs) (see `ConfigureSwaggerOptions`) | Auto-generates a Swagger document per API version. Only enabled in `DEBUG` builds. Includes JWT Bearer token definition for testing authenticated endpoints. |
| **OpenTelemetry** | [`RestApi/Program.cs`](src/Company.Service.RestApi/Program.cs) | Captures traces, metrics, and logs via AspNetCore, HttpClient, and Runtime instrumentations. Optionally exports via OTLP when `ExportTelemetry` is set. Logging is configured to forward to OpenTelemetry. |
| **Authentication / Authorization** | [`RestApi/Program.cs`](src/Company.Service.RestApi/Program.cs), [`AuthorityOptions.cs`](src/Company.Service.RestApi/Common/Configurations/AuthorityOptions.cs) | Configures JWT Bearer authentication with configurable authority. Can be disabled entirely (`Authority:Enabled: false`) — controllers fall back to `AllowAnonymous()`, which is the default for integration tests. |
| **Aspire Orchestration** | [`aspire/Company.Service.AppHost/Program.cs`](aspire/Company.Service.AppHost/Program.cs) | Bootstraps SQL Server, RabbitMQ, a mock OAuth2 server, the DbDeploy migration runner, and the RestApi in a single `dotnet run`. All inter-service dependencies and wait-order are declared. |
| **Two-Level Error Handling** | [`Application/Common/Behaviours/ExceptionHandlerPipelineBehaviour.cs`](src/Company.Service.Application/Common/Behaviours/ExceptionHandlerPipelineBehaviour.cs) + [`RestApi/Common/Filters/ExceptionHandlingFilter.cs`](src/Company.Service.RestApi/Common/Filters/ExceptionHandlingFilter.cs) | Application layer catches exceptions during handler execution and returns typed errors. API layer catches anything that escapes and returns a consistent 500 response with an error ID for tracing. |
| **Result Pattern** | [`Domain/Common/Types/Result.cs`](src/Company.Service.Domain/Common/Types/Result.cs) | `Result<TError>` and `ValueResult<TValue, TError>` types with `.Bind()`, `.Map()`, `.Tap()`, `.Match()` combinators replace exceptions as the primary error channel. |
| **Test Infrastructure — Unit** | [`tests/.../DbContextTestBase.cs`](tests/Company.Service.Application.UnitTests/DbContextTestBase.cs) | SQLite in-memory database for handler unit tests — fast, isolated, no Docker required. |
| **Test Infrastructure — Integration** | [`tests/.../IntegrationTestBase.cs`](tests/Company.Service.RestApi.IntegrationTests/IntegrationTestBase.cs), [`IntegrationTestWebAppFactory.cs`](tests/Company.Service.RestApi.IntegrationTests/IntegrationTestWebAppFactory.cs) | TestContainers (SQL Server). |

---

## Walkthrough: The `InvoiceAddress` Feature

This walkthrough follows a complete vertical slice — from domain model through API endpoint — to illustrate every architectural pattern in action.

### 1. Domain Layer (`src/.../Domain/`)

The domain is the innermost layer. It has **no external dependencies** and contains all business rules and validation.

#### Value Object: `Address`

Value objects are immutable, self-validating types. They use factory methods that return `ValueResult<TValue, TError>`.

```csharp
// Domain/ValueObjects/Address.cs
public record Address
{
    public string Country { get; private set; }
    public string City { get; private set; }
    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }

    private Address() { }  // EF Core requires a private constructor

    public static ValueResult<Address, ValidationError> CreateNew(
        string country, string city, string zipCode,
        string street, string number)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(country, nameof(country)),
            Validate.NotEmpty(city, nameof(city)),
            // ... more rules ...
        ).MapToValueResult(new Address()
        {
            Country = country, City = city, ZipCode = zipCode,
            Street = street, Number = number
        });
    }
}
```

**Key patterns:**
- `record` gives immutability by default
- Private constructor + factory method prevents invalid state
- `Validate.ExecuteRules()` collects failures and returns `Result<ValidationError>`
- `MapToValueResult()` converts a success `Result` into a `ValueResult` with the entity value

#### Entity: `InvoiceAddress`

Entities contain behavior and encapsulate validation. Factory methods return `ValueResult`, command methods return `Result` or `ValueResult`.

```csharp
// Domain/Entities/InvoiceAddress.cs
public class InvoiceAddress
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }

    private InvoiceAddress() { }

    public static ValueResult<InvoiceAddress, ValidationError> CreateNew(
        Guid tenantId, string name, Address address)
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
            return new Result<ValidationError>();  // success
        });
    }
}
```

**Key patterns:**
- Private EF Core constructor
- Factory methods validate **all** inputs before constructing
- Command methods (e.g., `ChangeName`) validate and then mutate state
- No throwing exceptions — return `Result` types instead

#### The Result Pattern

Two result types replace exceptions as the primary communication mechanism:

| Type | Use Case | Example |
|---|---|---|
| `Result<TError>` | Operations with no return value | `ChangeName()` returns `Result<ValidationError>` |
| `ValueResult<TValue, TError>` | Operations that return a value | `CreateNew()` returns `ValueResult<InvoiceAddress, ValidationError>` |

Both support functional combinators:

```csharp
// Chaining operations — short-circuits on first failure
Address.CreateNew(...)
    .Bind(address => InvoiceAddress.CreateNew(tenantId, name, address))
    .MapError<ApplicationError>(error => error.ToAppValidationError())
    .TapAsync(async invoiceAddress =>
    {
        _dbContext.InvoiceAdresses.Add(invoiceAddress);
        await _publishEndpoint.Publish(CreateEvent(invoiceAddress));
        await _dbContext.SaveChangesAsync(cancellationToken);
    });
```

| Method | What It Does |
|---|---|
| `.Bind()` | Chain another result-returning operation (short-circuits on failure) |
| `.Map()` | Transform the success value |
| `.MapError()` | Transform the error type |
| `.Tap()` / `.TapAsync()` | Side effect on success (keep the value flowing) |
| `.TapError()` | Side effect on failure |
| `.Match()` | Pattern-match into success/failure branches |

#### Domain Errors

```csharp
public record DomainError(string Message);           // Base

public record ValidationError(string Message,         // Validation failures
    ValidationFailure[] Failures) : DomainError(Message);

public record InvalidOperationError(string Message)   // Business rule violations
    : DomainError(Message);
```

### 2. Application Layer (`src/.../Application/`)

Organized by **feature**, each feature has Commands and Queries subdirectories.

#### Command

Commands inherit from `ApplicationRequest<TResponse>` and include nested input DTOs plus a FluentValidation validator.

```csharp
// Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs
public record CreateInvoiceAddressCommand : ApplicationRequest<InvoiceAddress>
{
    public required Guid TenantId { get; init; }
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

internal class CreateInvoiceAddressCommandValidator : AbstractValidator<CreateInvoiceAddressCommand>
{
    public CreateInvoiceAddressCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address.Street).NotEmpty();
        // ... more rules ...
    }
}
```

#### Command Handler

Handlers coordinate domain operations, persistence, and event publishing. They use `.Bind()` to chain domain factories, `.MapError()` to convert domain errors to application errors, and `.TapAsync()` for side effects.

```csharp
internal class CreateInvoiceAddressCommandHandler
    : IApplicationRequestHandler<CreateInvoiceAddressCommand, InvoiceAddress>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;

    public async ValueTask<ValueResult<InvoiceAddress, ApplicationError>> Handle(
        CreateInvoiceAddressCommand request, CancellationToken cancellationToken)
    {
        return await Address
            .CreateNew(
                request.Address.Country, request.Address.City,
                request.Address.ZipCode, request.Address.Street,
                request.Address.Number)
            .Bind(address => InvoiceAddress.CreateNew(
                tenantId: request.TenantId,
                name: request.Name,
                address: address))
            .MapError<ApplicationError>(error => error.ToAppValidationError())
            .TapAsync(async invoiceAddress =>
            {
                _dbContext.InvoiceAdresses.Add(invoiceAddress);
                await _publishEndpoint.Publish(CreateEvent(invoiceAddress));
                await _dbContext.SaveChangesAsync(cancellationToken);
            });
    }
}
```

#### Query

Queries are read-only handlers that return data. They use `.AsNoTracking()` for EF Core queries.

```csharp
// Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs
public record GetInvoiceAddressByIdQuery : ApplicationRequest<InvoiceAddress>
{
    public required Guid Id { get; init; }
    public Guid? TenantId { get; init; }
}

internal class GetInvoiceAddressByIdQueryHandler
    : IApplicationRequestHandler<GetInvoiceAddressByIdQuery, InvoiceAddress>
{
    private readonly IApplicationDbContext _context;

    public async ValueTask<ValueResult<InvoiceAddress, ApplicationError>> Handle(
        GetInvoiceAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var address = await _context.InvoiceAdresses
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (address is null)
            return new NotFoundError()
                { Message = $"Invoice address with Id {request.Id} not found." };

        return address;
    }
}
```

#### Application Errors

```csharp
public record ApplicationError
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Message { get; init; }
}

public record ValidationError(...) : ApplicationError;    // HTTP 400
public record NotFoundError(...) : ApplicationError;      // HTTP 404
public record BadRequestError(...) : ApplicationError;    // HTTP 400
```

### 3. API Layer (`src/.../RestApi/`)

#### Controller

Controllers are thin — they parse HTTP input, send a Mediator request, and match the result to an HTTP response.

```csharp
// RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs
[Route("api/v{version:apiVersion}/invoice-addresses")]
[ApiController]
public class InvoiceAddressesController : ApiControllerBase
{
    [HttpPost]
    public async Task<Results<InternalServerError<ProblemDetails>,
        BadRequest<ValidationProblemDetails>, Ok<InvoiceAddress>>> CreateInvoiceAddress(
        [FromBody] CreateInvoiceAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(request.ToCommand(), cancellationToken);

        return result.Match<
            Results<InternalServerError<ProblemDetails>,
                BadRequest<ValidationProblemDetails>, Ok<InvoiceAddress>>>(
            value => TypedResults.Ok(value.ToV1()),
            error => error switch
            {
                ValidationError ve => ValidationproblemResponse(ve),
                _ => InternalServerErrorProblemResponse(error)
            }
        );
    }
}
```

#### API Contracts

Input/output contracts are separate from domain models and are versioned independently.

```csharp
// RestApi/Api/InvoiceAddresses/V1/Contracts/CreateInvoiceAddressRequest.cs
public record class CreateInvoiceAddressRequest
{
    public required Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required AddressRequest Address { get; init; }
}

// Mapping extension method
extension(CreateInvoiceAddressRequest request)
{
    public CreateInvoiceAddressCommand ToCommand() => new()
    {
        TenantId = request.TenantId,
        Name = request.Name,
        Address = new()
        {
            Street = request.Address.Street,
            // ...
        }
    };
}
```

```csharp
// RestApi/Api/InvoiceAddresses/V1/Contracts/InvoiceAddress.cs
public record class InvoiceAddress
{
    public required Guid Id { get; init; }
    public required Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required Address Address { get; init; }
}

// Domain-to-contract mapping extension method
extension(Domain.Entities.InvoiceAddress invoiceAddress)
{
    public InvoiceAddress ToV1() => new()
    {
        Id = invoiceAddress.Id,
        TenantId = invoiceAddress.TenantId,
        Name = invoiceAddress.Name,
        Address = new()
        {
            Street = invoiceAddress.Address.Street,
            // ...
        }
    };
}
```

#### API Versioning

Different API versions live in separate folders under the feature directory (`V1/`, `V2/`, etc.). Versioning happens at the **contract level only** — the domain and application layers remain version-agnostic.

```csharp
// V2 has a different response contract shape
// RestApi/Api/InvoiceAddresses/V2/InvoiceAddressesController.cs
extension(Domain.Entities.InvoiceAddress invoiceAddress)
{
    public V2Contracts.InvoiceAddress ToV2() => new()
    {
        Id = invoiceAddress.Id,
        Name = invoiceAddress.Name,
        FullAddress = $"{invoiceAddress.Address.Street} {invoiceAddress.Address.Number}, "
                     + $"{invoiceAddress.Address.ZipCode} {invoiceAddress.Address.City}, "
                     + $"{invoiceAddress.Address.Country}"
    };
}
```

### 4. Persistence Layer (`src/.../Infrastructure.Data/`)

#### Entity Configuration

EF Core entity configurations use the `IEntityTypeConfiguration<TEntity>` pattern and define table mapping, constraints, owned types, and indexes.

```csharp
// Infrastructure.Data/Persistence/EntityConfigurations/InvoiceAddressEntityConfiguration.cs
internal class InvoiceAddressEntityConfiguration : IEntityTypeConfiguration<InvoiceAddress>
{
    public void Configure(EntityTypeBuilder<InvoiceAddress> builder)
    {
        builder.HasKey(ia => ia.Id);
        builder.Property(ia => ia.TenantId).IsRequired();
        builder.Property(ia => ia.Name).IsRequired().HasMaxLength(255);

        builder.OwnsOne(ia => ia.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Country).IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.City).IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);
            addressBuilder.Property(a => a.Street).IsRequired().HasMaxLength(255);
            addressBuilder.Property(a => a.Number).IsRequired().HasMaxLength(50);
        });

        builder.ToTable("InvoiceAddresses");
    }
}
```

Configurations are auto-discovered via `ApplyConfigurationsFromAssembly` in the DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDomainPlaceholderDbContext).Assembly);
}
```

### 5. Integration Events (`src/.../Application.IntegrationEvents/`)

Integration events are shared contracts for cross-service communication via a message broker (RabbitMQ). They're defined as immutable records in a separate NuGet-ready project.

```csharp
// Application.IntegrationEvents/V1/InvoiceAddresses/InvoiceAddressCreatedEvent.cs
public record InvoiceAddressCreatedEvent(
    Guid InvoiceAddressId,
    Guid TenantId,
    string Name,
    Address Address,
    DateTime CreatedDate
);
```

Handlers publish events after successful command execution (before `SaveChangesAsync` — the outbox pattern ensures delivery):

```csharp
.TapAsync(async invoiceAddress =>
{
    _dbContext.InvoiceAdresses.Add(invoiceAddress);
    await _publishEndpoint.Publish(new InvoiceAddressCreatedEvent(
        InvoiceAddressId: invoiceAddress.Id,
        TenantId: invoiceAddress.TenantId,
        Name: invoiceAddress.Name,
        Address: new(/* ... */),
        CreatedDate: _timeProvider.GetUtcNowDateTime()
    ));
    await _dbContext.SaveChangesAsync(cancellationToken);
});
```

---

## Testing

### Test Projects

| Project | What It Tests | Technology |
|---|---|---|
| `Domain.UnitTests` | Entity behavior, value object validation | xUnit + AwesomeAssertions |
| `Application.UnitTests` | Command/query handlers, validators | SQLite in-memory + NSubstitute |
| `RestApi.IntegrationTests` | Full HTTP flow with real DB | TestContainers (SQL Server) |
| `RestApi.UnitTests` | Controllers, filters, middleware | |

### What Each Layer Tests

| Layer | Focus | Example |
|---|---|---|
| **Domain** | Tests entity/value object **behaviour** — validation rules, state transitions, invariants. Pure logic, no dependencies. | `InvoiceAddress.CreateNew()` rejects empty tenant ID; `account.Suspend()` flips status to `Suspended`. |
| **Application** | Tests that handlers **orchestrate the domain** correctly — calling the right factory methods, chaining via `.Bind()`, mapping errors, persisting results via the DbContext, and publishing integration events on success. Uses SQLite in-memory for fast, isolated DB tests. | `CreateInvoiceAddressCommandHandler` calls `Address.CreateNew()` then `InvoiceAddress.CreateNew()`, saves to DB, and publishes `InvoiceAddressCreatedEvent`. |
| **Integration** | Tests the **full HTTP flow** — request deserialization, validation, handler execution, DB persistence, and response serialization. Provides **contract safety** by asserting every field of the response matches what the API promises. Uses TestContainers (real SQL Server) and `FakeTimeProvider` for deterministic time. | `POST /api/v1/invoice-addresses` returns `200 OK` with the full `InvoiceAddress` response body, saves to the database, and publishes an integration event. |

#### Domain — Behaviour Example

```csharp
[Fact]
public void ChangeName_WithEmptyName_ReturnsErrorAndDoesNotChange()
{
    var address = InvoiceAddress.CreateNew(tenantId, "Home", validAddress).Value!;

    var result = address.ChangeName(string.Empty);

    result.IsSuccess.Should().BeFalse();
    address.Name.Should().Be("Home");  // state unchanged on failure
}
```

#### Application — Orchestration Example

```csharp
[Fact]
public async Task Handle_WithValidCommand_CreatesAndSaves()
{
    var result = await _sut.Handle(command, default);
    result.IsSuccess.Should().BeTrue();

    // Verify persistence — all fields
    DbContext.ChangeTracker.Clear();
    var persisted = DbContext.InvoiceAdresses.FirstOrDefault(a => a.Id == result.Value!.Id);
    persisted!.TenantId.Should().Be(command.TenantId);
    persisted.Name.Should().Be(command.Name);
    persisted.Address.Street.Should().Be(command.Address.Street);
    // ... all fields, not just the one that changed ...
}
```

#### Integration — Contract Safety Example

```csharp
[Fact]
public async Task CreateInvoiceAddress_ReturnsOkAndSavesToDatabase()
{
    // Arrange
    var request = new CreateInvoiceAddressRequest { /* ... */ };

    // Act
    var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

    // Assert response — every field matches what the API contract promises
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await response.Content.ReadFromJsonAsync<InvoiceAddress>();
    created!.TenantId.Should().Be(request.TenantId);
    created.Name.Should().Be(request.Name);
    created.Address.Street.Should().Be(request.Address.Street);

    // Assert persistence
    var persisted = await DbContext.InvoiceAdresses.FindAsync([created.Id]);
    persisted.Should().NotBeNull();
    persisted!.Name.Should().Be(request.Name);

    // Assert integration event was published
    var eventPublished = await MassTransitTestHarness.Published<InvoiceAddressCreatedEvent>();
    eventPublished.Should().BeTrue();
}
```

### Running Tests

```bash
# Run all unit tests
dotnet test --filter "Category=Unit"

# Run integration tests (requires Docker)
dotnet test --filter "Category=Integration"

# Generate coverage report for unit tests
bash scripts/test-report.sh unit
```

---

## Key Architectural Decisions

| Decision | Rationale |
|---|---|
| **No Domain Events** | Results are the primary return mechanism — domain events would require fallback logic for error cases; integration events at the application layer handle cross-service communication |
| **Result Pattern over Exceptions** | Makes failure paths explicit at the type level; combinators (`.Bind()`, `.Map()`, etc.) enable clean chaining |
| **CQRS without Event Sourcing** | Commands and queries are separated at the handler level; no event sourcing or separate read/write databases |
| **API Versioning at Contract Layer** | Domain and application are version-agnostic; breaking changes are handled by adding new API contract versions |
| **Rich Domain Models** | Behavior lives in the entity, not in services — `InvoiceAddress.ChangeName()` instead of `InvoiceAddressService.ChangeName()` |
| **No Service Layer** | Mediator handlers replace the traditional service layer — each handler is a single, focused use case |

---

## Adding a New Feature — Step by Step

1. **Domain Entity** — Create the entity with factory methods, command methods, and validation. Add value objects for composed concepts. Write domain unit tests.

2. **Entity Configuration & Migration** — Add `IEntityTypeConfiguration<TEntity>` and generate an EF Core migration.

3. **Application Commands/Queries** — Create command/query records, FluentValidation validators, and handlers. Wire up domain calls via `.Bind()` and `.TapAsync()`. Write handler unit tests with SQLite.

4. **Integration Event** (if needed) — Define the event record in `Application.IntegrationEvents` and publish it on success.

5. **API Controller & Contracts** — Create the controller with typed results (`Results<...>`), request/response contracts, and mapping extensions. Write integration tests.

---

## Key Files to Reference

| File | Purpose |
|---|---|
| [`Domain/Common/Types/Result.cs`](src/Company.Service.Domain/Common/Types/Result.cs) | Result and ValueResult types with combinators |
| [`Domain/Common/Utils/Validate.cs`](src/Company.Service.Domain/Common/Utils/Validate.cs) | Validation helper utilities |
| [`Application/Common/Requests/ApplicationRequest.cs`](src/Company.Service.Application/Common/Requests/ApplicationRequest.cs) | Base class for all commands/queries |
| [`Application/ConfigureServices.cs`](src/Company.Service.Application/ConfigureServices.cs) | Mediator and validator registration |
| [`RestApi/Common/Controllers/ApiControllerBase.cs`](src/Company.Service.RestApi/Common/Controllers/ApiControllerBase.cs) | Base controller with response helpers |
| [`RestApi/Program.cs`](src/Company.Service.RestApi/Program.cs) | Application bootstrap and middleware |
| [`Infrastructure.Data/Persistence/EntityConfigurations/InvoiceAddressEntityConfiguration.cs`](src/Company.Service.Infrastructure.Data/Persistence/EntityConfigurations/InvoiceAddressEntityConfiguration.cs) | EF Core entity configuration example |
| [`tests/.../DbContextTestBase.cs`](tests/Company.Service.Application.UnitTests/DbContextTestBase.cs) | Base class for handler unit tests with SQLite |
| [`tests/.../IntegrationTestBase.cs`](tests/Company.Service.RestApi.IntegrationTests/IntegrationTestBase.cs) | Base class for integration tests |

---

## Complete Vertical Slice: `InvoiceAddress`

| Layer | File |
|---|---|
| Domain Entity | [`Domain/Entities/InvoiceAddress.cs`](src/Company.Service.Domain/Entities/InvoiceAddress.cs) |
| Domain Value Object | [`Domain/ValueObjects/Address.cs`](src/Company.Service.Domain/ValueObjects/Address.cs) |
| Application Command | [`Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs`](src/Company.Service.Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs) |
| Application Query | [`Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs`](src/Company.Service.Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs) |
| API Controller | [`RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs`](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs) |
| API Contracts | [`RestApi/Api/InvoiceAddresses/V1/Contracts/`](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/Contracts/) |
| EF Configuration | [`Infrastructure.Data/Persistence/EntityConfigurations/InvoiceAddressEntityConfiguration.cs`](src/Company.Service.Infrastructure.Data/Persistence/EntityConfigurations/InvoiceAddressEntityConfiguration.cs) |
| Integration Event | [`Application.IntegrationEvents/V1/InvoiceAddresses/InvoiceAddressCreatedEvent.cs`](src/Company.Service.Application.IntegrationEvents/V1/InvoiceAddresses/InvoiceAddressCreatedEvent.cs) |
| Domain Tests | [`tests/.../Entities/InvoiceAddressTests.cs`](tests/Company.Service.Domain.UnitTests/Entities/InvoiceAddressTests.cs) |
| Handler Tests | [`tests/.../Commands/CreateInvoiceAddressCommandHandlerTests.cs`](tests/Company.Service.Application.UnitTests/Features/InvoiceAdresses/Commands/CreateInvoiceAddressCommandHandlerTests.cs) |
| Validator Tests | [`tests/.../Commands/CreateInvoiceAddressCommandValidatorTests.cs`](tests/Company.Service.Application.UnitTests/Features/InvoiceAdresses/Commands/CreateInvoiceAddressCommandValidatorTests.cs) |
| Integration Tests | [`tests/.../InvoiceAddresses/V1/CreateInvoiceAddressTests.cs`](tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/CreateInvoiceAddressTests.cs) |
