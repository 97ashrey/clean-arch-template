# Welcome to the ServiceDomainPlaceholder Service DbDeploy

#### Contents

- [Introduction](#introduction)
- [Adding new migrations](#adding-new-migrations)

## Introduction

This piece of code is reposinble for managing and applying database migrations for the ServiceDomainPlaceholder Service. It relies on EFCore [DbContext](../Company.Service.Infrastructure.Data/Persistence/ServiceDomainPlaceholderDbContext.cs) provided by the `Infrastructure.Data` project to create and apply the migrations.

## Adding new migrations

The [DesignTimeDbContextFactory](./DesignTimeDbContextFactory.cs) is configured to target the sql server db that is constructed by the aspire project. To add a new migration, run the aspire project to create the database then change the directories to the DbDeploy project `cd src/Company.Service.DbDeploy`, and run the `dotnet ef migrations add [MigrationName]` command to create a new migration. This will generate a new migration file in the [Migrations](./Migrations/) folder.