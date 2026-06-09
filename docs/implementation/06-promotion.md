# Content Promotion System — Implementation Spec

> Depends on: Commerce (04-commerce.md) — promotion levels and orders must exist.

---

## Concept

`IsPromoted` / `PromotedUntil` on `ArticleEntity` and `VideoEntity` represent **paid promotion
placement** — not an editorial toggle. A content item becomes promoted when a customer purchases
a `PromotionLevel` through the Commerce flow and an admin verifies the payment.

**Admins cannot manually set `IsPromoted`** via article/video update endpoints. It is stamped
exclusively by `AdminVerifyPaymentFactory` after successful Commerce payment verification.

---

## Promotion Chain

```
ContentOrder
  └── ContentOrderItem.PromotionLevelId → PromotionLevel.DurationDays
                                                   ↓
                              article/video.IsPromoted = true
                              article/video.PromotedUntil = VerifiedAt + DurationDays
```

- One article/video → one `OrderItemId` (nullable) → one active promotion at a time
- `PromoPriceSnapshotUsd` on `ContentOrderItemEntity` freezes the price at purchase time
- Public featured endpoints filter by:
  `IsPromoted == true && Status == Published && (PromotedUntil == null || PromotedUntil > now)`

---

## Domain Entity Fields

### Added to `ArticleEntity` and `VideoEntity`

| Field | Type | Description |
|-------|------|-------------|
| `IsPromoted` | `bool` | Whether an active paid promotion is in effect (default: false) |
| `PromotedUntil` | `DateTimeOffset?` | When the promotion expires (null = no expiry set) |
| `UnpromotedAt` | `DateTimeOffset?` | Timestamp of a force-unpromote action (null if never applied) |
| `UnpromotedById` | `Guid?` | Identity UUID of the SuperAdmin who force-unpromoted |
| `UnpromotedReason` | `string?` | Reason given at force-unpromote time (max 500 chars) |

### Retired names (do not use)

| Old name | Replacement |
|----------|-------------|
| `IsFeatured` | `IsPromoted` |
| `FeaturedUntil` | `PromotedUntil` |
| `StampFeatured(until)` | `StampPromotion(until)` |

---

## Domain Methods

### `StampPromotion(DateTimeOffset until)`

Called **by Commerce payment verification only** (`AdminVerifyPaymentFactory`).

```csharp
public void StampPromotion(DateTimeOffset until)
{
    IsPromoted = true;
    PromotedUntil = until;
}
```

### `ForceUnpromote(Guid superAdminId, string reason)`

Called **by the SuperAdmin force-unpromote endpoint only**.

```csharp
public void ForceUnpromote(Guid superAdminId, string reason)
{
    if (!IsPromoted)
        throw new BadRequestException("Article is not currently promoted.");

    IsPromoted = false;
    PromotedUntil = null;
    UnpromotedAt = DateTimeOffset.UtcNow;
    UnpromotedById = superAdminId;
    UnpromotedReason = reason;
}
```

Guard: throws `BadRequestException` if content is not currently promoted.
Does **not** touch the order or payment — those stay intact for refund calculation.

---

## Force-Unpromote Endpoints (SuperAdmin only)

| Method | Endpoint | Auth |
|--------|----------|------|
| `PATCH` | `/api/v1/admin/articles/{slug}/unpromote` | SuperAdmin only |
| `PATCH` | `/api/v1/admin/videos/{slug}/unpromote` | SuperAdmin only |

**Request body:**
```json
{ "reason": "government request" }
```
`reason` is required, max 500 chars.

**Response (200 OK):**
```json
{ "articleId": "guid", "unpromotedAt": "2026-05-10T06:38:00Z" }
```

**Error responses:**
- `400` — content is not currently promoted
- `401` — not authenticated
- `403` — not SuperAdmin
- `404` — slug not found
- `429` — rate limit exceeded

**Handler flow:**
1. Fetch article/video by slug — throw `NotFoundException` if null
2. Call `entity.ForceUnpromote(superAdminId, reason)`
3. `repository.Update(entity)`
4. `unitOfWork.CommitAsync(ct)`
5. Return result with `entity.Id` + `entity.UnpromotedAt!.Value`

**Extract SuperAdminId in endpoint:**
```csharp
Guid superAdminId = Guid.Parse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

**Route constant to add to `EditorialRouteConstants.cs`:**
```csharp
public const string Unpromote = "unpromote";
```

---

## Files to Create

```
src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/
├── ForceUnpromoteArticle/
│   ├── AdminForceUnpromoteArticleCommand.cs     ← command + result records
│   ├── AdminForceUnpromoteArticleHandler.cs     ← ICommandHandler implementation
│   ├── AdminForceUnpromoteArticleValidator.cs   ← FluentValidation: Slug + Reason
│   ├── AdminForceUnpromoteArticleMetaField.cs   ← Swagger metadata
│   └── V1/
│       └── AdminForceUnpromoteArticleEndpointV1.cs
└── ForceUnpromoteVideo/
    ├── AdminForceUnpromoteVideoCommand.cs
    ├── AdminForceUnpromoteVideoHandler.cs
    ├── AdminForceUnpromoteVideoValidator.cs
    ├── AdminForceUnpromoteVideoMetaField.cs
    └── V1/
        └── AdminForceUnpromoteVideoEndpointV1.cs
```

---

## Files to Modify

| File | Change |
|------|--------|
| `Domain/Entities/ArticleEntity.cs` | Rename fields + add new method + 3 new properties |
| `Domain/Entities/VideoEntity.cs` | Same |
| `Specifications/ArticleSpecifications.cs` | Rename in `FeaturedArticleSpecification` |
| `Specifications/VideoSpecifications.cs` | Rename in `FeaturedVideoSpecification` |
| `VerifyPayment/AdminVerifyPaymentFactory.cs` | `StampFeatured` → `StampPromotion` |
| `Configurations/ArticleConfiguration.cs` | Rename default + add 3 new property configs |
| `Configurations/VideoConfiguration.cs` | Same |
| `Editorial/Constants/EditorialRouteConstants.cs` | Add `Unpromote` constant |
| `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs` | `AsFeatured` → `AsPromoted`, `StampFeatured` → `StampPromotion` |
| `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs` | Add `AsPromoted` support |
| `tests/Fixtures/Factories/Content/ArticleFactory.cs` | Update `CreateFeatured` |
| `tests/Fixtures/Factories/Content/VideoFactory.cs` | Update `CreateFeatured` |
| `tests/Unit/.../ArticleEntityTests.cs` | Rename test + add 3 ForceUnpromote tests |
| `tests/Unit/.../VideoEntityTests.cs` | Same |
| `tests/Unit/.../ArticleSpecificationsTests.cs` | Rename property refs |
| `tests/Unit/.../VideoSpecificationsTests.cs` | Same |

---

## Unit Tests Required

### `ArticleEntityTests.cs` and `VideoEntityTests.cs`

1. **`StampPromotion_ShouldSetIsPromotedAndPromotedUntil`** (rename of existing test)
   - Assert `IsPromoted == true`, `PromotedUntil == until`

2. **`ForceUnpromote_WhenArticleIsPromoted_ShouldClearPromotionAndRecordAudit`**
   - Build promoted article, call `ForceUnpromote(superAdminId, "government request")`
   - Assert: `IsPromoted == false`, `PromotedUntil == null`, `UnpromotedById == superAdminId`,
     `UnpromotedReason == "government request"`, `UnpromotedAt` is not null and ≈ `UtcNow`

3. **`ForceUnpromote_WhenArticleIsNotPromoted_ShouldThrowBadRequestException`**
   - Build non-promoted article, assert `Throw<BadRequestException>()`

4. **`ForceUnpromote_ShouldNotAffectOtherFields`**
   - Build promoted+published article with social boost
   - Assert `Status == Published`, `SocialBoost == true`, title/slug unchanged after unpromote

---

## Refund Calculation (future feature)

All data needed is already stored at order/payment time:

| What | Source |
|------|--------|
| Amount paid | `ContentPaymentEntity.AmountUsd` (immutable snapshot) |
| Promotion price paid | `ContentOrderItemEntity.PromoPriceSnapshotUsd` (immutable snapshot) |
| Promotion duration | `PromotionLevel.DurationDays` |
| Promotion start | `ContentPaymentEntity.VerifiedAt` |
| Promotion end | `VerifiedAt + DurationDays` |
| Force-unpromote date | `entity.UnpromotedAt` |
| Payment method | `ContentPaymentEntity.PaymentMethod` |
| Payment proof | `ContentPaymentEntity.PaymentProofFileId` + `ReceiptUrl` |

**Pro-rata formula:**
```
days_remaining = PromotedUntil - UnpromotedAt
refund_amount  = PromoPriceSnapshotUsd × (days_remaining / DurationDays)
```
