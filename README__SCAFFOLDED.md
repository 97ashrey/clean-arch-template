# Welcome to CompanyNamePlaceholder.ServiceDomainPlaceholder Service

<!--
  TODO: Describe what this service does, which domain it belongs to,
  and any key responsibilities or capabilities.
-->

#### Contents

- [Introduction](#introduction)
- [Structure](#structure)
- [Prerequisites](#prerequisites)
- [Run](#run)
- [Tech Stack](#tech-stack)

## Introduction

<!-- TODO: Describe the purpose of this service, the problems it solves,
     and any relevant context for developers joining the project. -->

Service manages ...

## Structure

```
Company.Service.sln
├── src/
│   ├── Company.Service.Domain              # Core business logic & rules
│   ├── Company.Service.Application         # Use cases (CQRS commands/queries)
│   ├── Company.Service.Application.IntegrationEvents  # Cross-service event contracts
│   ├── Company.Service.Infrastructure      # External services (messaging, auth)
│   ├── Company.Service.Infrastructure.Data # EF Core DbContext & persistence
│   ├── Company.Service.RestApi             # ASP.NET Web API entry point
│   └── Company.Service.DbDeploy            # Standalone migration runner
├── aspire/
│   └── Company.Service.AppHost             # Aspire orchestrator for local dev
└── tests/
    ├── Company.Service.Domain.UnitTests            # Entity & value object behavior
    ├── Company.Service.Application.UnitTests       # Handler & validator logic
    ├── Company.Service.RestApi.IntegrationTests    # Full HTTP + DB + messaging flow
    └── Company.Service.RestApi.UnitTests           # Controller/filter/middleware tests
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/) (required for running the Aspire host)

## Run

The solution uses the Aspire host project to bootstrap all dependencies locally.

```bash
cd aspire/Company.Service.AppHost
dotnet run
```

This spins up (via Docker):

- **SQL Server** — database for the service
- **RabbitMQ** — message broker for integration events
- **Mock OAuth2 Server** — identity provider for local development (configurable via `Authority:Enabled`)
- **DbDeploy** — applies EF Core migrations and seeds data
- **RestApi** — the service's HTTP API

Swagger UI is available at `https://localhost:{PORT}/swagger` when running in `DEBUG` mode (one Swagger document per API version).

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 10 | Runtime & framework |
| ASP.NET Core | HTTP API |
| Mediator | CQRS command/query dispatch |
| FluentValidation | Input validation |
| Entity Framework Core + SQL Server | Persistence |
| MassTransit + RabbitMQ | Async messaging & integration events |
| Aspire | Local orchestration |
| xUnit + AwesomeAssertions + NSubstitute | Unit testing |
| TestContainers | Integration testing with real SQL Server |
| OpenTelemetry | Traces, metrics, and logs |
| Swagger / OpenAPI | API documentation |
