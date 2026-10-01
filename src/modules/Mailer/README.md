# Mailer

Mailer owns two use-case areas, each a folder in `Mailer.Application`:

| Area | Owns |
| --- | --- |
| `Newsletter` | Subscriptions, the confirm and unsubscribe flows and their hosted pages, the subscriber list |
| `Notifications` | In-app notifications, read state, unread counts |

`Templates` is not a use-case area: it holds the localized catalogs and the renderers for the
email bodies (`Shared/Templates/Emails`) and the in-app notifications (`Shared/Templates/Notifications`). A
renderer reads only its catalog, so it is a template, not a service. The
send outbox (`OutboxEmailEntity`), the dispatcher job and the Resend / SMTP senders live in
`Mailer.Infrastructure`, driven on a schedule rather than by a request.

It is one module of the 116 modular monolith: its own database schema (`mailer`), its own layer
projects, and no direct dependency on another module's internals.

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
Mailer.Domain         ─► Shared.Domain, BuildingBlocks.Domain, Mailer.Contracts
Mailer.Application    ─► Mailer.Domain, BuildingBlocks.Presentation, <other>.Contracts
Mailer.Infrastructure ─► Mailer.Application
```

`Mailer.Domain` reaches its own Contracts for one reason: `EnumNotificationType` is published there, and the
entity stores the published value rather than a private copy the seam would have to translate.
No other module's Contracts are visible to it.

Endpoints live in `Mailer.Application` beside the command, handler and validator they serve, which
is why that layer reaches the outermost shared project rather than stopping at
`BuildingBlocks.Application`. The three layers grant each other `InternalsVisibleTo`: the module is
the encapsulation unit, so splitting it into three assemblies must not turn `internal` into layer-
private.

Inside `Mailer.Application`, an interface under `<Area>/Ports/` is implemented in
`Mailer.Infrastructure/Services/`; every other interface in the layer is implemented in the layer,
beside its declaration. Both kinds end in `Service`, so the folder, not the name, says who
implements it. See [`docs/application-services.md`](../../../docs/application-services.md).

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
