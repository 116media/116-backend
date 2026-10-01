# Content

Content owns five areas, each a folder in `Content.Application`:

| Area | Owns |
| --- | --- |
| `Editorial` | Articles, videos, shorts, lyrics, artists, albums, streaming links, and the submit / review / publish / promote workflows over them |
| `Catalog` | Categories, category pricing, packages, package slots, customers |
| `Lookup` | Tags, content types, pricing tiers, promotion levels |
| `Interactions` | Likes, bookmarks, shares, ratings, comments, playlists, view events |
| `Commerce` | Orders, order items, item tiers, payments |

It is one module of the 116 modular monolith: its own database schema (`content`), its own layer
projects, and no direct dependency on another module's internals.

## Layout

```text
Content/                     this folder
├── Content/                 the module itself
│   ├── src/
│   │   ├── Content.Domain/          entities, value objects, domain events — zero packages
│   │   ├── Content.Application/     use cases, handlers, validators, Carter endpoints, mappers
│   │   └── Content.Infrastructure/  DbContext, migrations, repositories, external services
│   └── tests/
│       ├── Content.TestData/           builders and factories its suites share
│       ├── Content.Unit.Tests/         guards, validators, handler orchestration
│       └── Content.Integration.Tests/  real HTTP and real repositories
├── Content.slnf             solution filter for this module
└── README.md                this file
```

`Content.slnf` opens the module, the shared kernel and the host without loading the other three
modules: `dotnet build src/modules/Content/Content.slnf`, or point your IDE at it.

## Reference direction

```text
Content.Domain         ─► Shared.Domain, BuildingBlocks.Domain
Content.Application    ─► Content.Domain, BuildingBlocks.Presentation, <other>.Contracts
Content.Infrastructure ─► Content.Application
```

Endpoints live in `Content.Application` beside the command, handler and validator they serve, which
is why that layer reaches the outermost shared project rather than stopping at
`BuildingBlocks.Application`. The three layers grant each other `InternalsVisibleTo`: the module is
the encapsulation unit, so splitting it into three assemblies must not turn `internal` into layer-
private.

Inside `Content.Application`, an interface under `<Area>/Ports/` is implemented in
`Content.Infrastructure/Services/`; every other interface in the layer is implemented in the layer,
beside its declaration. Both kinds end in `Service`, so the folder, not the name, says who
implements it. See [`docs/application-services.md`](../../../docs/application-services.md).

Content publishes no Contracts project, because nothing else in the monolith reads its data. If that
changes, the type another module needs belongs in a new `Content.Contracts`, not in a layer project.

It consumes `Identity.Contracts`, `Storage.Contracts`, `Mailer.Contracts`.

## Tests

```bash
dotnet test src/modules/Content/Content/tests/Content.Unit.Tests
dotnet test src/modules/Content/Content/tests/Content.Integration.Tests
```

Integration tests need Docker. One PostgreSQL container serves every suite in the repository and
each leases its own database from a migrated template; `tests/Fixtures` owns that machinery and
`tests/TestData` the data helpers every module shares.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/modules/Content/Content/src/Content.Infrastructure \
  --startup-project src/host/Api \
  --context ContentDbContext
```
