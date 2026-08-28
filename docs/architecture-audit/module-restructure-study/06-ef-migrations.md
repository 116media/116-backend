# 06 — EF Core Migrations Under the Restructure

**Verdict: NEUTRAL now, trending EASIER once four small design-time factories exist.** Migrations are not a
blocker for the restructure — the dependency direction, per-schema history isolation, and interceptor
sharing all survive untouched. This is the one area where the split has no real downside.

---

## The four DbContexts

| Context | Schema | DbSets | Migrations |
|---|---|---|---|
| `IdentityDbContext` | `identity` | 7 | 3 |
| `CoreDbContext` | `core` | 1 | 4 |
| `MailerDbContext` | `mailer` | 3 | 2 |
| `ContentDbContext` | `content` | ~57 | 29 |

All four use the identical `OnModelCreating`: `HasDefaultSchema(<schema>)` +
`ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`.

## What survives the split untouched

- **Reference direction is already clean.** Configurations import `…Domain.Entities`; the DbContext imports
  `…Domain.Entities` + `…Domain.Constants`. Direction is Infrastructure → Domain, never Presentation. Under
  the split, `<M>.Infrastructure` references `<M>.Domain` and holds the DbContext, Configurations, **and**
  Migrations together — so `Assembly.GetExecutingAssembly()` still resolves the configurations and the
  migrations-assembly still equals the context-assembly (**no `MigrationsAssembly()` override needed**).
- **Per-schema history is already isolated.** Each context's `HasDefaultSchema` puts its
  `__EFMigrationsHistory` in its own schema (`identity.__EFMigrationsHistory`, `content.…`, etc.). The four
  never collide; the split preserves this exactly.
- **Shared interceptors move cleanly.** `AuditableEntityInterceptor` and `DispatchDomainEventsInterceptor`
  live in `Shared/Infrastructure` and are wired centrally by `BaseModule`. After the split they live in
  `Shared.Infrastructure` and each `<M>.Infrastructure` keeps its existing `Shared` reference — still
  referenceable, no change. (Design-time model building skips interceptors anyway — they affect
  `SaveChanges`, not the model.)

## The one real gotcha: design-time context construction

**No `IDesignTimeDbContextFactory` exists today** (grep returns zero). Design-time works purely because
`--startup-project src/Api` boots `Program.cs`, which loads env via `DotNetEnv` and registers all four
contexts through `BaseModule`. The connection string comes from `BaseModule.GetDefaultConnectionString()` →
env vars (`POSTGRES_HOST/PORT/DB/USER/PASSWORD`). `migrations add` only builds the model (never connects),
so it scaffolds fine even with absent values; only `database update` needs a live DB.

- **If you keep the Api host as `--startup-project`:** migrations are literally unchanged except the
  `--project` path — zero new files. This is the NEUTRAL floor.
- **For true module independence** (the restructure's goal — migrate `<M>.Infrastructure` standalone without
  building the whole Api graph): each module needs its **own** `IDesignTimeDbContextFactory`, because
  standalone there is no host to resolve `DbContextOptions`.

### Post-split migration commands

Keeping the Api host (works today, zero new files):

```bash
dotnet ef migrations add <Name> --project modules/Content/src/Content.Infrastructure   --startup-project host/Api --context ContentDbContext
dotnet ef migrations add <Name> --project modules/Identity/src/Identity.Infrastructure --startup-project host/Api --context IdentityDbContext
dotnet ef migrations add <Name> --project modules/Core/src/Core.Infrastructure         --startup-project host/Api --context CoreDbContext
dotnet ef migrations add <Name> --project modules/Mailer/src/Mailer.Infrastructure     --startup-project host/Api --context MailerDbContext
```

Standalone (module independence — requires the factory below):

```bash
dotnet ef migrations add <Name> --project modules/Content/src/Content.Infrastructure --startup-project modules/Content/src/Content.Infrastructure
```

### The four factories to add (~15 lines each)

One per module in `<M>.Infrastructure/Persistence/`, mirroring `BaseModule.ConfigureDbContextOptions`
(Npgsql + snake_case) and loading env itself since there's no host:

```csharp
public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        var (host, port, db, user, pass) = AppEnvironment.Database();
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseNpgsql($"Host={host};Port={port};Database={db};Username={user};Password={pass};")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ContentDbContext(options);
    }
}
```

The tree in [03](03-full-target-structure.md) already places these as
`modules/<M>/src/<M>.Infrastructure/Persistence/<M>DbContextFactory.cs`.

## Bottom line

Migrations are a non-issue for the restructure. Keep the Api host and nothing changes; add four ~15-line
factories and each module builds and migrates in isolation — arguably **easier** than today, where a
Content migration drags in the full Api graph. The only thing to flag in any plan: the standalone factory
must call `DotNetEnv` itself because the connection string is env-derived with no host at design time.
