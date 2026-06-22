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
- Command methods return `Result<ValidationError>` if they can fail, or `void` for simple operations
- Private constructors prevent invalid state; reconstruction via internal constructors
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
- Domain validation errors converted to application `ValidationError`
- Persistence only after all validations pass



### Queries

All queries inherit from `ApplicationRequest<TResponse>`.

**See example**: [GetInvoiceAddressByIdQueryHandler.cs](src/Company.Service.Application/Features/InvoiceAddresses/Queries/GetInvoiceAddressByIdQueryHandler.cs)

Key patterns:
- Use `.AsNoTracking()` for queries (no tracking needed for reads)
- Return domain entities directly (contract reuse)
- Define projections at the query level if needed (specific DTOs for complex queries)
- Always return appropriate error types (`NotFoundError`, `ValidationError`, etc.)

### Reusable Output Contracts

For projections or specific query outputs, define contracts at the query/command level or in a shared location:
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

Each error type maps to specific HTTP responses in the controller.

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

**Location**: [Contracts/](src/Company.Service.RestApi/Api/InvoiceAddresses/V1/Contracts/)

Define input/output contracts that map to application commands/queries:
- Input contracts inherit from API request types (e.g., `CreateInvoiceAddressRequest`)
- Include `.ToCommand()` or `.ToQuery()` mapper methods
- Output contracts define response shape (separate per API version)
- Use nested records for complex structures

### API Contract Mappers

Use extension methods on domain entities:
- Define mappers per version (`.ToV1()`, `.ToV2()`, etc.)
- Place in `Contracts/` folder or as static methods in contract files
- Allow different API versions to have different response shapes

### Versioning

APIs are versioned through folder structure and multiple contracts per feature:
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

### Application Error Types

**Location**: [Common/Types/Errors/](src/Company.Service.Application/Common/Types/Errors/)

Error hierarchy:
- `ApplicationError` (base) - All errors inherit with `Id` and `Message` properties
- `ValidationError` - Validation failures with property/error mappings
- `NotFoundError` - Resource not found
- `InvalidOperationError` - Business rule violations
- Custom errors as needed

### Error to HTTP Response Mapping

Controllers map specific error types to HTTP responses in the controller.

**Mapping Guide**:
- `ValidationError` → HTTP 400 Bad Request
- `NotFoundError` → HTTP 404 Not Found
- `InvalidOperationError` → HTTP 400 Bad Request (or 409 Conflict)
- Other `ApplicationError` → HTTP 500 Internal Server Error

---

## 6. Integration Events

### Overview

Integration events allow services to communicate asynchronously. They're defined in a separate NuGet-ready project.

**Location**: `src/Company.Service.Application.IntegrationEvents/V1/`

### Event Definition

**Location**: `src/Company.Service.Application.IntegrationEvents/V1/`

Integration event records contain all relevant data needed by external services:
- Include all data that external services need to know about the event
- Use immutable record syntax
- Include timestamp to track when event occurred

### Event Publishing

Use MassTransit to publish events from command handlers after successful operations:
- Publish only after persistence succeeds
- Include all relevant aggregate data in the event
- Events are asynchronously delivered to subscribers

### Important: No Domain Events

This template does NOT use domain events because:
1. Results are the primary return mechanism
2. Always need to communicate what went wrong
3. Domain events would require fallback mechanisms for error cases
4. Integration events at the application layer serve the cross-service communication need

---

## 7. Testing

### Unit Tests - Application Layer

**Location**: `tests/Company.Service.Application.UnitTests/Features/[Feature]/`

Uses SQLite in-memory database for persistence tests. Base class provided: `DbContextTestBase`

Key patterns:
- Inherit from `DbContextTestBase` for SQLite setup
- Test both success and failure paths
- Assert `result.IsSuccess`, `result.Value`, and `result.Error` appropriately
- Verify domain behavior, not just persistence

### Integration Tests - RestApi Layer

**Location**: `tests/Company.Service.RestApi.IntegrationTests/[Feature]/V[Version]/`

**See example**: [GetInvoiceAddressByIdTests.cs](tests/Company.Service.RestApi.IntegrationTests/InvoiceAddresses/V1/GetInvoiceAddressByIdTests.cs)

Uses TestContainers for actual SQL Server database.

**Test Infrastructure**:
- `IntegrationTestBase`: Base class providing DbContext, HttpClient, Mediator, MassTransit test harness
- `IntegrationTestWebAppFactory`: WebApplicationFactory that configures TestContainers
- `TestContainerFixture`: Manages SQL Server container lifecycle
- `MassTransitTestHarness`: Captures published events for verification

Key patterns:
- Inherit from `IntegrationTestBase`
- Test full HTTP flow from controller to persistence
- Verify HTTP status codes and response payloads
- Database cleared between tests via Respawn

### Testing Best Practices

1. **Use AwsomeAssertions** community fork of **FluentAssertions**
2. **Unit tests** focus on command/query handlers with SQLite
3. **Integration tests** verify full HTTP flow with real database
4. **Clear database** between tests using Respawn
5. **Assert Result patterns** thoroughly - check `IsSuccess`, `Value`, and `Error`
6. **Verify domain behavior** not just persistence
7. **Test both success and failure paths** for every handler

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

### Step 2: Create Application Commands/Queries
```
src/Company.Service.Application/Features/[Feature]/Commands/
src/Company.Service.Application/Features/[Feature]/Queries/
```

- Define command/query record inheriting from `ApplicationRequest<TResponse>`
- Add FluentValidation validator
- Implement handler using domain methods
- Map domain errors to application errors

### Step 3: Create API Contracts and Controller
```
src/Company.Service.RestApi/Api/[Feature]/V1/Contracts/
src/Company.Service.RestApi/Api/[Feature]/V1/[Feature]Controller.cs
```

- Define request/response contracts
- Add mapper extension methods
- Implement controller with Result matching

### Step 4: Define Integration Event (if needed)
```
src/Company.Service.Application.IntegrationEvents/V1/[Event].cs
```

- Define event record with all relevant data
- Publish from command handler on success

### Step 5: Add Database Migration
```
src/Company.Service.DbDeploy/Migrations/
```

- Add EF Core migration for new entity
- Deployed separately via DbDeploy project

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
- Named: `ServiceDomainPlaceholderDbContext` (update to your service name)
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
2. Create Create/Update/Delete commands with handlers
3. Create Get by ID and Get list queries with handlers
4. Create API contracts and controller endpoints
5. Add migration
6. Add unit and integration tests
7. Define integration event if cross-service communication needed

### Modifying an existing feature
1. Update domain entity behavior/factory as needed
2. Update command/query handlers to use new behavior
3. Update API contracts if input/output changed
4. Add migration if database schema changed
5. Update existing tests or add new test cases
6. Consider backward compatibility for API changes (add new V2)

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

**Last Updated**: June 2026
