# 10 — Entity / Behavior Split (partial classes, mirrored folders)

## Intention

Each domain entity is separated into **state** and **behavior**, as two `partial class` files that
compile into one type:

- **`Entities/<Name>.cs`** — the entity's **state only**: backing fields, properties, and the private
  EF constructor. Nothing else. Opening this file answers "what does this entity hold?" at a glance.
- **`Behaviors/<Name>.cs`** — the entity's **behavior only**: every method, including the static
  `Create*` factories, the state-transition methods, and the invariant guards.

The two files have the **same class name and the same file name**, living in two mirrored folders. The
goal is that the state surface of every aggregate is readable in isolation, while the behavior that
makes it a rich entity is grouped separately.

> Scope: this is a **file-organization convention**, not a modeling change. It composes with — and does
> **not** replace — the domain fixes in [03](../03-content-domain.md) (aggregate boundaries, guards,
> value objects). Splitting the file does not make an anemic entity rich; the behavior itself does.

---

## The rule

### 1. Two partial files, one type

```csharp
// File: Domain/Entities/ArticleEntity.cs   → STATE
namespace _116.Content.Domain.Entities;

public partial class ArticleEntity : Aggregate<Guid>
{
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public EnumContentStatus Status { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public int LikeCount { get; private set; }
    // …all other fields & properties…

    private ArticleEntity() { }   // EF Core
}
```

```csharp
// File: Domain/Behaviors/ArticleEntity.cs   → BEHAVIOR
namespace _116.Content.Domain.Entities;      // ← SAME namespace, NOT .Behaviors

public partial class ArticleEntity
{
    public static ArticleEntity CreateFree(...) { ... }   // factories are methods → here
    public static ArticleEntity CreatePaid(...) { ... }

    public void Publish(...) { ... }
    public bool Submit() { ... }
    public void Approve(...) { ... }
    public void IncrementLikeCount() => LikeCount++;
    // …all methods…
}
```

### 2. The namespace rule (critical — or it will not compile)

`partial` merges files by **class name + namespace**, and ignores file name and folder entirely. So the
`Behaviors/` file **must declare the entity's own namespace** (`_116.Content.Domain.Entities`), **not**
`_116.Content.Domain.Behaviors`. The `Behaviors/` folder is *physical organization only*; it is **not** a
namespace boundary.

- If the folder-based-namespace convention is enforced in the project (analyzer `IDE0130`,
  *"namespace does not match folder structure"*), **suppress it for the `Behaviors/` folder** (an
  `.editorconfig` rule scoped to `Domain/Behaviors/**` setting `dotnet_diagnostic.IDE0130.severity =
  none`), or turn off `<EnforceCodeStyleInBuild>` for that path. Otherwise both files stay under
  `…Domain.Entities` by hand and it compiles as one class.

### 3. What goes where

| Element | File |
|---|---|
| backing fields, properties (`{ get; private set; }`) | `Entities/<Name>.cs` |
| private parameterless EF constructor | `Entities/<Name>.cs` |
| static factories (`CreateFree`, `CreatePaid`, `Create`) | `Behaviors/<Name>.cs` |
| state-transition methods (`Publish`, `Submit`, `Approve`, …) | `Behaviors/<Name>.cs` |
| counter mutators, guards, computed helpers | `Behaviors/<Name>.cs` |
| domain-event raises (`AddDomainEvent(...)`) inside methods | `Behaviors/<Name>.cs` |

### 4. When a behavior file exists

- An entity **with methods** → gets a `Behaviors/<Name>.cs`. (Even a pure junction row usually has a
  static `Create` factory, so it gets one.)
- An entity **with truly no methods at all** → lives only in `Entities/<Name>.cs`; no empty behavior
  file is created.

### 5. Non-negotiables

- Same class name, same file name; only the folder differs.
- Both files carry the same accessibility and base type is declared **once** (on the state file); the
  behavior file's `partial class ArticleEntity` needs no `: Aggregate<Guid>` repeat (allowed, but keep it
  on one file to avoid duplication).
- Never put behavior for two different entities in one file — one type per file pair, mirrored by name.

---

## Folder structure

The `Domain` layer gets two mirrored folders. Every entity that has behavior appears in **both**, under
the **same file name**:

```text
Content.Domain/
├── Content.Domain.csproj
├── Entities/                          # STATE — fields & properties only
│   ├── ArticleEntity.cs
│   ├── VideoEntity.cs
│   ├── LyricsEntity.cs
│   ├── ShortVideoEntity.cs
│   ├── ContentOrderEntity.cs
│   ├── ContentOrderItemEntity.cs
│   ├── ContentPaymentEntity.cs
│   ├── CategoryEntity.cs
│   ├── ArtistEntity.cs
│   ├── AlbumEntity.cs
│   ├── ArticleCommentEntity.cs
│   ├── ArticleLikeEntity.cs
│   ├── ArticleTagEntity.cs
│   └── …                             # every entity's state file
├── Behaviors/                         # BEHAVIOR — methods only (mirrors Entities/ by file name)
│   ├── ArticleEntity.cs
│   ├── VideoEntity.cs
│   ├── LyricsEntity.cs
│   ├── ShortVideoEntity.cs
│   ├── ContentOrderEntity.cs
│   ├── ContentOrderItemEntity.cs
│   ├── ContentPaymentEntity.cs
│   ├── CategoryEntity.cs
│   ├── ArtistEntity.cs
│   ├── AlbumEntity.cs
│   ├── ArticleCommentEntity.cs
│   ├── ArticleLikeEntity.cs          # (present only if this entity has methods beyond nothing)
│   └── …
├── Events/                           # domain events (unchanged)
├── Enums/
└── ValueObjects/
```

The two folders stay **name-for-name symmetric**: for every `Entities/X.cs` that has behavior there is a
`Behaviors/X.cs`, so you can flip between an entity's shape and its logic by switching folders with the
same file name.

Applied across the four modules, each module's `*.Domain` project follows the identical layout —
`Identity.Domain/{Entities,Behaviors}`, `Content.Domain/{Entities,Behaviors}`, etc.

---

## Practical notes

- **Two files with the same name compile fine.** MSBuild and the C# compiler key off the *type*, not the
  file path; `Entities/ArticleEntity.cs` + `Behaviors/ArticleEntity.cs` produce one `ArticleEntity` type.
- **EF Core is unaffected.** Configuration (`IEntityTypeConfiguration<ArticleEntity>`) targets the type;
  it does not care that the type is split across files.
- **IDE tabs.** Because the two files share a name, editors show two `ArticleEntity.cs` tabs — they
  disambiguate by folder (`Entities/` vs `Behaviors/`). This is the one ergonomic cost of same-name
  files (versus the `ArticleEntity.cs` + `ArticleEntity.Behavior.cs` suffix style, which nests in the
  tree). It is the direct consequence of the same-file-name-in-two-folders choice.
- **Tests.** Unit tests still reference the single `ArticleEntity` type; nothing about the test surface
  changes.
- **Reviewability.** A schema/property change shows up as a diff in `Entities/`; a logic change shows up
  in `Behaviors/` — the split you want for review.

---

## Consistency with the rest of the study

- This changes only the **physical file layout of the Domain layer**; it does not alter the module set,
  the layer projects ([09](09-sharedkernel-vs-buildingblocks-rules.md)), or any boundary decision.
- It is orthogonal to the domain-modeling work in [03](../03-content-domain.md): the aggregate-boundary,
  state-machine-guard, and value-object fixes still apply and land in the same `Behaviors/` /
  `ValueObjects/` files. Do those for correctness; this split is for readability.
