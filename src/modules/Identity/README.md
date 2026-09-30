# Identity

Identity owns four use-case areas, each a folder in `Identity.Application`:

| Area | Owns |
| --- | --- |
| `Auth` | Sign-up, login, social login, OTP issue and verification, password set / change / reset, sign-out from one device or all |
| `Roles` | Roles, permissions, role-permission assignment and the soft-delete / restore lifecycle of both |
| `Session` | Sessions, refresh tokens, revocation, forced logout, expiry cleanup, metrics and session-data export |
| `User` | Profiles, avatars, activation and deactivation, user-role assignment |

`Adapters` is not a use-case area: it holds the ports to systems outside the module, social token
verification and client-origin detection, whose implementations live in `Identity.Infrastructure`.

It is one module of the 116 modular monolith: its own database schema (`identity`), its own layer
projects, and no direct dependency on another module's internals.

## Layout

```text
Identity/                     this folder
├── Identity/                 the module itself
│   ├── src/
│   │   ├── Identity.Domain/          entities, value objects, domain events — zero packages
│   │   ├── Identity.Application/     use cases, handlers, validators, Carter endpoints, mappers
│   │   └── Identity.Infrastructure/  DbContext, migrations, repositories, external services
│   └── tests/
│       ├── Identity.TestData/           builders and factories its suites share
│       ├── Identity.Unit.Tests/         guards, validators, handler orchestration
│       └── Identity.Integration.Tests/  real HTTP and real repositories
├── Identity.Contracts/       published seam — what other modules may see
├── Identity.slnf             solution filter for this module
└── README.md                this file
```

`Identity.slnf` opens the module, the shared kernel and the host without loading the other three
modules: `dotnet build src/modules/Identity/Identity.slnf`, or point your IDE at it.

## Reference direction

```text
Identity.Domain         ─► Shared.Domain, BuildingBlocks.Domain
Identity.Application    ─► Identity.Domain, BuildingBlocks.Presentation, <other>.Contracts
Identity.Infrastructure ─► Identity.Application
```

Endpoints live in `Identity.Application` beside the command, handler and validator they serve, which
is why that layer reaches the outermost shared project rather than stopping at
`BuildingBlocks.Application`. The three layers grant each other `InternalsVisibleTo`: the module is
the encapsulation unit, so splitting it into three assemblies must not turn `internal` into layer-
private.

Other modules see `Identity.Contracts` and nothing else: `IUserLookupService`, `IClaimsProvider` and
`AuthorDto` — how another module renders an author or a customer without reading the users table. A
type another module needs belongs there; a `public` type in a layer project is not a seam.

It consumes `Storage.Contracts`, `Mailer.Contracts`.

## Tests

```bash
dotnet test src/modules/Identity/Identity/tests/Identity.Unit.Tests
dotnet test src/modules/Identity/Identity/tests/Identity.Integration.Tests
```

Integration tests need Docker. One PostgreSQL container serves every suite in the repository and
each leases its own database from a migrated template; `tests/Fixtures` owns that machinery and
`tests/TestData` the data helpers every module shares.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/modules/Identity/Identity/src/Identity.Infrastructure \
  --startup-project src/host/Api \
  --context IdentityDbContext
```
