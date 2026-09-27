# Storage

Storage owns uploaded files: their metadata rows, the Cloudinary transport, colour extraction and
the outbox that cleans up orphaned assets. It is one module of the 116 modular monolith: its own
database schema (`storage`), its own layer projects, and no direct dependency on another module's
internals.

## Layout

```text
Storage/                     this folder
├── Storage/                 the module itself
│   ├── src/
│   │   ├── Storage.Domain/          entities, value objects, domain events — zero packages
│   │   ├── Storage.Application/     use cases, handlers, validators, Carter endpoints, mappers
│   │   └── Storage.Infrastructure/  DbContext, migrations, repositories, external services
│   └── tests/
│       ├── Storage.TestData/           builders and factories its suites share
│       ├── Storage.Unit.Tests/         guards, validators, handler orchestration
│       └── Storage.Integration.Tests/  real HTTP and real repositories
├── Storage.Contracts/       published seam — what other modules may see
├── Storage.slnf             solution filter for this module
└── README.md                this file
```

`Storage.slnf` opens the module, the shared kernel and the host without loading the other three
modules: `dotnet build src/modules/Storage/Storage.slnf`, or point your IDE at it.

## Reference direction

```text
Storage.Domain         ─► Shared.Domain, BuildingBlocks.Domain
Storage.Application    ─► Storage.Domain, BuildingBlocks.Presentation, <other>.Contracts
Storage.Infrastructure ─► Storage.Application
```

Endpoints live in `Storage.Application` beside the command, handler and validator they serve, which
is why that layer reaches the outermost shared project rather than stopping at
`BuildingBlocks.Application`. The three layers grant each other `InternalsVisibleTo`: the module is
the encapsulation unit, so splitting it into three assemblies must not turn `internal` into layer-
private.

Other modules see `Storage.Contracts` and nothing else: `IFileStorageService`, the `FileDto` family,
`EnumStoredFileKind` and `FileUploadLimits` — how another module uploads a file and holds a
reference to it instead of a foreign key. A type another module needs belongs there; a `public` type
in a layer project is not a seam.

The module was called `Core` until stage 18. Four names still say Core deliberately: the
`InitCoreSchema` migration pair, `EnumCoreContentType` and `EnumCoreUserRole`.

## Tests

```bash
dotnet test src/modules/Storage/Storage/tests/Storage.Unit.Tests
dotnet test src/modules/Storage/Storage/tests/Storage.Integration.Tests
```

Integration tests need Docker. One PostgreSQL container serves every suite in the repository and
each leases its own database from a migrated template; `tests/Fixtures` owns that machinery and
`tests/TestData` the data helpers every module shares.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/modules/Storage/Storage/src/Storage.Infrastructure \
  --startup-project src/host/Api \
  --context StorageDbContext
```
