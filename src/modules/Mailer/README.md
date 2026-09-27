# Mailer

Mailer owns outbound email and in-app notifications: templates, the send outbox, the dispatcher job
and newsletter subscriptions. It is one module of the 116 modular monolith: its own database schema
(`mailer`), its own layer projects, and no direct dependency on another module's internals.

## Layout

```text
Mailer/                     this folder
├── Mailer/                 the module itself
│   ├── src/
│   │   ├── Mailer.Domain/          entities, value objects, domain events — zero packages
│   │   ├── Mailer.Application/     use cases, handlers, validators, Carter endpoints, mappers
│   │   └── Mailer.Infrastructure/  DbContext, migrations, repositories, external services
│   └── tests/
│       ├── Mailer.TestData/           builders and factories its suites share
│       ├── Mailer.Unit.Tests/         guards, validators, handler orchestration
│       └── Mailer.Integration.Tests/  real HTTP and real repositories
├── Mailer.Contracts/       published seam — what other modules may see
├── Mailer.slnf             solution filter for this module
└── README.md                this file
```

`Mailer.slnf` opens the module, the shared kernel and the host without loading the other three
modules: `dotnet build src/modules/Mailer/Mailer.slnf`, or point your IDE at it.

## Reference direction

```text
Mailer.Domain         ─► Shared.Domain, BuildingBlocks.Domain
Mailer.Application    ─► Mailer.Domain, BuildingBlocks.Presentation, <other>.Contracts
Mailer.Infrastructure ─► Mailer.Application
```

Endpoints live in `Mailer.Application` beside the command, handler and validator they serve, which
is why that layer reaches the outermost shared project rather than stopping at
`BuildingBlocks.Application`. The three layers grant each other `InternalsVisibleTo`: the module is
the encapsulation unit, so splitting it into three assemblies must not turn `internal` into layer-
private.

Other modules see `Mailer.Contracts` and nothing else: `IEmailDispatcher`, `IEmailService`,
`INotificationService`, `OutboundEmail` and `EnumNotificationType` — how another module asks for an
email or a notification. A type another module needs belongs there; a `public` type in a layer
project is not a seam.

It consumes `Identity.Contracts`.

## Tests

```bash
dotnet test src/modules/Mailer/Mailer/tests/Mailer.Unit.Tests
dotnet test src/modules/Mailer/Mailer/tests/Mailer.Integration.Tests
```

Integration tests need Docker. One PostgreSQL container serves every suite in the repository and
each leases its own database from a migrated template; `tests/Fixtures` owns that machinery and
`tests/TestData` the data helpers every module shares.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/modules/Mailer/Mailer/src/Mailer.Infrastructure \
  --startup-project src/host/Api \
  --context MailerDbContext
```
