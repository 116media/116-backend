# Example Domain Spec: ContentOrderEntity

This is a **fully worked spec** for `ContentOrderEntity` — the core commerce aggregate.
This entity already exists. Use this file to understand how to write a domain spec
and as a reference when adding new methods to this entity.

---

# Spec: ContentOrderEntity

## Intent

`ContentOrderEntity` is the aggregate root for the content commerce workflow.
It tracks a customer's purchase of promotional content placements (article slots, video slots,
featured promotions, social boosts). It owns its line items and drives the payment lifecycle
through a finite state machine from `Draft` to `Paid` or `Cancelled`.

---

## Entity Properties

| Property | Type | Visibility | Constraints |
|----------|------|-----------|-------------|
| `Id` | `Guid` | `public { get; private set; }` | PK |
| `CustomerId` | `Guid` | `public { get; private set; }` | FK to CustomerEntity |
| `PackageId` | `Guid?` | `public { get; private set; }` | Optional FK to PackageEntity |
| `TotalAmountUsd` | `decimal` | `public { get; private set; }` | ≥ 0, sum of all tier prices |
| `Status` | `EnumOrderStatus` | `public { get; private set; }` | See state machine below |
| `Items` | `ICollection<ContentOrderItemEntity>` | `public { get; private set; }` | Navigation property |

---

## Entity Base Class

```csharp
public class ContentOrderEntity : Aggregate<Guid>
```

`Aggregate<Guid>` — this is the root. It owns `ContentOrderItemEntity` children
and `ContentPaymentEntity` (separate aggregate associated by `OrderId`).

---

## Status Enum

```csharp
public enum EnumOrderStatus
{
    Draft,          // Initial state — items can be added/removed
    PendingPayment, // Submitted — payment expected
    Paid,           // Payment verified
    Cancelled       // Terminal state
}
```

---

## State Machine

```
Draft         ──→ PendingPayment    via Submit()
Draft         ──→ Cancelled         via Cancel()
PendingPayment ──→ Paid             via MarkPaid()
PendingPayment ──→ Cancelled        via Cancel()
Paid          ──→ [none]            (terminal)
Cancelled     ──→ [none]            (terminal)
```

Invalid transitions throw `BadRequestException`.

---

## Factory Method

```csharp
public static ContentOrderEntity Create(Guid id, Guid customerId, Guid? packageId = null)
{
    return new ContentOrderEntity
    {
        Id = id,
        CustomerId = customerId,
        PackageId = packageId,
        TotalAmountUsd = 0m,
        Status = EnumOrderStatus.Draft,
        Items = []
    };
}
```

---

## Domain Methods

### `EnsureDraft()`

```
Purpose:   Guard — verifies the order is in Draft status
Precondition: Status == Draft
Throws:    BadRequestException (ContentOrderErrors.NotInDraft)
Returns:   void
Called by: AdminAddOrderItemFactory, AdminAddItemTierFactory (before adding items/tiers)
```

### `EnsurePendingPayment()`

```
Purpose:   Guard — verifies the order is in PendingPayment status
Precondition: Status == PendingPayment
Throws:    BadRequestException (ContentOrderErrors.NotInPendingPayment)
Returns:   void
Called by: AdminVerifyPaymentFactory, AdminRejectPaymentFactory
```

### `Submit()`

```
Purpose:   Transition Draft → PendingPayment
Precondition: Status == Draft (EnsureDraft() called internally)
State change: Status = PendingPayment
Throws:    BadRequestException if not Draft
Returns:   void
Side note: Does NOT create payment — that is done by AdminSubmitOrderFactory
```

### `MarkPaid()`

```
Purpose:   Transition PendingPayment → Paid
Precondition: Status == PendingPayment (EnsurePendingPayment() called internally)
State change: Status = Paid
Throws:    BadRequestException if not PendingPayment
Returns:   void
```

### `Cancel()`

```
Purpose:   Transition Draft|PendingPayment → Cancelled
Precondition: Status is Draft or PendingPayment
Throws:    BadRequestException if Status is Paid or Cancelled
State change: Status = Cancelled
Returns:   void
```

### `RecalculateTotal(decimal tierPriceUsd)`

```
Purpose:   Increment TotalAmountUsd by the newly added tier's price
Precondition: None (called after EnsureDraft passes)
State change: TotalAmountUsd += tierPriceUsd
Returns:   void
```

---

## EF Core Configuration

```csharp
// Table: content.content_orders
builder.ToTable("content_orders", schema: "content");
builder.HasKey(x => x.Id);

builder.Property(x => x.TotalAmountUsd)
    .HasColumnType("decimal(18,4)")
    .IsRequired();

builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(30)
    .IsRequired();

// FK to Customer (no navigation property on Customer side — unidirectional)
builder.HasOne<CustomerEntity>()
    .WithMany()
    .HasForeignKey(x => x.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

// Owned collection of items
builder.HasMany(x => x.Items)
    .WithOne()
    .HasForeignKey(x => x.OrderId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

## Test Cases

**Domain entity tests (`ContentOrderEntityTests`):**

```
[Create]
- Create_WhenAllValid_ShouldReturnDraftOrderWithZeroTotal
- Create_WhenPackageIdProvided_ShouldSetPackageId
- Create_WhenPackageIdNull_ShouldLeavePackageIdNull

[Submit]
- Submit_WhenDraft_ShouldTransitionToPendingPayment
- Submit_WhenPendingPayment_ShouldThrowBadRequestException
- Submit_WhenPaid_ShouldThrowBadRequestException
- Submit_WhenCancelled_ShouldThrowBadRequestException

[MarkPaid]
- MarkPaid_WhenPendingPayment_ShouldTransitionToPaid
- MarkPaid_WhenDraft_ShouldThrowBadRequestException
- MarkPaid_WhenPaid_ShouldThrowBadRequestException
- MarkPaid_WhenCancelled_ShouldThrowBadRequestException

[Cancel]
- Cancel_WhenDraft_ShouldTransitionToCancelled
- Cancel_WhenPendingPayment_ShouldTransitionToCancelled
- Cancel_WhenPaid_ShouldThrowBadRequestException
- Cancel_WhenAlreadyCancelled_ShouldThrowBadRequestException

[RecalculateTotal]
- RecalculateTotal_WhenCalledOnce_ShouldIncrementTotal
- RecalculateTotal_WhenCalledMultipleTimes_ShouldAccumulate

[Guards]
- EnsureDraft_WhenDraft_ShouldNotThrow
- EnsureDraft_WhenNotDraft_ShouldThrowBadRequestException

  [Theory][InlineData(EnumOrderStatus.PendingPayment)]
  [Theory][InlineData(EnumOrderStatus.Paid)]
  [Theory][InlineData(EnumOrderStatus.Cancelled)]

- EnsurePendingPayment_WhenPendingPayment_ShouldNotThrow
- EnsurePendingPayment_WhenNotPendingPayment_ShouldThrowBadRequestException

  [Theory][InlineData(EnumOrderStatus.Draft)]
  [Theory][InlineData(EnumOrderStatus.Paid)]
  [Theory][InlineData(EnumOrderStatus.Cancelled)]
```

---

## Test Example

```csharp
public class ContentOrderEntityTests
{
    [Fact]
    public void Create_WhenAllValid_ShouldReturnDraftOrderWithZeroTotal()
    {
        Guid id = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        ContentOrderEntity order = ContentOrderEntity.Create(id, customerId);

        order.Id.Should().Be(id);
        order.CustomerId.Should().Be(customerId);
        order.PackageId.Should().BeNull();
        order.TotalAmountUsd.Should().Be(0m);
        order.Status.Should().Be(EnumOrderStatus.Draft);
    }

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Submit();

        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Theory]
    [InlineData(EnumOrderStatus.PendingPayment)]
    [InlineData(EnumOrderStatus.Paid)]
    [InlineData(EnumOrderStatus.Cancelled)]
    public void Submit_WhenNotDraft_ShouldThrowBadRequestException(EnumOrderStatus status)
    {
        // Build an order in the given status using the factory
        ContentOrderEntity order = status switch
        {
            EnumOrderStatus.PendingPayment => ContentOrderFactory.CreateSubmitted(),
            EnumOrderStatus.Paid           => ContentOrderFactory.CreatePaid(),
            EnumOrderStatus.Cancelled      => ContentOrderFactory.CreateCancelled(),
            _                              => throw new ArgumentOutOfRangeException()
        };

        Action act = () => order.Submit();

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void RecalculateTotal_WhenCalledMultipleTimes_ShouldAccumulate()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.RecalculateTotal(100m);
        order.RecalculateTotal(250m);

        order.TotalAmountUsd.Should().Be(350m);
    }
}
```