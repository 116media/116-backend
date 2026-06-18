# Content Module: Pricing System & Workflow

## Overview

The content module covers all editorial and video content published on 116. Content is either:

- **Free** — Platform-generated content (scandals, gossip, trending news) to drive traffic and engagement
- **Paid** — Commissioned content where artists, labels, or businesses pay to have their story told, promoted, or featured on the platform

The pricing system is **admin-managed**: a Super Admin creates content types, categories, and pricing tiers directly from the dashboard. No hardcoded prices exist in code.

---

## Content Types

### Articles

| Format | Description | Pricing Type |
|---|---|---|
| Chronique Sale | Scandals / Gossip / viral news | Free |
| Buzz de la semaine | Weekly industry buzz | Free |
| Artist Profile | In-depth artist biography or spotlight | Paid |
| Album / Single Review | Music review with editorial commentary | Paid |
| À la Une | Homepage top story (highest visibility) | Paid (premium placement) |
| Sponsored Spotlight | Brand or product placement editorial | Paid |
| Editorial Partner Post | Brand collaboration / product placement | Paid |
| Lyrics Page | Dedicated page for song lyrics (SEO-optimised) | Paid |

### Videos

| Show | Description | Pricing Type |
|---|---|---|
| 116 Music Video | Official music video or lyric video upload | Paid |
| 116 Podcast | Music talk, debates, multi-guest shows | Paid |
| 116 Interview | One-on-one artist interview | Paid |
| 116 FlexBeat | Beat breakdown and production secrets (producer spotlight) | Paid |
| 116 BTS | Behind the scenes: studio, video shoots, events | Paid |
| 116 Le Focus | Raw personal sit-down with one artist | Paid |
| 116 Reality | Tour life, daily hustle, artist lifestyle | Paid |
| 116 Discovery | New talent spotlight (Noisey-style) | Paid |
| 116 Documentary | Culture, history, movement documentaries | Paid |
| 116 Behind the Lyrics | Artist explains the lyrics of their song (Lemonade-style) | Paid |
| 116 Lyric Video | Official lyric video (SEO-optimised) | Paid |
| 116 Studio | Artist cooks, smokes, vibes in the studio while talking music | Paid |

---

## Pricing Tiers (Admin-Configurable)

Each content category can be configured with one or more pricing tiers. Admins create and edit these from the dashboard.

| Tier | Description |
|---|---|
| `base_upload` | Base cost to have the content created and published |
| `social_boost` | Additional fee to have the content promoted on Facebook & Instagram |
| `featured_week` | Fee to have the content featured/alaune on the homepage for one week |
| `extended_featured` | Fee for extended homepage placement (e.g. 2 weeks) |

> **Note:** Tiers are additive. There are no combo discounts. The total is the sum of all selected tiers.

---

## Sample Pricing (Kinshasa Market)

All prices in USD. CDF equivalent ≈ USD × 1,700.

### Articles

| Category | Base Upload | Social Boost | Featured (1 week) |
|---|---|---|---|
| Chronique Sale / Buzz | Free | — | — |
| Artist Profile | $25 | +$15 | +$20 |
| Album / Single Review | $20 | +$10 | +$20 |
| À la Une / Alaune | $50 | +$15 | Included |
| Sponsored Spotlight | $50 | +$25 | +$30 |
| Lyrics Page | $15 | +$10 | — |

### Videos

| Show | Base Upload | Social Boost |
|---|---|---|
| 116 Music Video | $40 | +$20 |
| 116 Interview | $80 | +$20 |
| 116 FlexBeat | $150 | +$20 |
| 116 Le Focus | $200 | +$20 |
| 116 BTS | $60 | +$20 |
| 116 Podcast | $80 | +$20 |
| 116 Reality | Custom (from $250) | +$30 |
| 116 Discovery | $180 | +$30 |
| 116 Documentary | $400 | +$30 |
| 116 Behind the Lyrics | $120 | +$20 |
| 116 Lyric Video | $40 | +$20 |
| 116 Studio | $80 | +$20 |

> **Social Boost** is a manual process: the admin checks a flag on the content record and runs a Facebook/Instagram campaign manually. There is no API integration — it is a checkbox, not automation.

---

## Database Structure (Logical)

```
content.content_types           (Article, Video)
        │
        ▼
content.categories              (Artist Profile, Music Video, Documentary, etc.)
        │
        ├── is_free             (true for Chronique Sale, false for paid categories)
        ├── content_type_id     (FK → content_types)
        │
        ▼
content.pricing_tiers           (base_upload, social_boost, featured_week, extended_featured)
        │
content.category_pricing        (joins category + tier with a price)
        │                       e.g. Artist Profile + base_upload = $25
        ▼
content.articles / content.videos   (actual content records)
        │
        ├── category_id
        ├── selected_tiers      (JSONB: which pricing tiers were chosen)
        ├── total_price_usd     (auto-calculated from selected tiers)
        ├── social_boost        (boolean: flag for manual social media promotion)
        ├── is_promoted         (boolean)
        ├── promoted_until      (date: when promoted placement expires)
        │
        ▼
content.content_payments        (payment tracking per content item)
        │
        ├── amount_usd
        ├── payment_method      (bank_transfer, mobile_money, cash, other)
        ├── payment_proof_url   (Cloudinary upload of the receipt photo)
        ├── status              (pending, verified, rejected)
        ├── verified_by         (admin user id)
        ├── verified_at
        └── receipt_url         (generated downloadable receipt)
```

---

## Admin Workflow: Setting Up Pricing

This is a one-time setup done by the Super Admin, then updated as needed.

```
Step 1: Create Content Types
─────────────────────────────
Super Admin creates: "Article", "Video"

Step 2: Create Pricing Tiers
─────────────────────────────
Super Admin creates tiers:
  - base_upload      → "Base creation and publishing fee"
  - social_boost     → "Facebook & Instagram promotion"
  - featured_week    → "Homepage featured placement for 7 days"
  - extended_featured → "Homepage featured placement for 14 days"

Step 3: Create Categories
─────────────────────────────
Super Admin creates:
  - Artist Profile    (content_type = Article, is_free = false)
  - Chronique Sale    (content_type = Article, is_free = true)
  - Music Video       (content_type = Video, is_free = false)
  - Documentary       (content_type = Video, is_free = false)
  ... etc.

Step 4: Configure Category Pricing
─────────────────────────────────────
For each paid category, Super Admin sets prices per tier:
  - Artist Profile + base_upload   = $25
  - Artist Profile + social_boost  = $15
  - Artist Profile + featured_week = $20
  - Music Video    + base_upload   = $40
  - Music Video    + social_boost  = $20
  ... etc.
```

---

## Admin Workflow: Creating Content

### Articles (two-step form)

```
Step 1 — Identifiers
───────────────────────────────────
  - Admin selects: Category (e.g. Artist Profile)
  - Admin fills: Title, Slug (auto-generated from title, editable)
  - For paid content: selects Customer + Order Item
  - Admin clicks "Save Draft"
    → POST /api/v1/admin/articles
    → article created: body='', headline='', author_id=JWT, status=Draft
    → returns { articleId }

Step 2 — Content + Images
───────────────────────────────────
  - Admin writes Headline (100–300 chars)
  - Admin writes Body in rich-text editor
    → each image inserted fires POST /api/v1/admin/articles/{id}/images immediately
    → editor replaces blob/base64 with returned Cloudinary URL in-place
  - Admin optionally uploads Cover Image (same endpoint, imageType=cover)
  - Admin clicks "Submit"
    → PUT /api/v1/admin/articles/{id}  (saves headline, body, coverImageUrl — also used later
       to edit a Draft or Rejected article before resubmitting)
    → PATCH /api/v1/admin/articles/{id}/submit  (transitions status)

Status after Submit:
  - Free content  → PendingReview  (editorial team reviews immediately)
  - Paid content  → PendingPayment (waiting for customer to pay)

Note: social_boost, is_promoted, promoted_until are NEVER set through the article form.
They are stamped automatically by the system when payment is verified in the Commerce flow.
```

### Videos (single-step form + separate YouTube attach)

```
Step 1: Admin creates video record
───────────────────────────────────
  - Fills in title, slug, description, category
  - For paid content: selects Customer + Order Item
  - Optionally sets ShootingScheduledAt (for pre-booked productions)
  - POST /api/v1/admin/videos → video created, status=Draft

Step 2: Submit → Approve → Attach YouTube → Publish
───────────────────────────────────
  - PATCH /submit  → PendingPayment or PendingReview
  - PATCH /approve → Approved (after editorial sign-off)
  - PATCH /youtube → attaches YoutubeVideoId (required gate before publish)
  - PATCH /publish → Published
```

---

## Payment Workflow

```
Step 4: Customer sends payment proof
──────────────────────────────────────
  - Customer pays via bank transfer or mobile money (MoMo)
  - Customer sends photo of receipt (WhatsApp, email, or portal upload)
  - Admin uploads the receipt photo → stored in content.content_payments
    payment_status = PENDING

Step 5: Admin verifies payment
──────────────────────────────────────
  - Admin opens payment record, views the uploaded proof
  - Admin clicks "Mark as Verified"
    payment_status = VERIFIED
    verified_by    = admin user id
    verified_at    = now()

Step 6: System generates receipt
──────────────────────────────────────
  - A downloadable PDF receipt is generated automatically
  - receipt_url stored in the payment record
  - Customer can download their receipt

Step 7: Content activated
──────────────────────────────────────
  - Content status → APPROVED (or PENDING_REVIEW for Super Admin approval)
  - If promoted/alaune: is_promoted = true, promoted_until = now() + 7 days
  - If social_boost: social_boost flag = true (admin handles campaign manually)
```

---

## Content Approval Flow

All content requires approval before publishing. The full status lifecycle:

```
DRAFT
  │
  ▼
PENDING_PAYMENT   ← waiting for customer to pay
  │
  ▼ (payment verified)
PENDING_REVIEW    ← waiting for Super Admin or Moderator approval
  │
  ├─ Rejected → REJECTED (with reason)
  │
  ▼ (approved)
PUBLISHED
```

**Video-specific note:** A video cannot be set to PUBLISHED until a YouTube link is attached. The video goes through the payment and approval flow first, then the YouTube link is added after shooting and editing are complete.

---

## Packages (Optional — Bundled Deals)

For clients who want multiple content pieces, the Super Admin can configure **packages** that bundle several content items together.

A package has:
- A name (e.g. "Artist Starter Pack", "Label Pro Pack")
- Required content items (e.g. 1 × Artist Profile article + 1 × 116 Interview video)
- Optional bonus slots (client picks from a list of content types)
- A flat package price (instead of individual item pricing)

> Packages are optional — clients can also purchase individual content items without a package.

---

## Free Content Rules

Free categories (Chronique Sale, Buzz de la Semaine) have no restrictions:
- No account required to trigger creation (admin creates them internally)
- No payment flow triggered
- Published directly after Super Admin approval
- Never get Featured or Alaune placement (those are reserved for paid content)
- Purpose: drive organic traffic and engagement on the platform
