# Welcome to ServiceDomainPlaceholder Service

#### Contents

- [Introduction](#introduction)
- [Structure](#structure)
- [Run](#run)

## Introduction

Service manages ...

## Structure

- `Company.Service.AppHost` - an aspire project that bootstraps the whole application for local development purposes. 
- `Company.Service.RestApi` - a Web API project that serves as an entry point to the service.
- `Company.Service.Application` - a business layer that contains implementation of domain-specific use cases, organized into highly-cohesive folders.
- `Company.Service.Domain` - a core layer that contains implementation of domain-specific models.
- `Company.Service.Infrastructure` - an infrastructure layer that contains implementation of external services.
- `Company.Service.Infrastructure.Data` - an infrastructure layer that contains implementation of services related to database access i.e. persistence.
- `Company.Service.DbDeploy` - an executable application used for updating the database via database migrations.

## Run

The solution relies on the Company.Service.AppHost, an aspire project, to bootstrap the whole application for local development purposes. `Docker` is required to run containers. Select it from visual studio and just run as usual. The solution will spin up the following services:

- RabbitMq
- SqlDb
- IdentityServerMock
- DbDeploy (applies migrations and initial data seed)
- RestApi

You can chose to manually run each service if you wish to do so, see the `Program.cs` in the aspire project to see which settings you need to provide.