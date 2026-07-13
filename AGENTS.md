# AGENTS.md - Development Guide for Clean Architecture Template

This document provides guidance for agentic development within this clean architecture .NET template. It documents architectural patterns, conventions, and workflows to ensure consistent implementation of new features.

---

## 1. Architecture Overview

### Layered Structure
```
Company.Service.RestApi
├── Controllers (thin, just mapping and mediator calls)
└── API Contracts (request/response DTOs)

Company.Service.Application
├── Features (organized by domain concept)
│   └── [Feature]
│       ├── Commands/
│       ├── Queries/
│       └── Common Types (request/response contracts)
├── Common
│   ├── Requests (ApplicationRequest base class)
│   ├── Behaviours (pipeline behaviors for mediator)
│   └── Types (errors, validation)
└── ConfigureServices.cs (mediator & validator registration)

Company.Service.Domain
├── Entities (rich domain models with behavior)
├── ValueObjects (immutable, self-validating types)
└── Common
    ├── Types (Result<TError>, ValueResult<TValue, TError>)
    └── Utils (validation utilities)

Company.Service.Infrastructure
├── Data (DbContext, migrations)
└── External Services (authentication, message publishing)

Company.Service.Application.IntegrationEvents
└── V1 (integration event contracts for external services)

Company.Service.DbDeploy
└── Migrations (deployed separately from RestApi)
```

### Key Principles
- **Domain-Driven Design**: Rich domain models with encapsulated behavior
- **Result Pattern**: All domain operations return `Result<TError>` or `ValueResult<TValue, TError>` instead of throwing exceptions
- **CQRS**: Commands for writes, Queries for reads
- **No Domain Events**: Not used; Results are the primary communication mechanism
- **Two-Level Error Handling**: Application layer (ExceptionHandlerPipelineBehaviour) and API layer (ExceptionHandlerFilter)

---

## 2. Domain Layer Patterns

### Rich Domain Models

**Location**: `src/Company.Service.Domain/Entities/`

Domain entities contain behavior and validation logic. They are self-validating through factory methods or command methods returning Results.

**See example**: [InvoiceAddress.cs](src/Company.Service.Domain/Entities/InvoiceAddress.cs)

Key patterns:
- Factory methods (`CreateNew`, `Recreate`) return `ValueResult<TEntity, ValidationError>` and validate all inputs
- Command methods return `Result<TError>`/`ValueResult<TValue, TError>` if they can fail, or `void` for simple operations. `TError` is a sub type of `DomainError` like `ValidationError` or `InvalidOperationError`
- Private constructors prevent invalid state, and allow EF Core integration
- All validation logic encapsulated in the entity

### Value Objects

**Location**: `src/Company.Service.Domain/ValueObjects/`

Immutable types that represent a concept and handle their own validation.

**See example**: [Address.cs](src/Company.Service.Domain/ValueObjects/Address.cs)

When to use:
- Use GUIDs for aggregate IDs (simple and effective)
- Use strings and ints for scalar properties where appropriate (no need to over-engineer)
- Use value objects for composed concepts (e.g., Address, Money, PhoneNumber)
- Make value objects records (immutable by default with C# 9+)
- Return `ValueResult<TValueObject, ValidationError>` from factory methods

### Domain Errors

**Location**: `src/Company.Service.Domain/Common/Types/Errors/`

**See example**: [InvalidOperationError.cs](src/Company.Service.Domain/Common/Types/Errors/InvalidOperationError.cs)

Immutable types that represent a concept of a domain error, generics like `InvalidOperationError` are good enough for most cases, but a specific type can be used to convey additional meaning of the error.

### Result Types

**Location**: [Result.cs](src/Company.Service.Domain/Common/Types/Result.cs)

Two primary result types manage success/failure flows:

**Result<TError>** - For operations that don't return a value
- Use for command methods that modify state but don't return values
- `.IsSuccess` property indicates operation outcome
- `.Error` property contains failure details

**ValueResult<TValue, TError>** - For operations that return a value
- Use for factory methods and queries
- `.Value` property contains success result
- `.Error` property contains failure details

**Result API Patterns**:
- `.Bind()` - Chain result-returning operations
- `.Map()` - Transform the value
- `.MapError()` - Transform the error
- `.Tap()` / `.TapError()` - Side effects without changing the result
- `.Match()` - Pattern matching on success/failure
- `.MatchAsync()` - Async pattern matching

### Important: No Domain Events

This template does NOT use domain events because:
1. Results are the primary return mechanism
2. Always need to communicate what went wrong
3. Domain events would require fallback mechanisms for error cases
4. Integration events at the application layer serve the cross-service communication need

---

## 3. Application Layer Patterns

### Directory Structure by Feature

```
src/Company.Service.Application/Features/[FeatureName]/
├── Commands/
│   ├── Create[Entity]Command.cs
│   ├── Update[Entity]Command.cs
│   ├── Delete[Entity]Command.cs
│   └── [Custom]Command.cs
├── Queries/
│   ├── Get[Entity]ByIdQuery.cs
│   ├── Get[Entities]Query.cs
│   └── [Custom]Query.cs
└── (no Dtos folder - contracts defined with commands/queries)
```

### Commands

All commands inherit from `ApplicationRequest<TResponse>`.

**See example**: [CreateInvoiceAddressCommandHandler.cs](src/Company.Service.Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs)

Key patterns:
- Command record with nested DTOs for complex inputs
- Validator defined inline or in same file
- Handler chains domain factory/command methods via `.Bind()`
- Domain errors converted to application errors
- Persistence only after all validations pass

### Queries

All queries inherit from `ApplicationRequest<TResponse>`.

**See example**: [GetInvoiceAddressByIdQueryHandler.cs](src/Company.Service.Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs)

Key patterns:
- Use `.AsNoTracking()` for queries (no tracking needed for reads)
- Return domain entities directly (contract reuse)
- Define projections at the query level if needed (specific DTOs for complex queries)
- Always return appropriate error types (`NotFoundError`, `ValidationError`, etc.)
- Always order paginated queries add `.OrderBy(e => e.Id)` before `.Skip()`/`.Take()` to guarantee stable ordering across pages. Without it, results can be returned in an unpredictable order, potentially causing duplicate or missing items across pages.

### Reusable Output Contracts

For projections or specific query/command outputs, define contracts at the query/command level or in a shared location:
- **Inline**: Define DTOs in the query/command file for single-use projections
- **Shared**: Define DTOs in `Common/Contracts/` if reused across multiple queries

### Application Errors

**Location**: [Common/Types/Errors/](src/Company.Service.Application/Common/Types/Errors/)

Error hierarchy:
- `ApplicationError` (base) - All errors inherit with `Id` and `Message` properties
- `ValidationError` - Validation failures with property/error mappings
- `NotFoundError` - Resource not found
- `InvalidOperationError` - Business rule violations
- Custom errors as needed

---

## 4. API Layer Patterns

### Controllers

**Location**: `src/Company.Service.RestApi/Api/[Feature]/V[Version]/`

**See example**: [InvoiceAddressesController.cs](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs)

Controller patterns:
- Inherit from `ApiControllerBase` for shared response helpers
- Use typed results (`Results<...>`) for compile-time safety
- Map domain entities to API contracts via `.ToV1()`, `.ToV2()` etc.
- Use `.Match()` to convert Result to HTTP response
- Each request type maps to exactly one command/query
- Map request contracts to commands/queries using `.ToCommand()` or `.ToQuery()` methods

### API Request Contracts

**Location**: `src/Company.Service.RestApi/Api/[Feature]/V[Version]/Contracts/`

**See example**: [InvoiceAddress](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/Contracts/InvoiceAddress.cs)

Define input/output contracts that map to application commands/queries:
- Input contracts inherit from API request types (e.g., `CreateInvoiceAddressRequest`)
- Include `.ToCommand()` or `.ToQuery()` mapper methods
- Output contracts define response shape (separate per API version)
- Use nested records for complex structures

### API Contract Mappers

Use extension methods on domain entities or custom projections:
- Define mappers per version (`.ToV1()`, `.ToV2()`, etc.)
- Place in `Contracts/` folder or as static methods in contract files
- Allow different API versions to have different response shapes

### Versioning

APIs are versioned through folder structure/c# namespaces and multiple contracts per feature:
- `V1/` - First version
- `V2/` - Second version (breaking changes)

Map between versions at the controller level, NOT at the domain level.

---

## 5. Error Handling

### Two-Level Error Handling

**Level 1: Application Layer**
- `ExceptionHandlerPipelineBehaviour` catches exceptions during command/query handling
- Logs with error ID for tracing
- Returns application error wrapped in `ValueResult`

**Level 2: API Layer**
- `ExceptionHandlerFilter` catches any unhandled exceptions from controller actions
- Logs with error ID
- Returns HTTP 500 with error ID

### Error to HTTP Response Mapping

Controllers map specific error types to HTTP responses in the controller.

**Mapping Guide**:
- `ValidationError` → HTTP 400 Bad Request
- `NotFoundError` → HTTP 404 Not Found
- `InvalidOperationError` → HTTP 400 Bad Request
- Other `ApplicationError` → HTTP 500 Internal Server Error

---

## 6. Integration Events

Integration events allow services to communicate asynchronously. They're defined in a separate NuGet-ready project.

**Location**: `src/Company.Service.Application.IntegrationEvents/V1/`

### Event Definition

**See example**: [AccountOrderCreatedEvent](src/Company.Service.Application.IntegrationEvents/V1/Accounts/AccountOrderCreatedEvent.cs)

Integration event records contain all relevant data needed by external services:
- Include all data that external services need to know about the event
- Use immutable record syntax
- Include timestamp to track when event occurred
- Versioned by folders/c# namespaces

### Event Publishing

Use MassTransit to publish events from command handlers after successful operations:
- Include all relevant aggregate data in the event
- Events are asynchronously delivered to subscribers
- Outbox pattern is used behind the scenes, events should be published before SaveChanges calls

---

## 7. Testing

### Unit Tests - Domain Layer

**Location**: `tests/Company.Service.Domain.UnitTests/Entities/` and `tests/Company.Service.Domain.UnitTests/ValueObjects/`

**See examples**:
- [AccountTests.cs](tests/Company.Service.Domain.UnitTests/Entities/AccountTests.cs)
- [AccountOrderTests.cs](tests/Company.Service.Domain.UnitTests/Entities/AccountOrderTests.cs)
- [SubscriptionTests.cs](tests/Company.Service.Domain.UnitTests/Entities/SubscriptionTests.cs)
- [AddressTests.cs](tests/Company.Service.Domain.UnitTests/ValueObjects/AddressTests.cs)

**Watch out for these common gaps**

1. **Exact error messages** — Assert `result.Error!.Message` with `Should().Be()` not `Should().Contain()`. When the production code uses string interpolation with an **enum value** (e.g., `$"... {SomeEnum.SomeValue} ..."`), replicate that interpolation in the test assertion rather than hardcoding the enum name as a string — keeps test and source in sync.

2. **`[Theory]` over `foreach` for enums** — Don't iterate enum values in a `[Fact]` with `foreach`. Use `[Theory]` + `[InlineData(SomeEnum.Value)]` so each case is a separate, independently failing test.

3. **State consistency on failure** — When a command method returns a failure `Result`, assert that the entity's state remains unchanged. Don't just check the error — verify the entity wasn't mutated.

### Unit Tests - Application Layer

**Location**: `tests/Company.Service.Application.UnitTests/Features/[Feature]/`

Uses SQLite in-memory database for persistence tests. Base class provided: `DbContextTestBase`

Key patterns:
- Inherit from `DbContextTestBase` for SQLite setup
- Test both success and failure paths
- Assert `result.IsSuccess`, `result.Value`, and `result.Error` appropriately
- Verify domain behavior, not just persistence

#### Error Assertion Guidance

When a handler transforms a domain error into an application error (via `.MapError()`):
- **Assert the error type** (`result.Error.Should().BeOfType<SomeError>()`) — this verifies the handler
  correctly classified the error
- **Do NOT assert the exact error message** — the message originates from the domain layer,
  which already has its own unit tests covering that content. The handler test should verify
  *what* error is returned, not *what it says*.

Exceptions — assert the message when the handler **constructs** the error itself:
- `NotFoundError` messages (constructed inline in the handler with the entity ID)
- `ValidationError` messages when the message text is defined in the handler (not the domain)
- When the error message includes dynamic values the handler injects (e.g., entity IDs)

#### Validation Testing in Handlers

Handler tests should include **one** validation failure test to prove the domain-to-application
error mapping occurs. Do not test every possible validation rule — the domain layer already has
its own unit tests covering each rule individually.

```csharp
[Fact]
public async Task Handle_WithInvalidData_ReturnsValidationError()
{
    // Arrange — single invalid field proves mapping occurs
    var command = new CreateCommand { /* … one invalid field … */ };

    // Act
    var result = await _sut.Handle(command, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Value.Should().BeNull();
    result.Error.Should().BeOfType<ValidationError>();
}
```

#### Validator Testing

Every command/query validator must have a dedicated test class with tests for the
validator itself (not the handler). These tests use `FluentValidation.TestHelper`
to verify validation rules directly, independently of handler logic.

**Location**: Same directory as handler tests, named `[Command/Query]ValidatorTests.cs`

**Test patterns**:
- One `CreateValidCommand()` factory method to establish a baseline valid command
- One `[Fact]` per rule proving it fires on invalid input, using `with` expressions
- One `[Fact]` for the valid case (`ShouldNotHaveAnyValidationErrors()`)
- One `[Fact]` combining multiple invalid fields to verify the total error count
- Assert the error code (e.g., `WithErrorCode("NotEmptyValidator")`) to pin the exact rule

```csharp
[Fact]
public void Validate_WithEmptyName_ShouldHaveError()
{
    // Arrange
    var command = CreateValidCommand() with { Name = string.Empty };

    // Act
    var result = _validator.TestValidate(command);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorCode("NotEmptyValidator");
}
```

**See examples**:
- [CreateInvoiceAddressCommandValidatorTests.cs](tests/Company.Service.Application.UnitTests/Features/InvoiceAdresses/Commands/CreateInvoiceAddressCommandValidatorTests.cs)
- [UpdateInvoiceAddressCommandValidatorTests.cs](tests/Company.Service.Application.UnitTests/Features/InvoiceAdresses/Commands/UpdateInvoiceAddressCommandValidatorTests.cs)

#### Persistence Validation

When verifying an entity was persisted to the database, assert **all fields** match the
expected values — not just the field that changed. This guards against silent data loss
or mapping errors.

```csharp
// Verify it was persisted — all fields, not just the one that changed
DbContext.ChangeTracker.Clear();
var persisted = DbContext.Items.FirstOrDefault(i => i.Id == result.Value!.Id);
persisted.Should().NotBeNull();
persisted!.TenantId.Should().Be(command.TenantId);
persisted.Name.Should().Be(command.Name);
persisted.Email.Should().Be(command.Email);
persisted.NestedValue.Name.Should().Be(command.NestedValue.Name);
persisted.NestedValue.Value.Should().Be(command.NestedValue.Value);
// ... all properties including nullable ones ...
persisted.Status.Should().Be(ItemStatus.Pending);
persisted.CreatedDate.Should().Be(fakeTimeProvider.GetUtcNow().DateTime);
persisted.ParentId.Should().BeNull();
persisted.CompletedDate.Should().BeNull();
```

#### Event Publishing Verification

When a handler publishes an integration event, verify not only that it was published,
but also that **all event fields are correctly mapped** from the source data.
Use NSubstitute's `Arg.Do` to capture the published event, then assert each field
with `Should()` for clear failure messages.

```csharp
// Arrange — set up capture before the Act call
ItemCreatedEvent? capturedEvent = null;
publishEndpoint.Publish(
        Arg.Do<ItemCreatedEvent>(e => capturedEvent = e),
        Arg.Any<CancellationToken>())
    .Returns(Task.CompletedTask);

// Act
var result = await _sut.Handle(command, default);

// Assert result and persistence (all fields)...

// Assert event fields against persisted data, not the command
capturedEvent.Should().NotBeNull();
capturedEvent!.ItemId.Should().Be(persisted.Id);
capturedEvent.TenantId.Should().Be(persisted.TenantId);
capturedEvent.Name.Should().Be(persisted.Name);
capturedEvent.NestedValue.Name.Should().Be(persisted.NestedValue.Name);
capturedEvent.CreatedDate.Should().Be(persisted.CreatedDate);
```

Assert against the persisted entity (loaded after `DbContext.ChangeTracker.Clear()`)
to verify the full chain: command → domain → database → event.

This applies to both unit tests (with mocked publish endpoint) and integration tests
(use `MassTransitTestHarness.Published<T>()` to get the event and assert its properties).

#### Query Tests with Complex Filters (TheoryData with TestCase class)

For list queries with filtering/pagination options, use `TheoryData<TestCase>` with a test case class
containing `Seed`, `Query`, and `Assertion` fields.

**See example**: [GetInvoiceAddressesQueryHandlerTests.cs](tests/Company.Service.Application.UnitTests/Features/InvoiceAdresses/Queries/GetInvoiceAddressesQueryHandlerTests.cs)

The test case class wraps the assert lambda behind a `private get` to prevent xUnit from
treating it as a test case to serialize. The `CreateFromFactory` factory method enables lazy
initialization so each test case gets fresh data (e.g., random GUIDs):

```csharp
public class EntityQueryTestCase
{
    public required string Name { get; init; }

    public required List<TEntity> Seed { get; init; }

    public required TQuery Query { get; init; }

    public required Action<TPaginatedResult, List<TEntity>> Assertion { private get; init; }

    public void Assert(TPaginatedResult result)
    {
        Assertion(result, Seed);
    }

    public static EntityQueryTestCase CreateFromFactory(Func<EntityQueryTestCase> factory) => factory();
}
```

Test data is defined as a `static TheoryData<TestCase>` property using collection expressions.
Simple cases where all data is known upfront use a direct `new() { ... }` entry.
Cases requiring random or computed data at runtime use `CreateFromFactory(() => ...)`:

```csharp
public static TheoryData<EntityQueryTestCase> Data =>
[
    // Simple case — all values known at compile/collection-init time
    new()
    {
        Name = "Get all entities",
        Seed = [entity1, entity2],
        Query = new(),
        Assertion = (result, seed) =>
        {
            result.Items.Should().BeEquivalentTo(seed);
        }
    },
    // Factory case — needs fresh random GUIDs per test run
    EntityQueryTestCase.CreateFromFactory(() =>
    {
        var parentId = Guid.NewGuid();
        return new()
        {
            Name = "Filter by parent ID",
            Seed = [child1, child2, otherChild],
            Query = new() { ParentId = parentId },
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEquivalentTo([child1, child2]);
            }
        };
    }),
];
```

The test method seeds the database, saves, clears the change tracker to simulate a fresh
read, calls the handler, and delegates assertions to the test case's `Assert` method:

```csharp
[Theory]
[MemberData(nameof(Data))]
public async Task Handle_ReturnsEntities(EntityQueryTestCase testCase)
{
    // Arrange
    DbContext.Set<TEntity>().AddRange(testCase.Seed);
    await DbContext.SaveChangesAsync();
    DbContext.ChangeTracker.Clear();

    // Act
    var queryResult = await _sut.Handle(testCase.Query, default);

    // Assert
    queryResult.IsSuccess.Should().BeTrue();
    testCase.Assert(queryResult.Value!);
}
```

**Key rules**:
- Assert against the **seed objects** or the **query's filter values**, not against hardcoded strings or IDs — this keeps tests in sync with test data
- Use `BeEquivalentTo` for unordered collection comparisons (ignores insertion order)
- Use `HaveCount` + `ContainEquivalentOf` / `NotContain` for more explicit assertions when needed
- Always call `DbContext.ChangeTracker.Clear()` after seeding to ensure the handler reads fresh data from the database, not the tracked entities
- Cover edge cases: empty results, filters with no matches, pagination (first page, middle page, last page), and combinations of multiple filters

#### Assert Values from Source Objects, Not Literals

In success-path assertions, reference the **source object's properties** rather than hardcoded strings:

```csharp
// Prefer this:
result.Value.Name.Should().Be(command.Name);
result.Value.Email.Should().Be(command.Email);

// Over this:
result.Value.Name.Should().Be("Test Name");
result.Value.Email.Should().Be("test@example.com");
```

This applies to assertions against:
- **Command/query properties** — use `command.PropertyName`, `query.PropertyName`
- **Seeded entity properties** — use `entity.PropertyName`, `order.PropertyName`
- **FK-related identifiers** — use the seeded parent entity's property, not a copy of the value

This keeps tests in sync with test data — if the test data changes, assertions automatically follow.

### Integration Tests - RestApi Layer

**Location**: `tests/Company.Service.RestApi.IntegrationTests/[Feature]/V[Version]/`

**See examples**:
- [CreateInvoiceAddressTests.cs](tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/CreateInvoiceAddressTests.cs) — validation theory + success pattern
- [GetInvoiceAddressesTests.cs](tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/GetInvoiceAddressesTests.cs) — TheoryData query tests

Uses TestContainers for actual SQL Server database.

**Test Infrastructure**:
- `IntegrationTestBase`: Base class providing DbContext, HttpClient, Mediator, FakeTimeProvider, MassTransit test harness
- `IntegrationTestWebAppFactory`: WebApplicationFactory that configures TestContainers
- `TestContainerFixture`: Manages SQL Server container lifecycle
- `MassTransitTestHarness`: Captures published events for verification
- `FakeTimeProvider`: Controllable time source replacing `TimeProvider.System` — allows deterministic time assertions without `DateTime.UtcNow`

**Integration Test Patterns**:

#### Validation Tests (TheoryData with `with` expressions)
Consolidate repetitive validation tests into a single `[Theory]` using `TheoryData<string, Func<...>>`.
Use `with` expressions on a record to produce each invalid variant from a single inline base request:

```csharp
public static TheoryData<string, Func<CreateItemRequest, CreateItemRequest>> ValidationTestCases
{
    get
    {
        var data = new TheoryData<string, Func<CreateItemRequest, CreateItemRequest>>
        {
            { "TenantId", r => r with { TenantId = Guid.Empty } },
            { "Name", r => r with { Name = string.Empty } },
            { "Nested.Property", r => r with { Nested = r.Nested with { Property = string.Empty } } },
            // ...
        };
        return data;
    }
}

[Theory]
[MemberData(nameof(ValidationTestCases))]
public async Task Create_ReturnsBadRequest_WhenFieldIsInvalid(
    string expectedErrorKey,
    Func<CreateItemRequest, CreateItemRequest> invalidate)
{
    var request = invalidate(new() { /* valid base request */ });
    var response = await Client.PostAsJsonAsync("/api/v1/...", request);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    problemDetails!.Errors.Should().ContainKey(expectedErrorKey);
}
```

#### Success Tests
- For endpoints requiring FK-constrained entities, seed the parent entities directly via `DbContext` before calling the API
- Always call `DbContext.ChangeTracker.Clear()` after seeding to avoid state leaks
- Verify the full response body (all properties), not just `Id`
- Verify the entity was persisted to the database via `DbContext.FindAsync`
- For commands that publish integration events, verify via `MassTransitTestHarness.Published<T>()`

```csharp
// Seed FK-dependent data
var parentEntity = ParentEntity.CreateNew(tenantId, ...).Value!;
DbContext.ParentEntities.Add(parentEntity);
await DbContext.SaveChangesAsync();
DbContext.ChangeTracker.Clear();

// Act
var response = await Client.PostAsJsonAsync("/api/v1/...", request);

// Assert response
response.StatusCode.Should().Be(HttpStatusCode.OK);
var result = await response.Content.ReadFromJsonAsync<ItemResponse>();
result!.Id.Should().NotBeEmpty();
result.TenantId.Should().Be(tenantId);
// ... verify all properties

// Assert persistence
var persisted = await DbContext.Items.FindAsync([result.Id], CancellationToken.None);
persisted.Should().NotBeNull();

// Assert integration event (if applicable)
var eventPublished = await MassTransitTestHarness.Published<ItemCreatedEvent>();
eventPublished.Should().BeTrue();
```

#### Query Tests (TheoryData with TestCase class)
For list endpoints with filtering/pagination, use `TheoryData<TestCase>` with a test case class
containing `Seed`, `Request`, and `Assert` fields.

The test case class wraps the assert lambda behind a `private get` to prevent xUnit from
treating it as a test case to serialize. The `CreateFromFactory` factory method enables lazy
initialization so each test case gets fresh data:

```csharp
public class GetItemsTestCase
{
    public required string Name { get; init; }

    public required List<DomainEntity> Seed { get; init; }

    public required GetItemsRequest Request { get; init; }

    public required Action<PagedResponse<ItemResponse>, List<DomainEntity>> Assert { private get; init; }

    public void AssertResponse(PagedResponse<ItemResponse> response, List<DomainEntity> seed)
    {
        Assert(response, seed);
    }

    public static GetItemsTestCase CreateFromFactory(Func<GetItemsTestCase> factory) => factory();
}
```

Test data is defined as a `TheoryData` using collection expressions with `CreateFromFactory`.

```csharp
public static TheoryData<GetItemsTestCase> Data =>
[
    GetItemsTestCase.CreateFromFactory(() =>
    {
        var items = CreateItems();
        return new()
        {
            Name = "Filter by tenant IDs",
            Seed = items,
            Request = new() { TenantIds = [tenantId] },
            Assert = (pagedResponse, seed) =>
            {
                // assertions on pagedResponse only...
            }
        };
    }),
];
```

#### Time-Sensitive Tests using FakeTimeProvider

The `FakeTimeProvider` (from `Microsoft.Extensions.Time.Testing`) replaces the real `TimeProvider.System`
in the integration test DI container. Use it to make time assertions deterministic and to simulate time passage.

**Available on `IntegrationTestBase`** as the `FakeTimeProvider` property.

**Key methods**:
- `FakeTimeProvider.SetUtcNow(DateTimeOffset)` — set the fake clock to a specific moment
- `FakeTimeProvider.GetUtcNowDateTime()` — get the current fake time as `DateTime` (convenience helper from `Common.Utils`)

**Pattern — asserting creation timestamps**:
```csharp
FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

var response = await Client.PostAsJsonAsync("/api/v1/...", request);
var created = await response.Content.ReadFromJsonAsync<...>();
created.CreatedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());

var persisted = await DbContext.Entities.FindAsync([created.Id]);
persisted!.CreatedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());
```

**Pattern — simulating time passing (e.g., state transitions)**:
```csharp
// Arrange: seed entity at time T0
FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

// ... create and persist entity ...

// Act: advance clock to T0 + 1 day for the completion operation
FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(1));
var response = await Client.PutAsJsonAsync($"/api/v1/.../{entity.Id}/complete", null);

// Assert: completed date reflects the advanced time
var result = await response.Content.ReadFromJsonAsync<...>();
result.CompletedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());

var persisted = await DbContext.Entities.FindAsync([entity.Id]);
persisted!.CompletedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());
```

**Important guidelines**:
- Always call `FakeTimeProvider.SetUtcNow()` at the start of each test to establish a known baseline
- Assert time-dependent fields using `FakeTimeProvider.GetUtcNowDateTime()`, never `DateTime.UtcNow` or `DateTime.Now`
- When seeding entities with timestamps (e.g., `createdDate`), pass `FakeTimeProvider.GetUtcNowDateTime()` as the seed value to stay consistent
- This replaces any need for `DateTime.UtcNow` mocking, `Thread.Sleep`, or other time-dependent hacks

**See real examples**:
- [CreateAccountOrderTests.cs](tests/Company.Service.RestApi.IntegrationTests/Accounts/V1/CreateAccountOrderTests.cs) — asserting `CreatedDate` after creation
- [CompleteAccountOrderTests.cs](tests/Company.Service.RestApi.IntegrationTests/Accounts/V1/CompleteAccountOrderTests.cs) — simulating time passing for state transitions

### Testing Best Practices

1. **Use AwsomeAssertions** community fork of **FluentAssertions**
2. **Unit tests** focus on command/query handlers with SQLite — assert `result.IsSuccess`, `result.Value`, and `result.Error`
3. **Integration tests** verify full HTTP flow with real database
4. **Clear database** between tests using Respawn
5. **Test both success and failure paths** for every handler — but **only one validation failure test** is needed in handler tests to prove error mapping works; individual validation rules are tested at the domain layer
6. **Consolidate validation tests** into a single `[Theory]` using `TheoryData<string, Func<...>>` with `with` expressions — avoids 90% boilerplate duplication
7. **Verify full response body** in success tests (all properties, not just Id)
8. **Verify DB persistence with all fields** — when checking persistence, assert every property of the persisted entity against the source values, not just the field that changed
9. **Seed FK-constrained parents** via DbContext before testing entities with foreign keys
10. **Use FakeTimeProvider for all time-dependent assertions** — never depend on `DateTime.UtcNow` in integration tests; call `FakeTimeProvider.SetUtcNow()` at test start and assert against `FakeTimeProvider.GetUtcNowDateTime()`
11. **Assert values from source objects, not literals** — in success assertions, reference the command/entity/query properties (`command.Name`, `entity.Email`) instead of hardcoded strings
12. **Don't assert error messages for domain-transformed errors in handler tests** — when a handler maps a domain error via `.MapError()`, assert only the error type, not the message (the domain already covers message content)
13. **Verify integration event fields, not just publication** — when a handler publishes an event, capture it with `Arg.Do<>` and assert every field is correctly mapped from the source data
14. **Verify contract mapping with a dedicated `[Fact]` for every endpoint returning a response contract** — Every endpoint that returns a response contract should have a test that seeds data, calls the endpoint, and asserts every property of the response matches expected values
15. **Verify code coverage after writing unit tests** — Run `bash scripts/test-report.sh unit` to generate an HTML coverage report for all unit tests. Open `test-coverage-reports/index.html` to inspect per-class coverage. Use `--no-build --no-restore` for quick iterations after code changes, or omit them for a full build.

---

## 8. Workflow for Adding Features

### Step 1: Define Domain Model
```
src/Company.Service.Domain/Entities/[Entity].cs
src/Company.Service.Domain/ValueObjects/[ValueObject].cs
```

- Implement factory methods with Result validation
- Implement behavior methods
- Use Result types for fallible operations

### Step 2: Add Entity Configuration and Database Migration
```
src/Company.Service.Infrastructure.Data/Persistence/EntityConfigurations/[Entity]EntityConfiguration.cs
src/Company.Service.DbDeploy/Migrations/
```

- Implement `IEntityTypeConfiguration<TEntity>` to configure table mapping, keys, required fields, max lengths, and owned types
- Place configuration in `Infrastructure.Data/Persistence/EntityConfigurations/`
- EF Core auto-discovers configurations via `ApplyConfigurationsFromAssembly` in `ServiceDomainPlaceholderDbContext.OnModelCreating`
- Add EF Core migration via `dotnet ef migrations add` in the DbDeploy project
- Migrations are deployed separately via the DbDeploy project

### Step 3: Create Application Commands/Queries
```
src/Company.Service.Application/Features/[Feature]/Commands/
src/Company.Service.Application/Features/[Feature]/Queries/
```

- Define command/query record inheriting from `ApplicationRequest<TResponse>`
- Add FluentValidation validator
- Implement handler using domain methods
- Map domain errors to application errors

### Step 4: Define Integration Event (if needed)
```
src/Company.Service.Application.IntegrationEvents/V1/[Event].cs
```

- Define event record with all relevant data
- Publish from command handler on success

### Step 5: Create API Contracts and Controller
```
src/Company.Service.RestApi/Api/[Feature]/V1/Contracts/
src/Company.Service.RestApi/Api/[Feature]/V1/[Feature]Controller.cs
```

- Define request/response contracts
- Add mapper extension methods
- Implement controller with Result matching

### Step 6: Write Tests
```
tests/Company.Service.Application.UnitTests/Features/[Feature]/
tests/Company.Service.RestApi.IntegrationTests/[Feature]/V1/
```

- Unit tests with SQLite for business logic
- Integration tests with TestContainers for HTTP flow

---

## 9. Key Conventions

### Naming
- **Entities**: Singular noun (e.g., `InvoiceAddress`, not `InvoiceAddresses`)
- **Commands**: `[Verb][Entity]Command` (e.g., `CreateInvoiceAddressCommand`)
- **Queries**: `Get[Entity|Entities][By/]Query` (e.g., `GetInvoiceAddressByIdQuery`)
- **Features**: Plural noun (e.g., `/Features/InvoiceAddresses/`)

### Access Modifiers
- **Domain entities**: `public class` (aggregate roots)
- **Domain value objects**: `public record` (immutable)
- **Application handlers**: `internal class` (implementation detail)
- **Application validators**: `internal class`

### Dependency Injection
- Registered via `ConfigureServices.AddApplicationServices<TUserProvider>()`
- Mediator configured with source generation (`Mediator.AddMediator`)
- Validators auto-registered from assembly
- Pipeline behaviors: logging, exception handling, validation

### Database Context
- Named: `ServiceDomainPlaceholderDbContext`
- Interface: `IApplicationDbContext`
- Injected into handlers for persistence
- DbContext moved to Infrastructure.Data layer

---

## 10. Anti-Patterns to Avoid

❌ **Domain events** - Use Results instead; events published at application layer
❌ **Anemic models** - Move behavior into domain entities
❌ **Service layer** - Use Mediator command/query handlers instead
❌ **Throwing exceptions for validation** - Return Results from domain methods
❌ **Persisting in query handlers** - Queries are read-only
❌ **Contracts in domain** - Keep domain model separate from API contracts
❌ **Direct DbContext queries in controllers** - Use Mediator

---

## 11. Common Tasks for Agents

### Adding a new CRUD feature
1. Create domain entity with factory and behavior methods
2. Add entity type configuration and database migration
3. Create Create/Update/Delete commands with handlers
4. Create Get by ID and Get list queries with handlers
5. Define integration event if cross-service communication needed
6. Create API contracts and controller endpoints
7. Add unit and integration tests

### Modifying an existing feature
1. Update domain entity behavior/factory as needed
2. Update entity type configuration if schema changed
3. Add migration if database schema changed
4. Update command/query handlers to use new behavior
5. Update integration events if need, consider backward compatibility (add new V2)
6. Update API contracts if input/output changed
7. Update existing tests or add new test cases
8. Consider backward compatibility for API changes (add new V2)

### Debugging a failing test
1. Check if Result.IsSuccess is being properly tested
2. Verify domain validation is being called via Results
3. For integration tests, ensure database is properly seeded
4. Use Respawn if database state is causing issues
5. Check error logs in integration test output

---

## 12. References

### Key Files
- Domain Result types: [src/Company.Service.Domain/Common/Types/Result.cs](src/Company.Service.Domain/Common/Types/Result.cs)
- Application Request base: [src/Company.Service.Application/Common/Requests/ApplicationRequest.cs](src/Company.Service.Application/Common/Requests/ApplicationRequest.cs)
- Exception handling: [src/Company.Service.Application/Common/Behaviours/ExceptionHandlerPipelineBehaviour.cs](src/Company.Service.Application/Common/Behaviours/ExceptionHandlerPipelineBehaviour.cs)
- Service registration: [src/Company.Service.Application/ConfigureServices.cs](src/Company.Service.Application/ConfigureServices.cs)

### Example Vertical Slice
- Domain: [src/Company.Service.Domain/Entities/InvoiceAddress.cs](src/Company.Service.Domain/Entities/InvoiceAddress.cs)
- Command: [src/Company.Service.Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs](src/Company.Service.Application/Features/InvoiceAddresses/Commands/CreateInvoiceAddressCommandHandler.cs)
- Query: [src/Company.Service.Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs](src/Company.Service.Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs)
- Controller: [src/Company.Service.RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/InvoiceAddressesController.cs)
- Tests: [tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/GetInvoiceAddressByIdTests.cs](tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/GetInvoiceAddressByIdTests.cs)

---

**Last Updated**: June 2026 — Removed some duplicate mentions of various topics
