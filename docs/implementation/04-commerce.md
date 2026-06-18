# Commerce Sub-Module — Implementation Plan

> Depends on: Catalog (categories, customers, packages) and Editorial (articles/videos must exist
> to be linked to an order item after payment). This sub-module drives the entire B2B revenue flow.

## Scope

| Entity | SQL Table | Repository |
|---|---|---|
| `ContentOrderEntity` | `content.content_orders` | `IContentOrderRepository` |
| `ContentOrderItemEntity` | `content.content_order_items` | `IContentOrderRepository` |
| `ContentItemTierEntity` | `content.content_item_tiers` | `IContentOrderRepository` |
| `ContentPaymentEntity` | `content.content_payments` | `IContentOrderRepository` |

## Flow recap

```
Admin creates order (customer + optional package)
  → Admin adds order items (category + content_kind per item)
    → Admin adds pricing tiers to each item
      → Order total computed and set
        → Order submitted (status: PendingPayment)
          → Customer sends payment proof
            → Admin attaches proof + verifies payment
              → Payment verified → order status: Paid
                → App stamps social_boost + is_promoted on each article/video
                  → Admin creates the article/video, linking order_item_id
```

---

## 🔴 CRUCIAL — The entire B2B business flow depends on these

---

### POST /api/v1/admin/orders

> Opens a new content order for a B2B client. This is the first step in the revenue flow —
> before any commissioned article or video can be created, an order must exist that links the
> work to the customer who is paying for it. The order starts in `Draft` status and has no items
> or total yet — the admin adds items and tiers before submitting. Optionally linking a package
> lets the admin apply a pre-configured bundle deal to the order instead of building it
> item-by-item from scratch.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateOrderCommand(CustomerId, PackageId?)` |
| **Response** | `201` + `ContentOrderSummaryDto(Id, CustomerName, Status, TotalAmountUsd, CreatedAt, ItemCount)` |

**TODOs**
- [ ] `ContentOrderEntity.Create(id, customerId, packageId)` — status starts as `Draft`, total starts at `0`
- [ ] `CreateOrderCommand(Guid CustomerId, Guid? PackageId) : ICommand<ContentOrderSummaryDto>`
- [ ] `CreateOrderCommandValidator` — `CustomerId` required
- [ ] `CreateOrderCommandHandler` — verifies customer exists (`ICustomerRepository.GetByIdAsync()`), optionally verifies package exists and is active (`IPackageRepository.GetByIdWithSlotsAsync()`), creates entity, calls `IContentOrderRepository.AddAsync()`, commits `IContentUnitOfWork`
- [ ] `ContentOrderRepository.AddAsync(order)`
- [ ] `CreateOrderEndpointV1` Carter module

---

### POST /api/v1/admin/orders/{id}/items

> Adds one commissioned content item to a Draft order. Each item specifies what category the
> content belongs to (e.g. "116 Le Focus"), what kind of content it is (Article or Video), whether
> the client wants a social boost, and whether they are upgrading to a promotion level.
> The promotion level price is snapshotted at this moment so the client's quote is locked even if
> the admin adjusts promotion prices later. After payment is verified, the admin creates the actual
> article or video and links it back to this order item via `order_item_id`.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddOrderItemCommand(OrderId, ContentKind, CategoryId, PromotionLevelId?, SocialBoost, IsBonus)` |
| **Response** | `201` + `OrderItemDto(Id, ContentKind, CategoryName, PromotionLevelName?, PromoPriceUsd?, SocialBoost, IsBonus)` |

> `PromoPriceSnapshotUsd` is frozen from `promotion_levels.price_usd` at this moment.

**TODOs**
- [ ] `ContentOrderItemEntity.Create(id, orderId, contentKind, categoryId, promotionLevelId, promoPriceSnapshotUsd, socialBoost, isBonus)`
- [ ] `AddOrderItemCommand(Guid OrderId, EnumContentKind ContentKind, Guid CategoryId, Guid? PromotionLevelId, bool SocialBoost, bool IsBonus) : ICommand<OrderItemDto>`
- [ ] `AddOrderItemCommandValidator` — order must be in `Draft`, category must be active and `IsFree = false`, if `PromotionLevelId` set it must be active
- [ ] `AddOrderItemCommandHandler` — fetches order (validates `Draft` status), fetches category, optionally fetches promotion level (snapshot its `PriceUsd`), creates item entity, calls `IContentOrderRepository.AddItemAsync()`, commits UoW
- [ ] `ContentOrderRepository.AddItemAsync(item)`
- [ ] `AddOrderItemEndpointV1` Carter module

---

### POST /api/v1/admin/orders/{id}/items/{itemId}/tiers

> Attaches a pricing tier (e.g. `base_upload`, `social_boost`) to a specific order item and
> freezes the current price from the category's pricing table as an immutable snapshot. This
> snapshot is the guarantee to the client that their quoted price cannot change retroactively —
> if the admin later adjusts the category's tier price, existing order items are completely
> unaffected. After each tier is added, the order's total amount is recomputed as the sum of all
> tier snapshots plus the promotion level price.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddItemTierCommand(OrderId, OrderItemId, PricingTierId)` |
| **Response** | `201` + `ItemTierDto(TierName, PriceSnapshotUsd)` |

> `PriceSnapshotUsd` is frozen from `category_pricing.price_usd` for the item's category + this tier.

**TODOs**
- [ ] `ContentItemTierEntity.Create(id, orderItemId, pricingTierId, priceSnapshotUsd)`
- [ ] `AddItemTierCommand(Guid OrderId, Guid OrderItemId, Guid PricingTierId) : ICommand<ItemTierDto>`
- [ ] `AddItemTierCommandValidator` — tier must exist in `category_pricing` for this item's category
- [ ] `AddItemTierCommandHandler` — fetches order item, fetches `CategoryPricingEntity` for `(item.CategoryId, pricingTierId)` to get current price, creates tier entity with snapshot, calls `IContentOrderRepository.AddItemTierAsync()`, recomputes and updates `ContentOrderEntity.TotalAmountUsd` via `SetTotal()`, commits UoW
- [ ] `ContentOrderRepository.AddItemTierAsync(tier)`
- [ ] `AddItemTierEndpointV1` Carter module

---

### PATCH /api/v1/admin/orders/{id}/submit

> Locks the order and moves it from `Draft` to `PendingPayment`. This is the moment the order
> becomes an invoice — no new items or tiers can be added after this point. A `ContentPaymentEntity`
> record is created at submission time with the order's total amount, starting the payment
> tracking lifecycle. The client then receives the total and payment instructions (bank transfer,
> mobile money). At least one item with at least one tier must exist before the order can be
> submitted.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `SubmitOrderCommand(OrderId)` |
| **Response** | `204 No Content` |

> Moves order from `Draft` → `PendingPayment`. Creates the `ContentPaymentEntity` row.

**TODOs**
- [ ] `SubmitOrderCommand(Guid OrderId) : ICommand`
- [ ] `SubmitOrderCommandHandler` — fetches order with items, validates at least one item exists with at least one tier, calls `ContentOrderEntity.Submit()`, creates `ContentPaymentEntity.Create(id, orderId, order.TotalAmountUsd)`, calls `IContentOrderRepository.AddPaymentAsync()`, calls `IContentOrderRepository.UpdateAsync(order)`, commits UoW
- [ ] `ContentPaymentEntity.Create(id, orderId, amountUsd)` — status starts as `Pending`
- [ ] `ContentOrderRepository.AddPaymentAsync(payment)` and `ContentOrderRepository.UpdateAsync(order)` — add `UpdateAsync` to `IContentOrderRepository`
- [ ] `SubmitOrderEndpointV1` Carter module

---

### POST /api/v1/admin/orders/{id}/payment/proof

> Records the customer's payment method and uploads the receipt photo they sent (via WhatsApp,
> email, or portal upload). The receipt image is stored on Cloudinary and its URL is saved in
> the payment record. This endpoint makes the proof available for the admin to review before
> verifying. Without a proof URL the admin has no evidence to base the verification decision on.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `FileUpload` |
| **Command** | `AttachPaymentProofCommand(OrderId, File, PaymentMethod)` |
| **Content-Type** | `multipart/form-data` |
| **Response** | `204 No Content` |

> Admin uploads the customer's payment receipt (image or PDF, max 5 MB) as `multipart/form-data`.
> The file is uploaded to Cloudinary via `IFileRepository.UploadAndStoreRawFileAsync()` (Core module),
> which persists a `FileEntity` in `core.files` and returns the stored metadata. The payment record
> stores `PaymentProofFileId`, `PaymentProofStorageUrl`, and `PaymentProofMimeType` directly
> (denormalized — no cross-context FK). The endpoint returns `201` with a `FileDto` so the frontend
> can decide how to render the proof (image viewer vs PDF viewer) based on `MimeType`.

| | |
|---|---|
| **Response** | `201` + `FileDto(Id, StorageUrl, MimeType, OriginalFileName, SizeInBytes)` |

**TODOs**
- [x] `AttachPaymentProofCommand(Guid OrderId, IFormFile File, EnumPaymentMethod PaymentMethod) : ICommand<AdminAttachPaymentProofResult>`
- [x] `AttachPaymentProofCommandValidator` — `File` not null, `PaymentMethod` required
- [x] `AttachPaymentProofCommandHandler` — fetches payment, calls `IFileRepository.UploadAndStoreRawFileAsync(file, orderId, "content/payment-proofs", originalFileName, mimeType)`, calls `ContentPaymentEntity.AttachProof(proofFileId, storageUrl, mimeType, paymentMethod)`, `UpdatePaymentAsync`, commits UoW
- [x] `AttachPaymentProofEndpointV1` Carter module — `IFormFile file` + `EnumPaymentMethod paymentMethod` form params, `.DisableAntiforgery()`, returns `201`

---

### PATCH /api/v1/admin/orders/{id}/payment/verify

> The critical gate that turns a commissioned deal into active revenue. Verifying the payment:
> (1) marks the payment record as `Verified`; (2) sets the order status to `Paid`; (3) stamps
> `SocialBoost = true` on all articles and videos linked to order items where social boost was
> selected; (4) sets `IsPromoted = true` and computes `PromotedUntil` (based on the promotion
> level's `DurationDays`) on all content items that include a promotion level upgrade. After this
> step the admin can create the actual articles and videos and link them to the order items.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `VerifyPaymentCommand(OrderId, ReceiptUrl, AdminUserId)` |
| **Response** | `204 No Content` |

> Verifying payment:
> 1. Sets payment status to `Verified`
> 2. Sets order status to `Paid`
> 3. Stamps `SocialBoost` and `IsPromoted/PromotedUntil` on each article/video linked to the order items

**TODOs**
- [ ] `VerifyPaymentCommand(Guid OrderId, string ReceiptUrl, Guid AdminUserId) : ICommand`
- [ ] `VerifyPaymentCommandValidator` — `ReceiptUrl` required, max 500 chars
- [ ] `VerifyPaymentCommandHandler`:
  - Fetches order with items and payment
  - Calls `ContentPaymentEntity.Verify(adminUserId, receiptUrl)`
  - Calls `ContentOrderEntity.MarkPaid()`
  - For each order item: if `SocialBoost = true`, fetches linked article/video and calls `ArticleEntity.StampSocialBoost()` / `VideoEntity.StampSocialBoost()`
  - For each order item with a `PromotionLevelId`: fetches promotion level, computes `promotedUntil = UtcNow + durationDays`, calls `ArticleEntity.StampPromotion(until)` / `VideoEntity.StampPromotion(until)`
  - Calls `IContentOrderRepository.UpdatePaymentAsync()` and `IContentOrderRepository.UpdateAsync(order)`
  - Commits UoW
- [ ] `VerifyPaymentEndpointV1` Carter module

---

### PATCH /api/v1/admin/orders/{id}/payment/reject

> Marks the payment as rejected when the proof of payment is invalid, the amount is incorrect,
> or the payment has not actually been received. The admin can add notes explaining the rejection
> so the team knows how to follow up with the client. Rejection does not cancel the order —
> the order remains in `PendingPayment` status so a corrected proof can be submitted.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RejectPaymentCommand(OrderId, Notes?)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `RejectPaymentCommand(Guid OrderId, string? Notes) : ICommand`
- [ ] `RejectPaymentCommandHandler` — fetches payment, calls `ContentPaymentEntity.Reject(notes)`, calls `IContentOrderRepository.UpdatePaymentAsync()`, commits UoW
- [ ] `RejectPaymentEndpointV1` Carter module

---

## 🟡 IMPORTANT — Order management and admin dashboard

---

### GET /api/v1/admin/orders

> Returns the paginated list of all orders with optional filters for status and customer. This is
> the admin team's primary revenue pipeline view — it shows how many orders are in Draft (still
> being built), how many are in `PendingPayment` (waiting for client payment), and how many are
> `Paid` (content creation can begin). Filtering by customer lets the admin quickly pull up all
> orders for a specific artist or label.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllOrdersQuery(Page, PageSize, Status?, CustomerId?)` |
| **Response** | `200` + `PagedResponse<ContentOrderSummaryDto>` |

**TODOs**
- [ ] `GetAllOrdersQuery(int Page, int PageSize, EnumOrderStatus? Status, Guid? CustomerId) : IQuery<PagedResponse<ContentOrderSummaryDto>>`
- [ ] `GetAllOrdersQueryHandler` — calls `IContentOrderRepository.GetAllAsync(page, pageSize, status, customerId)`
- [ ] `ContentOrderRepository.GetAllAsync(page, pageSize, status, customerId)` — includes `Customer` for name, ordered by `CreatedAt DESC`
- [ ] `GetAllOrdersEndpointV1` Carter module

---

### GET /api/v1/admin/orders/{id}

> Returns the complete order detail — customer info, all line items (category, content kind,
> pricing tiers with snapshots, promotion level), and the payment record. Used when the admin
> is preparing to verify a payment and needs to confirm the order composition matches what the
> client paid for. Also used to generate a breakdown for the client if they request an itemized
> quote before payment.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetOrderByIdQuery(Id)` |
| **Response** | `200` + `ContentOrderDetailDto(Id, Customer, Status, TotalAmountUsd, Items[], Payment?)` |

**TODOs**
- [ ] `ContentOrderDetailDto` — extends summary with `Items[]` (`OrderItemDto`) and `Payment?` (`PaymentDto`)
- [ ] `GetOrderByIdQuery(Guid Id) : IQuery<ContentOrderDetailDto>`
- [ ] `GetOrderByIdQueryHandler` — calls `IContentOrderRepository.GetByIdWithItemsAsync(id)`, throws `ResourceNotFoundException` if null
- [ ] `ContentOrderRepository.GetByIdWithItemsAsync(id)` — includes `Items` with `Category`, `PromotionLevel`, `Tiers` with `PricingTier`, and `Payment`
- [ ] `GetOrderByIdEndpointV1` Carter module

---

### PATCH /api/v1/admin/orders/{id}/cancel

> Cancels an order that was opened by mistake or that the client has decided not to proceed with.
> Cancellation is only permitted when the order is in `Draft` or `PendingPayment` — a `Paid` order
> cannot be cancelled because the content creation workflow has already started. Cancelled orders
> are preserved in the database for audit purposes and do not cascade-delete their items.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CancelOrderCommand(OrderId)` |
| **Response** | `204 No Content` |

> Only allowed when status is `Draft` or `PendingPayment`.

**TODOs**
- [ ] `CancelOrderCommand(Guid OrderId) : ICommand`
- [ ] `CancelOrderCommandHandler` — fetches order, validates status is `Draft` or `PendingPayment`, calls `ContentOrderEntity.Cancel()`, calls `IContentOrderRepository.UpdateAsync(order)`, commits UoW
- [ ] `CancelOrderEndpointV1` Carter module

---

## 🟢 MODERATE — Pending payment dashboard and receipt access

---

### GET /api/v1/admin/orders/pending-payment

> Quick dashboard view showing all orders currently waiting for customer payment, ordered
> oldest-first so the most overdue deals surface at the top. This is the admin team's daily
> follow-up list — these are clients who have been sent payment instructions and have not yet
> sent a receipt. Keeping this list short is the most direct indicator of cash flow health on
> the platform.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPendingPaymentOrdersQuery(Page, PageSize)` |
| **Response** | `200` + `PagedResponse<ContentOrderSummaryDto>` |

> Quick admin dashboard view — shows all orders waiting for customer payment.

**TODOs**
- [ ] `GetPendingPaymentOrdersQuery(int Page, int PageSize) : IQuery<PagedResponse<ContentOrderSummaryDto>>`
- [ ] `GetPendingPaymentOrdersQueryHandler` — calls `IContentOrderRepository.GetAllAsync()` with `Status = PendingPayment`, ordered by `CreatedAt ASC` (oldest first)
- [ ] `GetPendingPaymentOrdersEndpointV1` Carter module

---

### GET /api/v1/admin/orders/{id}/payment

> Returns the full payment record for an order — payment method, proof URL, verification status,
> who verified it and when, and the receipt URL. Used by the admin when reviewing a receipt before
> marking it as verified, or when a client claims to have paid and the admin needs to check the
> proof on file. Also provides the receipt URL so the client can download their payment confirmation.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetOrderPaymentQuery(OrderId)` |
| **Response** | `200` + `PaymentDto(Id, AmountUsd, PaymentMethod, PaymentProofUrl, Status, VerifiedBy?, VerifiedAt?, ReceiptUrl?)` |

**TODOs**
- [ ] `PaymentDto(Guid Id, decimal AmountUsd, EnumPaymentMethod? PaymentMethod, FileDto? PaymentProof, EnumPaymentStatus Status, Guid? VerifiedBy, DateTimeOffset? VerifiedAt, string? ReceiptUrl)` — `PaymentProof` is a nested `FileDto` (not a plain URL string) so the frontend gets MIME type to choose the correct renderer
- [ ] `GetOrderPaymentQuery(Guid OrderId) : IQuery<PaymentDto>`
- [ ] `GetOrderPaymentQueryHandler` — calls `IContentOrderRepository.GetPaymentByOrderIdAsync(orderId)`, throws `ResourceNotFoundException` if null
- [ ] `GetOrderPaymentEndpointV1` Carter module

---

## ⚪ TRIVIAL — Customer order history

---

### GET /api/v1/admin/customers/{id}/orders

> Returns the paginated order history for a specific customer. Used when a returning client asks
> about the status of past commissions, when the admin needs to check what content was purchased
> before proposing a new package deal, or when the finance team is auditing revenue by client.
> Delegates internally to the same `GetAllAsync` used by the main orders list with a `CustomerId`
> filter applied.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetCustomerOrdersQuery(CustomerId, Page, PageSize)` |
| **Response** | `200` + `PagedResponse<ContentOrderSummaryDto>` |

**TODOs**
- [ ] `GetCustomerOrdersQuery(Guid CustomerId, int Page, int PageSize) : IQuery<PagedResponse<ContentOrderSummaryDto>>`
- [ ] `GetCustomerOrdersQueryHandler` — calls `IContentOrderRepository.GetAllAsync()` with `CustomerId` filter
- [ ] `GetCustomerOrdersEndpointV1` Carter module