# AGENTS_INTERNAL.md — Template Scaffolding Internals

This file contains notes about how the template is structured for scaffolding.
It is **excluded from the template output** — it only exists in the template repo.

---

## Two-Mode Architecture

This template repo compiles in two modes:

| Mode | Context | `EXAMPLE` constant | What compiles | What goes to output |
|---|---|---|---|---|
| **Template repo** | Source of truth (this repo) | Defined via `Directory.Build.props` | All example features + scaffold | N/A |
| **Scaffolded output** | User's new project | Not defined | Only non-example code | Clean, no examples |

---

## Code Examples in AGENTS.md Must Be Generic

All code examples in `AGENTS.md` must use **generic, domain-agnostic placeholders** — never
feature-specific types, names, or context. This ensures the guide reads as a reusable pattern
reference rather than being tied to any example feature in the repo.

**Guidelines:**

| Avoid (domain-specific) | Use instead (generic) |
|---|---|
| `InvoiceAdressesTestCase` | `EntityQueryTestCase` |
| `InvoiceAddress` | `TEntity` |
| `GetInvoiceAddressesQuery` | `TQuery` |
| `PagedList<InvoiceAddress>` | `TPaginatedResult` |
| `DbContext.InvoiceAdresses` | `DbContext.Set<TEntity>()` |
| `tenantId`, `TenantIds` | `parentId`, `ParentId` |
| "Filter by tenant IDs" | "Filter by parent ID" |
| `homeAddress`, `workAddress` | `child1`, `child2` |
| `Handle_ReturnsInvoiceAdresses` | `Handle_ReturnsEntities` |

**When to apply this**:

1. **Adding a new code example** in AGENTS.md — always use generic types and descriptive-but-neutral names.
2. **Updating an existing example** — replace any domain-specific terms that may have been introduced.

**Exception**: When documenting a **specific example file** as a link reference (e.g., "See example:
[file.cs](path)"), the surrounding prose may refer to that concrete file, but the inline code
blocks in AGENTS.md must remain generic.

**Rationale**: The AGENTS.md is the primary reference for developers using the scaffolded output,
where no example features exist. Generic examples are immediately applicable to their own domain.

---

## How Example Features Are Handled

### InvoiceAddress (kept as commented reference)

All InvoiceAddress files are wrapped with `//__EXAMPLE_START__` / `//__EXAMPLE_END__` markers:

```csharp
//__EXAMPLE_START__
public class InvoiceAddress { ... }
//__EXAMPLE_END__
```

In the template repo: `//__EXAMPLE_START__` is a comment → code compiles normally.
In scaffolded output: marker is replaced with `/*` / `*/` → code is inside a block comment (inert reference).

This is configured in `.template.config/template.json` under `symbols`:
```json
"exampleStart": { "replaces": "//__EXAMPLE_START__", "value": "/*" },
"exampleEnd":   { "replaces": "//__EXAMPLE_END__",   "value": "*/" }
```

### Account / AccountOrder / Subscription (excluded entirely)

These features are **removed from the output** via the template.json `exclude` list:

```json
"exclude": [
    "src/Company.Service.Domain/Entities/Account.cs",
    "src/Company.Service.Application/Features/Accounts/**",
    "src/Company.Service.RestApi/Api/Accounts/**",
    ...
]
```

They still exist in the template repo and compile normally under `EXAMPLE`.

### Bridging Files (DbContext, IApplicationDbContext, ApplicationDbContext)

Three files bridge example features with the core architecture:

- `IApplicationDbContext.cs` — interface defining DbSets
- `ServiceDomainPlaceholderDbContext.cs` — EF Core DbContext
- `ApplicationDbContext.cs` — decorator/facade

These files use `#if EXAMPLE` / `#endif` for the Account/AccountOrder/Subscription DbSets,
and `//__EXAMPLE_START__` / `//__EXAMPLE_END__` for the InvoiceAddress DbSet.

```csharp
#if EXAMPLE
    DbSet<Account> Accounts { get; }
    DbSet<AccountOrder> AccountOrders { get; }
    DbSet<Subscription> Subscriptions { get; }
#endif

//__EXAMPLE_START__
    DbSet<InvoiceAddress> InvoiceAdresses { get; }
//__EXAMPLE_END__
```

In template repo: `EXAMPLE` is defined → all DbSets compile.
In scaffolded output: `EXAMPLE` not defined → Account/Subscription DbSets are gone,
InvoiceAddress DbSet is commented out.

The `using Company.Service.Domain.Entities;` is also wrapped in `//__EXAMPLE_START__`
markers so it doesn't leave an unused-using warning in the output.

### `Directory.Build.props`

Defines the `EXAMPLE` constant for the template repo build.
**Excluded from template output** — the scaffolded project should not define EXAMPLE.

---

## Adding a New Example Feature

1. Create the feature files (entity, handlers, controller, etc.)
2. Wrap each file's content in `//__EXAMPLE_START__` / `//__EXAMPLE_END__`
3. If it references a DbSet, add `#if EXAMPLE` / `#endif` in the 3 bridging files
4. Add file/directory to `template.json` `exclude` list if it shouldn't be in output
5. Update `scripts/clean-examples.sh` — add the new feature's files and directories to the removal lists so the script can clean them from scaffolded output
6. Build and test

### Keeping `scripts/clean-examples.sh` Up to Date

The `scripts/clean-examples.sh` script removes all commented-out example files from a
scaffolded project. When adding a **commented-out** example feature (one wrapped in
`//__EXAMPLE_START__` / `//__EXAMPLE_END__`), you must also add its files to this script.

**What to add**:

1. **Directory patterns** — Add the feature's directory path to the `find -type d` block
   (e.g., `-path "*/InvoiceAddresses"`). This scrubs the entire directory.

2. **File name patterns** — Add each individual file to the `find -type f` block via
   `-name "*.cs"` clauses. Cover all files across layers:
   - Domain entities and value objects
   - Entity type configurations
   - Application commands, queries, handlers, validators
   - API controllers, contracts, mappers
   - Integration events
   - All test files (unit + integration)

**Checklist before committing**:
- All new `.cs` files are represented by a `-name` pattern in the file removal section
- Any new subdirectory is represented by a `-path` pattern in the directory removal section
- Both the singular and plural directory naming conventions are covered (e.g.,
  `InvoiceAddresses` and `InvoiceAdresses` — since the real domain name and the
  DbContext property name may differ)

---

## Integration Events

The `Application.IntegrationEvents` project stays in the scaffolded output (it's a
NuGet-ready project for cross-service contracts). The example events within it
(`V1/Accounts/`, `V1/Shared/`) are wrapped in `//__EXAMPLE_START__` markers.

---

## Migrations

The entire `Migrations/` directory is excluded from template output. The user
creates their own first migration:

```bash
dotnet ef migrations add Initial
```

This generates a clean migration with MassTransit outbox tables + the user's entities.

---

## Tests

All example-specific test files are excluded from template output via `template.json`.
Test infrastructure files (`IntegrationTestBase`, `DbContextTestBase`, etc.) that
don't reference example code ARE included in the output as reusable base classes.
