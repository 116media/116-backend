# Ads Module: Schema, Relationships & Workflow

## Overview

The ads module manages the complete advertising lifecycle on the 116 platform. It supports two ad formats:

- **Banner Ads**: Static/animated image ads placed in specific page zones (sidebar, between articles, homepage hero)
- **Popup Story Ads**: Instagram-style story sequences that appear after a delay on homepage, article pages, and video pages

---

## Table Relationships

```
ads.pricing_packages
        │
        │ (a campaign is purchased under a package)
        ▼
ads.campaigns ──────────────────────────────────────────────┐
        │                                                   │
        │ (a campaign contains many ads)                    │ (payment proof & receipt)
        ▼                                                   ▼
ads.advertisements                               ads.campaign_payments
        │
        ├─── ad_type_id ──────► ads.ad_types        (banner / popup_story)
        ├─── placement_id ────► ads.placements       (homepage_hero, sidebar, article_inline, etc.)
        │
        │ (each ad display = one impression)
        ▼
ads.impressions
        │
        │ (user clicks the ad = one click)
        ▼
ads.clicks
```

### ads.ad_types
Defines the format of the advertisement.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `name` | e.g. `banner`, `popup_story` |
| `description` | Human-readable description |
| `is_active` | Whether this type is currently offered |

### ads.placements
Defines where on the platform the ad can appear.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `name` | e.g. `homepage_hero`, `sidebar`, `article_inline`, `video_page` |
| `description` | Page location description |
| `is_active` | Whether this placement is available |

### ads.pricing_packages
Defines the tiers available for advertisers to purchase.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `name` | `free`, `starter`, `pro`, `premium` |
| `price_usd` | Package price |
| `duration_days` | How long the campaign runs |
| `max_ads` | Maximum number of advertisements per campaign |
| `allowed_placements` | JSONB list of allowed placement names |
| `allowed_ad_types` | JSONB list of allowed ad type names |
| `max_daily_impressions` | Cap on daily ad displays |
| `priority_level` | Higher priority = displayed before lower-tier ads |
| `is_active` | Whether the package is currently offered |

### ads.campaigns
The top-level container for an advertiser's campaign.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `advertiser_name` | Name of the company or individual |
| `package_id` | FK → `ads.pricing_packages` |
| `title` | Campaign name (e.g. "Kinshasa Music Awards 2025") |
| `budget_usd` | Total allocated budget |
| `starts_at` | Campaign start date |
| `ends_at` | Campaign end date |
| `status` | `draft`, `pending_payment`, `active`, `paused`, `completed`, `cancelled` |
| `priority_level` | Overrides package default if needed |

### ads.advertisements
Individual ad units within a campaign.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `campaign_id` | FK → `ads.campaigns` |
| `ad_type_id` | FK → `ads.ad_types` |
| `placement_id` | FK → `ads.placements` |
| `title` | Internal label for this ad |
| `media_url` | Image or video URL (Cloudinary) |
| `target_url` | Where the user goes when they click (external link) |
| `story_order` | For popup stories: display order within the sequence |
| `story_duration_seconds` | How long each story slide displays |
| `is_active` | Whether this specific ad is running |

### ads.campaign_payments
Tracks manual payment for each campaign.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `campaign_id` | FK → `ads.campaigns` |
| `amount_usd` | Amount paid |
| `payment_method` | `bank_transfer`, `mobile_money`, `cash`, `other` |
| `payment_proof_url` | Uploaded receipt file (Cloudinary) |
| `status` | `pending`, `verified`, `rejected` |
| `verified_by` | FK → admin user who verified |
| `verified_at` | Timestamp of verification |
| `receipt_url` | Generated receipt available for download |
| `notes` | Admin notes |

### ads.impressions
One record per ad display.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `advertisement_id` | FK → `ads.advertisements` |
| `user_id` | FK → user (nullable for anonymous) |
| `ip_address` | Visitor IP |
| `user_agent` | Browser/device info |
| `displayed_at` | Timestamp |

### ads.clicks
One record per user click on an ad.

| Column | Description |
|---|---|
| `id` | UUID primary key |
| `advertisement_id` | FK → `ads.advertisements` |
| `impression_id` | FK → `ads.impressions` (which display triggered the click) |
| `user_id` | FK → user (nullable) |
| `ip_address` | Visitor IP |
| `clicked_at` | Timestamp |

---

## Pricing Packages

Adapted to the economic reality of Kinshasa and the DRC market.

| Package | Price (USD) | CDF Approx | Duration | Max Ads | Placements | Priority |
|---|---|---|---|---|---|---|
| **Free** | $0 | 0 | 7 days | 1 | Sidebar only | 1 |
| **Starter** | $25 | ~42,500 | 30 days | 3 | Sidebar + Article pages | 3 |
| **Pro** | $80 | ~136,000 | 30 days | 5 | Sidebar + Articles + Video pages + Story ads | 6 |
| **Premium** | $250 | ~425,000 | 30 days | 10 | ALL placements incl. homepage hero + popup stories | 10 |

### Package Rules

**Free**
- Max 100 impressions/day
- Sidebar placement only
- Banner ads only (no popup, no story)
- Intended for small local businesses to test the platform

**Starter**
- Max 500 impressions/day
- Sidebar and article inline placements
- Banner ads only
- Ideal for emerging artists, small restaurants, local service businesses

**Pro** _(sweet spot)_
- Max 2,000 impressions/day
- Sidebar, article inline, and video page placements
- Banner ads + story ads
- Behavioral targeting available
- Ideal for music labels, established businesses, event organizers

**Premium**
- Max 10,000 impressions/day
- ALL placements including homepage hero and popup stories
- All ad types (banner, story, popup)
- Dedicated support
- Ideal for major brands, international companies, large festivals

---

## Display Logic

When a user visits a page, the system selects which ads to display using:

1. **Active campaigns only**: `starts_at <= now() <= ends_at` and `status = 'active'`
2. **Verified payment**: Only campaigns with a verified payment record are activated
3. **Placement match**: Ads are filtered by the current page's placement zone
4. **Priority order**: Higher priority campaigns (Premium > Pro > Starter > Free) display first
5. **Budget & impression cap**: Campaigns that have hit their daily impression limit are skipped
6. **Fair rotation**: Among campaigns of equal priority, ads rotate evenly

---

## Complete Workflow

### Phase 1: Advertiser Onboarding

```
1. Advertiser contacts 116 (email, WhatsApp, etc.)
2. Admin creates campaign record in dashboard
3. Admin selects pricing package for the campaign
4. Admin configures:
   - Campaign title, advertiser name, dates
   - Budget
5. Campaign saved as DRAFT
```

### Phase 2: Ad Creation

```
6. Admin creates advertisements within the campaign:
   - Selects ad type (banner / popup story)
   - Selects placement (sidebar / homepage_hero / article_inline / etc.)
   - Uploads media (image or short video) to Cloudinary
   - Sets target URL (external link)
   - For story ads: sets story_order and story_duration_seconds
7. System validates ad count against package limit
8. Campaign status → PENDING_PAYMENT
```

### Phase 3: Payment

```
9.  Advertiser receives payment instructions (bank / MoMo)
10. Advertiser sends payment proof (photo of receipt)
11. Advertiser or admin uploads proof → stored in ads.campaign_payments
    payment status = PENDING
```

### Phase 4: Admin Payment Verification

```
12. Admin reviews uploaded proof of payment
13. Admin marks payment as VERIFIED
    - payment status = VERIFIED
    - verified_by = admin user id
    - verified_at = now()
14. System generates downloadable receipt (PDF)
    - receipt_url stored in ads.campaign_payments
15. Campaign status → ACTIVE
```

### Phase 5: Campaign Live

```
16. Ads are now eligible to display on the platform
17. Every display → record in ads.impressions
18. Every click → record in ads.clicks
19. Daily impression counts tracked and capped per package limits
20. Campaign auto-deactivates when ends_at is reached
```

### Phase 6: Reporting

```
21. Admin can view per-campaign analytics:
    - Total impressions
    - Total clicks
    - CTR (clicks ÷ impressions × 100)
    - Performance by placement
    - Top-performing ads
```

---

## Popup Story Ad Behaviour

A popup story is a sequence of ads displayed like Instagram Stories:

1. User visits homepage (or article/video page)
2. After a configurable delay (e.g. 5 seconds), the story popup appears
3. First story slide (`story_order = 1`) is shown for `story_duration_seconds`
4. Automatically advances to next slide
5. User can tap/click to advance or close
6. Impression recorded per slide shown
7. Click recorded if user taps the CTA on any slide

All slides in the sequence belong to the same campaign (and share the same `campaign_id`).

---

## Business Logic Rules

- A **Free** campaign cannot use homepage hero or popup placements.
- A **Starter** campaign cannot use popup story or story ad types.
- If a campaign exceeds its daily impression cap, it stops displaying until the next day.
- Only campaigns with `payment_status = VERIFIED` can be set to `ACTIVE`.
- Hard-deleting a campaign cascades and removes all its advertisements, impressions, and clicks.
- Core system placements (homepage hero) are reserved for **Premium** only.