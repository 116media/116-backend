# Domain Entity Spec Template

Copy this template when adding a new domain entity or adding new methods to an existing entity.
Domain entities live in `src/Modules/[Module]/[Module]/Domain/Entities/`.
They have no dependencies — no repositories, no services, no DI.

---

```markdown
# Spec: [EntityName]Entity

## Intent

[What does this entity represent in the domain? What business concept does it model?
What are its key responsibilities?]

---

## Entity Properties

| Property | Type | Visibility | Constraints | Default |
|----------|------|-----------|-------------|---------|
| `Id` | `Guid` | `public { get; private set; }` | PK, generated on Create | `Guid.NewGuid()` |
| `OrderId` | `Guid` | `public { get; private set; }` | Required FK | — |
| `AmountUsd` | `decimal` | `public { get; private set; }` | > 0 | — |
| `Status` | `EnumPaymentStatus` | `public { get; private set; }` | Enum | `Pending` |
| `Notes` | `string?` | `public { get; private set; }` | Max 500 | `null` |

---

## Entity Base Class

```csharp
// Use Aggregate<Guid> when this entity is the root and has domain events
public class MyEntity : Aggregate<Guid>

// Use Entity<Guid> when this entity is a child (owned by an aggregate root)
public class MyEntity : Entity<Guid>
```

---

## Factory Method(s)

```csharp
// Primary creation method — validates invariants inline
public static MyEntity Create(Guid id, Guid parentId, decimal amount)
{
    // Guard against invalid inputs
    if (amount <= 0) throw new ArgumentException("Amount must be positive.");

    return new MyEntity
    {
        Id = id,
        ParentId = parentId,
        Amount = amount,
        Status = EnumMyStatus.Initial
    };
}
```

---

## State Machine

List every valid status transition:

```
Initial   ──→ Active       via Activate()
Active    ──→ Suspended    via Suspend()
Active    ──→ Terminated   via Terminate()
Suspended ──→ Active       via Reactivate()
Suspended ──→ Terminated   via Terminate()
Terminated ──→ [none]      (terminal state)
```

**Invalid transitions (must throw `BadRequestException`):**
- `Active` → `Activate()` — already active
- `Terminated` → any method — terminal state

---

## Domain Methods

For each method, specify:

```
Method: Activate()
Precondition: Status must not be Active (throws BadRequestException)
State change: Status = Active
Returns: void
Guard method name: EnsureNotActive() (internal, called inside Activate)
```

```
Method: EnsureActive()
Purpose: Guard used by other aggregate roots to validate this entity's state
Precondition: Status must be Active
Throws: BadRequestException if not Active
Returns: void
Called by: [list callers if known]
```

---

## Navigation Properties (EF Core)

```csharp
// These are set by EF Core — no public setter — accessed via reflection in builders
public ParentEntity Parent { get; private set; } = null!;
public ICollection<ChildEntity> Children { get; private set; } = [];
```

---

## EF Core Configuration

Describe the table mapping:

```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("my_entities", schema: "content");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountUsd)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<string>()   // or HasConversion<int>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne<ParentEntity>()
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## Test Cases

**Domain entity tests (`MyEntityTests`):**

```
[Create]
- Create_WhenAllValid_ShouldReturnEntityWithExpectedValues
- Create_WhenAmountIsZero_ShouldThrowArgumentException
- Create_WhenAmountIsNegative_ShouldThrowArgumentException

[State transitions — valid]
- Activate_WhenInitial_ShouldSetStatusToActive
- Terminate_WhenActive_ShouldSetStatusToTerminated
- Terminate_WhenSuspended_ShouldSetStatusToTerminated

[State transitions — invalid]
- Activate_WhenAlreadyActive_ShouldThrowBadRequestException
- Activate_WhenTerminated_ShouldThrowBadRequestException

[Guard methods]
- EnsureActive_WhenActive_ShouldNotThrow
- EnsureActive_WhenInitial_ShouldThrowBadRequestException
- EnsureActive_WhenTerminated_ShouldThrowBadRequestException

[Property methods]
- SetNotes_WhenNotesExceedMaxLength_ShouldThrowArgumentException
- SetNotes_WhenNull_ShouldSetNullSuccessfully
```
```

---

## Domain entity rules

1. **No constructor parameters** — EF Core requires a parameterless private constructor
2. **All setters are `private set`** — state changes only through domain methods
3. **Guard methods are `public` and named `Ensure[State]()`** — used by other aggregates
4. **Factory methods are `public static`** and named `Create(...)` or `CreateXxx(...)`
5. **No repository calls** — entities are pure domain, no infrastructure dependencies
6. **Collections initialize to `[]`** — never null
7. **`Aggregate<Guid>` for roots, `Entity<Guid>` for children**