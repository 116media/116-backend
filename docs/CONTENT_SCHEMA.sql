-- =============================================================================
-- 116 Platform — Content Module SQL Schema
-- Schema:   content
-- Database: PostgreSQL 15+
--
-- DESIGN PRINCIPLES
-- ─────────────────
-- 1. 3NF normalisation — no derived or repeated data
-- 2. Admin-configurable pricing — zero hardcoded prices anywhere
-- 3. Order pattern for commerce — every purchase (individual or package)
--    flows through content_orders → content_order_items → content_item_tiers
--    → content_payments. One payment per order, one receipt per customer.
--    This removes the polymorphic discriminator from payment tables (it only
--    appears once, in content_order_items, where it belongs).
-- 4. Editorial and commerce are separated — articles/videos carry only their
--    editorial state. All purchase decisions live on the order tables.
-- 5. Cross-schema references to identity.users are intentionally FK-free so
--    the content schema can be extracted as a microservice without rewiring.
-- 6. Denormalized counters (like_count, rating_average, etc.) are acceptable
--    on read-heavy columns. The application maintains them.
--
-- FLOW SUMMARY
-- ─────────────
-- Individual purchase:
--   customer → order (package_id = NULL)
--     └── order_item (category_id, content_kind, promotion_level_id?)
--           └── content_item_tiers (base_upload $40, social_boost $20)
--   → one content_payment (amount = sum of tiers + promo price)
--   → one receipt PDF
--   → admin creates article/video → article.order_item_id = order_item.id
--
-- Package purchase:
--   customer → order (package_id = "Artist Starter Pack")
--     ├── order_item 1 (category: Artist Profile, kind: article)
--     ├── order_item 2 (category: 116 Interview,  kind: video)
--     └── order_item 3 (category: 116 FlexBeat,   kind: video, is_bonus: true)
--   → one content_payment (amount = package.flat_price_usd)
--   → one receipt PDF
--   → admin creates each content item → each points back via order_item_id
-- =============================================================================


-- =============================================================================
-- 1. SCHEMA
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS content;


-- =============================================================================
-- 2. ENUM TYPES
-- =============================================================================

-- Editorial lifecycle for articles and videos
-- Free content  : draft → pending_review → approved → published
-- Paid content  : draft → pending_payment → pending_review → approved → published
-- Video gate    : approved → published requires youtube_video_id to be non-null
CREATE TYPE content.content_status AS ENUM (
    'draft',
    'pending_payment',
    'pending_review',
    'approved',
    'published',
    'rejected',
    'archived'
);

-- Commerce lifecycle for an order (independent from editorial status)
-- draft          : order is being configured by admin
-- pending_payment: order sent to customer, waiting for receipt upload
-- paid           : payment verified → triggers content items to pending_review
-- cancelled      : order voided before payment
CREATE TYPE content.order_status AS ENUM (
    'draft',
    'pending_payment',
    'paid',
    'cancelled'
);

CREATE TYPE content.payment_status AS ENUM (
    'pending',
    'verified',
    'rejected'
);

CREATE TYPE content.payment_method AS ENUM (
    'bank_transfer',
    'mobile_money',
    'cash',
    'other'
);

-- Used in content_order_items to record what kind of content was commissioned.
-- Appears once — not duplicated across payment/tier tables.
CREATE TYPE content.content_kind AS ENUM (
    'article',
    'video'
);


-- =============================================================================
-- 3. REFERENCE / LOOKUP TABLES
-- Admin-managed. Rarely change after initial setup.
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- content_types
-- Top-level content format. Admin creates these once.
--
-- Example records:
--   ('...uuid...', 'Article', true)
--   ('...uuid...', 'Video',   true)
--   ('...uuid...', 'Short',   true)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.content_types (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name       VARCHAR(30) NOT NULL,
    is_active  BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT uq_content_types_name UNIQUE (name)
);

COMMENT ON TABLE  content.content_types
    IS 'Top-level content format: Article, Video, Short. Admin-managed at setup time.';


-- ─────────────────────────────────────────────────────────────────────────────
-- pricing_tiers
-- Add-on service fees. Only creation/distribution costs belong here.
-- Homepage placement upgrades live in promotion_levels (separate concern).
--
-- Example records:
--   ('...uuid...', 'base_upload',  'Content creation and publishing fee', true)
--   ('...uuid...', 'social_boost', 'Manual Facebook/Instagram promotion', true)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.pricing_tiers (
    id          UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name        VARCHAR(40)  NOT NULL,
    description VARCHAR(200),
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ,
    created_by  TEXT,
    updated_at  TIMESTAMPTZ,
    updated_by  TEXT,

    CONSTRAINT uq_pricing_tiers_name UNIQUE (name)
);

COMMENT ON TABLE  content.pricing_tiers
    IS 'Add-on service fees per content item. Only creation costs — NOT placement upgrades.';
COMMENT ON COLUMN content.pricing_tiers.name
    IS 'Slug-style name used in code: e.g. "base_upload", "social_boost".';


-- ─────────────────────────────────────────────────────────────────────────────
-- promotion_levels
-- Homepage placement upgrades. Completely separate from pricing_tiers
-- because they are promotion decisions, not content creation costs.
-- The price here is added to the order total alongside the tier prices.
--
-- Example records:
--   ('...uuid...', 'Featured',          7,  20.00, true)  ← 7 days on homepage
--   ('...uuid...', 'À la Une',          7,  50.00, true)  ← top spotlight slot
--   ('...uuid...', 'Extended Featured', 14, 35.00, true)  ← two weeks
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.promotion_levels (
    id            UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name          VARCHAR(40)   NOT NULL,
    duration_days INT           NOT NULL,
    price_usd     NUMERIC(10,2) NOT NULL,
    is_active     BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMPTZ,
    created_by    TEXT,
    updated_at    TIMESTAMPTZ,
    updated_by    TEXT,

    CONSTRAINT uq_promotion_levels_name     UNIQUE (name),
    CONSTRAINT chk_promotion_levels_duration CHECK (duration_days > 0),
    CONSTRAINT chk_promotion_levels_price    CHECK  (price_usd >= 0)
);

COMMENT ON TABLE  content.promotion_levels
    IS 'Homepage placement upgrades: Featured, À la Une, Extended. Priced separately from creation fees.';
COMMENT ON COLUMN content.promotion_levels.duration_days
    IS 'How long the content stays on the promoted slot. Used to compute articles.promoted_until at payment verification.';


-- ─────────────────────────────────────────────────────────────────────────────
-- tags
-- Shared across articles and videos.
--
-- Example records:
--   ('...uuid...', 'Afrobeats',    'afrobeats')
--   ('...uuid...', 'Kinshasa',     'kinshasa')
--   ('...uuid...', 'Gaz Mawete',   'gaz-mawete')
--   ('...uuid...', 'Rhumba',       'rhumba')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.tags (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name       VARCHAR(50) NOT NULL,
    slug       VARCHAR(60) NOT NULL,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT uq_tags_name UNIQUE (name),
    CONSTRAINT uq_tags_slug UNIQUE (slug)
);


-- =============================================================================
-- 4. ADMIN-MANAGED CONFIGURATION TABLES
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- categories
-- One category belongs to one content type.
-- is_free = TRUE skips the entire payment flow.
-- is_free = FALSE requires a verified order before status moves to pending_review.
--
-- Example records:
--   content_type = Article:
--     ('...', article_type_id, 'Chronique Sale',      'chronique-sale',      ..., is_free=true)
--     ('...', article_type_id, 'Artist Profile',      'artist-profile',      ..., is_free=false)
--     ('...', article_type_id, 'Album Review',        'album-review',        ..., is_free=false)
--     ('...', article_type_id, 'Sponsored Spotlight', 'sponsored-spotlight', ..., is_free=false)
--
--   content_type = Video:
--     ('...', video_type_id, '116 Music Video',    '116-music-video',    ..., is_free=false)
--     ('...', video_type_id, '116 Interview',      '116-interview',      ..., is_free=false)
--     ('...', video_type_id, '116 Le Focus',       '116-le-focus',       ..., is_free=false)
--     ('...', video_type_id, '116 FlexBeat',       '116-flexbeat',       ..., is_free=false)
--     ('...', video_type_id, '116 Documentary',    '116-documentary',    ..., is_free=false)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.categories (
    id              UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    content_type_id UUID         NOT NULL REFERENCES content.content_types (id) ON DELETE RESTRICT,
    name            VARCHAR(60)  NOT NULL,
    slug            VARCHAR(80)  NOT NULL,
    description     VARCHAR(300),
    is_free         BOOLEAN      NOT NULL DEFAULT FALSE,
    is_active       BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ,
    created_by      TEXT,
    updated_at      TIMESTAMPTZ,
    updated_by      TEXT,

    CONSTRAINT uq_categories_slug UNIQUE (slug)
);

COMMENT ON TABLE  content.categories
    IS 'Content categories per type. is_free=TRUE skips payment flow entirely.';
COMMENT ON COLUMN content.categories.is_free
    IS 'TRUE for Chronique Sale, Buzz. Free content: no order, no payment, never gets featured placement.';


-- ─────────────────────────────────────────────────────────────────────────────
-- category_pricing
-- Price per (category, pricing_tier) pair.
-- No row = that tier is not available for that category.
-- price_usd here is the CURRENT price. Past orders are protected by
-- content_item_tiers.price_snapshot_usd which is frozen at order creation.
--
-- Example records (for "Artist Profile" category):
--   (artist_profile_id, base_upload_id,  25.00)  ← base article writing fee
--   (artist_profile_id, social_boost_id, 15.00)  ← +$15 for FB/IG promotion
--
-- Example records (for "116 Music Video" category):
--   (music_video_id, base_upload_id,  40.00)  ← base video upload fee
--   (music_video_id, social_boost_id, 20.00)  ← +$20 for social promotion
--
-- Example records (for "116 Le Focus" category):
--   (le_focus_id, base_upload_id,  200.00)  ← premium show: $200 base
--   (le_focus_id, social_boost_id,  20.00)  ← social boost available
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.category_pricing (
    id              UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    category_id     UUID          NOT NULL REFERENCES content.categories (id) ON DELETE CASCADE,
    pricing_tier_id UUID          NOT NULL REFERENCES content.pricing_tiers (id) ON DELETE RESTRICT,
    price_usd       NUMERIC(10,2) NOT NULL,
    created_at      TIMESTAMPTZ,
    created_by      TEXT,
    updated_at      TIMESTAMPTZ,
    updated_by      TEXT,

    CONSTRAINT uq_category_pricing        UNIQUE (category_id, pricing_tier_id),
    CONSTRAINT chk_category_pricing_price CHECK  (price_usd >= 0)
);

COMMENT ON TABLE  content.category_pricing
    IS 'Current price per (category, tier) pair. Editing price_usd does NOT affect past orders.';
COMMENT ON COLUMN content.category_pricing.price_usd
    IS 'Live price. Historical price is preserved in content_item_tiers.price_snapshot_usd.';


-- ─────────────────────────────────────────────────────────────────────────────
-- customers
-- B2B clients: artists, labels, brands who commission paid content.
-- Completely separate from identity.users (platform visitors who consume content).
--
-- Example records:
--   ('...', 'Gaz Mawete',          'gaz@teamgaz.cd',       '+243 81 xxx xxxx', NULL,              '...')
--   ('...', 'Team Paiya Records',  'contact@teampaiya.cd', '+243 99 xxx xxxx', 'Team Paiya',      '...')
--   ('...', 'Wenge Musica BCBG',   'wenge@bcbg.cd',        '+243 82 xxx xxxx', 'Wenge Musica',    '...')
--   ('...', 'Bralima S.A.',        'marketing@bralima.cd', '+243 81 xxx xxxx', 'Bralima Brewery', '...')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.customers (
    id         UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    full_name  VARCHAR(100) NOT NULL,
    email      VARCHAR(200) NOT NULL,
    phone      VARCHAR(30),
    company    VARCHAR(100),
    notes      VARCHAR(500),
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT uq_customers_email UNIQUE (email)
);

COMMENT ON TABLE  content.customers
    IS 'B2B clients commissioning paid content. NOT the same as identity.users (B2C platform visitors).';
COMMENT ON COLUMN content.customers.company
    IS 'NULL for solo artists. Set for labels and corporate clients.';


-- ─────────────────────────────────────────────────────────────────────────────
-- packages
-- Pre-configured bundles for structured artists / labels.
-- Individual à-la-carte purchase (order.package_id = NULL) is always available.
-- A package has a flat price that overrides per-item tier pricing.
--
-- Example records:
--   ('...', 'Artist Starter Pack', 'For emerging artists: 1 article + 1 video', 150.00, true)
--   ('...', 'Label Pro Pack',      '3 videos + 2 articles, social boost on all', 400.00, true)
--   ('...', 'Full Press Kit',      '1 Focus + 1 Interview + profile article',    350.00, true)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.packages (
    id             UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name           VARCHAR(100)  NOT NULL,
    description    VARCHAR(500),
    flat_price_usd NUMERIC(10,2) NOT NULL,
    is_active      BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMPTZ,
    created_by     TEXT,
    updated_at     TIMESTAMPTZ,
    updated_by     TEXT,

    CONSTRAINT chk_packages_price CHECK (flat_price_usd >= 0)
);

COMMENT ON TABLE  content.packages
    IS 'Pre-configured bundles for structured artists/labels. Individual purchase always available (order.package_id = NULL).';
COMMENT ON COLUMN content.packages.flat_price_usd
    IS 'Flat rate for the entire bundle, overrides per-item tier pricing when package_id is set on an order.';


-- ─────────────────────────────────────────────────────────────────────────────
-- package_slots
-- Defines what content is included in a package.
-- category_id = NULL means the client can pick any category for that slot.
--
-- Example: "Artist Starter Pack" slots
--   (starter_pack_id, artist_profile_cat_id, is_required=true,  quantity=1)  ← must-have article
--   (starter_pack_id, music_video_cat_id,    is_required=true,  quantity=1)  ← must-have video
--   (starter_pack_id, NULL,                  is_required=false, quantity=1)  ← bonus: pick any
--
-- Example: "Label Pro Pack" slots
--   (pro_pack_id, interview_cat_id, is_required=true, quantity=2)  ← 2 interviews
--   (pro_pack_id, article_cat_id,   is_required=true, quantity=2)  ← 2 articles
--   (pro_pack_id, NULL,             is_required=false, quantity=1) ← 1 bonus any
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.package_slots (
    id          UUID    NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    package_id  UUID    NOT NULL REFERENCES content.packages (id) ON DELETE CASCADE,
    category_id UUID    REFERENCES content.categories (id) ON DELETE RESTRICT,
    is_required BOOLEAN NOT NULL DEFAULT TRUE,
    quantity    INT     NOT NULL DEFAULT 1,
    created_at  TIMESTAMPTZ,
    created_by  TEXT,
    updated_at  TIMESTAMPTZ,
    updated_by  TEXT,

    CONSTRAINT chk_package_slots_quantity CHECK (quantity > 0)
);

COMMENT ON COLUMN content.package_slots.category_id
    IS 'NULL = open slot: client picks any category. Non-null = fixed content type required.';
COMMENT ON COLUMN content.package_slots.is_required
    IS 'TRUE = must-have slot. FALSE = bonus/optional (client may or may not fill it).';


-- =============================================================================
-- 5. MAIN CONTENT TABLES
-- Pure editorial tables. No package_id, no promotion_level_id here.
-- Those are commerce decisions that live on content_order_items.
-- articles.is_promoted / promoted_until are the result of a verified order —
-- computed by the app and stamped here for fast querying.
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- articles
-- customer_id: NULL for free/editorial content (Chronique Sale, Buzz, etc.)
-- social_boost: operational flag stamped by app when order is verified
-- is_promoted / promoted_until: stamped by app on payment verification
--   formula: promoted_until = payment.verified_at + promotion_level.duration_days
--
-- Example records:
--   Free article (no customer, no order):
--     id: 'a1b2...', customer_id: NULL, order_item_id: NULL,
--     category: 'Chronique Sale', title: 'Le scandale de la semaine à Kin',
--     status: 'published', is_promoted: false
--
--   Paid article — individual purchase (Gaz Mawete's Artist Profile):
--     id: 'c3d4...', customer_id: 'gaz-mawete-uuid', order_item_id: 'i1i2...',
--     category: 'Artist Profile', title: 'Gaz Mawete: La voix de Bandal',
--     social_boost: true, is_promoted: true,
--     promoted_until: '2026-03-08 00:00:00+00',
--     status: 'published'
--
--   Paid article — part of a Label Pro Pack (Team Paiya):
--     id: 'e5f6...', customer_id: 'team-paiya-uuid', order_item_id: 'i3i4...',
--     category: 'Sponsored Spotlight', title: 'Team Paiya: La nouvelle génération',
--     status: 'approved'
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.articles (
    id               UUID                   NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    customer_id      UUID                   REFERENCES content.customers (id) ON DELETE SET NULL,
    -- NULL for free/editorial content (Chronique Sale, Buzz, etc.).
    -- FK to content.content_order_items is added below via ALTER TABLE (forward reference).
    order_item_id    UUID,
    category_id      UUID                   NOT NULL REFERENCES content.categories (id) ON DELETE RESTRICT,
    title            VARCHAR(200)           NOT NULL,
    slug             VARCHAR(220)           NOT NULL,
    -- Short teaser / aperçu shown in article cards and SEO previews. Min 100, max 300 chars.
    -- Enforced at application level (validator). DB stores empty string for new drafts.
    headline         VARCHAR(300)           NOT NULL DEFAULT '',
    body             TEXT                   NOT NULL DEFAULT '',
    cover_image_url  VARCHAR(500),
    -- Identity user UUID (from JWT). Stored as plain TEXT — no FK to identity schema by design
    -- (cross-schema FK-free so content schema can be extracted as a microservice).
    -- Set automatically from the authenticated user's JWT claims. Never filled manually.
    author_id        TEXT                   NOT NULL,
    -- Stamped by app when order is verified (denormalized for fast queries)
    social_boost     BOOLEAN                NOT NULL DEFAULT FALSE,
    is_promoted      BOOLEAN                NOT NULL DEFAULT FALSE,
    promoted_until   TIMESTAMPTZ,
    status           content.content_status NOT NULL DEFAULT 'draft',
    rejection_reason VARCHAR(500),
    published_at     TIMESTAMPTZ,
    meta_title       VARCHAR(70),
    meta_description VARCHAR(160),
    -- Denormalized interaction counters maintained by application
    like_count       INT                    NOT NULL DEFAULT 0,
    comment_count    INT                    NOT NULL DEFAULT 0,
    share_count      INT                    NOT NULL DEFAULT 0,
    bookmark_count   INT                    NOT NULL DEFAULT 0,
    created_at       TIMESTAMPTZ,
    created_by       TEXT,
    updated_at       TIMESTAMPTZ,
    updated_by       TEXT,

    CONSTRAINT uq_articles_slug            UNIQUE (slug),
    CONSTRAINT chk_articles_like_count     CHECK  (like_count >= 0),
    CONSTRAINT chk_articles_comment_count  CHECK  (comment_count >= 0),
    CONSTRAINT chk_articles_share_count    CHECK  (share_count >= 0),
    CONSTRAINT chk_articles_bookmark_count CHECK  (bookmark_count >= 0)
);

COMMENT ON TABLE  content.articles
    IS 'Editorial articles. Commerce metadata (package, promotion) lives on content_order_items, not here.';
COMMENT ON COLUMN content.articles.customer_id
    IS 'NULL for free/editorial content (Chronique Sale, Buzz). No customer = no order = no payment flow.';
COMMENT ON COLUMN content.articles.social_boost
    IS 'Stamped by app when order is verified. Admin uses this as a manual Facebook/Instagram work flag.';
COMMENT ON COLUMN content.articles.is_promoted
    IS 'Stamped by app on payment verification. Enables homepage promoted slot display.';
COMMENT ON COLUMN content.articles.promoted_until
    IS 'Computed at payment verification: verified_at + promotion_level.duration_days.';
COMMENT ON COLUMN content.articles.headline
    IS 'Short teaser / aperçu (100–300 chars). Shown in article cards, feeds, and meta previews. Min length enforced at application level.';
COMMENT ON COLUMN content.articles.author_id
    IS 'Identity user UUID from JWT. No FK — cross-schema FK-free by design. Set automatically from the authenticated user claims. Never filled manually by admin.';
COMMENT ON COLUMN content.articles.rejection_reason
    IS 'Required when status transitions to rejected. Shown to the admin team.';


-- ─────────────────────────────────────────────────────────────────────────────
-- article_images
-- Tracks every image uploaded for an article (cover + body images).
-- Images are uploaded to Cloudinary BEFORE the article body is saved
-- (editor upload handler fires on image insert, before Save is clicked).
-- Because the article draft is created first (POST /admin/articles → returns articleId),
-- article_id is always known at upload time — no NULL staging needed.
--
-- image_type:
--   'cover' — the article cover image (cover_image_url field)
--   'body'  — an image embedded inside the rich-text body HTML
--
-- Cleanup mechanisms:
--   1. On PUT /admin/articles/{id}: handler diffs new body URLs against this table.
--      Images no longer in the body are deleted from Cloudinary + this table after commit.
--   2. Background job (every 24h): deletes records for abandoned drafts
--      (articles WHERE body = '' AND headline = '' AND created_at < now() - 24h).
--      Calls ICloudinaryService.DeleteImagesAsync(publicIds[]) before removing rows.
--   3. ON DELETE CASCADE: when an article is hard deleted, all image rows are removed.
--      The application handler must call Cloudinary deletion before hard deleting the article.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_images (
    id           UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    article_id   UUID         NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    -- Provider-agnostic storage identifier. Named storage_key (not cloudinary_public_id)
    -- so the column remains meaningful if the CDN provider changes (S3 key, Cloudinary
    -- public_id, Bunny path — all map cleanly to this field).
    storage_key  TEXT         NOT NULL,
    url          VARCHAR(500) NOT NULL,
    -- 'cover' | 'body'
    image_type   TEXT         NOT NULL DEFAULT 'body',
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT now()
);

COMMENT ON TABLE  content.article_images
    IS 'Tracks all media (cover + body images) belonging to an article. Used to detect and delete orphaned resources when the article body is updated, the draft is abandoned, or the article is hard deleted.';
COMMENT ON COLUMN content.article_images.storage_key
    IS 'Provider-agnostic resource identifier used to delete the asset from the media storage layer (e.g. Cloudinary public_id, S3 object key). Named storage_key to avoid coupling the schema to a specific CDN provider.';
COMMENT ON COLUMN content.article_images.image_type
    IS 'cover = the article cover image; body = image embedded in the rich-text body HTML.';


-- ─────────────────────────────────────────────────────────────────────────────
-- videos
-- youtube_video_id: required before status can transition to published.
-- shooting_scheduled_at: clients pay before filming — this is the planned date.
-- social_boost / is_promoted / promoted_until: same stamping pattern as articles.
-- author_id: same pattern as articles.author_id — JWT user UUID, no FK, provider-agnostic.
-- thumbnail_storage_key: provider-agnostic storage identifier for the thumbnail asset.
--   Named storage_key (not cloudinary_public_id) so it stays meaningful if the CDN
--   changes. Used to delete the old thumbnail when replaced or when video is hard deleted.
--
-- Example records:
--   116 Music Video — individual, Gaz Mawete:
--     id: 'v1v2...', customer_id: 'gaz-mawete-uuid', order_item_id: 'i5i6...',
--     category: '116 Music Video', title: 'Gaz Mawete — Bolingo Ya Vie (Official Video)',
--     youtube_video_id: 'dQw4w9WgXcQ', status: 'published',
--     rating_average: 4.7, rating_count: 312
--
--   116 Le Focus — part of Full Press Kit package (Fally Ipupa):
--     id: 'v3v4...', customer_id: 'fally-label-uuid', order_item_id: 'i7i8...',
--     category: '116 Le Focus', title: 'Fally Ipupa: 20 ans de carrière',
--     youtube_video_id: NULL,               ← not shot yet
--     shooting_scheduled_at: '2026-03-15',
--     status: 'pending_payment'
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.videos (
    id                    UUID                   NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    customer_id           UUID                   REFERENCES content.customers (id) ON DELETE SET NULL,
    -- NULL for free/editorial content.
    -- FK to content.content_order_items is added below via ALTER TABLE (forward reference).
    order_item_id         UUID,
    category_id           UUID                   NOT NULL REFERENCES content.categories (id) ON DELETE RESTRICT,
    -- Identity user UUID from JWT. No FK — cross-schema FK-free by design.
    -- Set automatically from authenticated user claims. Never filled manually.
    author_id             TEXT                   NOT NULL,
    title                 VARCHAR(200)           NOT NULL,
    slug                  VARCHAR(220)           NOT NULL,
    description           TEXT,
    thumbnail_url         VARCHAR(500),
    -- Provider-agnostic storage identifier for the thumbnail asset (see storage_key note
    -- on article_images). NULL until a thumbnail is uploaded.
    thumbnail_storage_key TEXT,
    youtube_video_id      VARCHAR(20),
    social_boost          BOOLEAN                NOT NULL DEFAULT FALSE,
    is_promoted           BOOLEAN                NOT NULL DEFAULT FALSE,
    promoted_until        TIMESTAMPTZ,
    has_lyrics            BOOLEAN                NOT NULL DEFAULT FALSE,
    status                content.content_status NOT NULL DEFAULT 'draft',
    rejection_reason      VARCHAR(500),
    shooting_scheduled_at TIMESTAMPTZ,
    published_at          TIMESTAMPTZ,
    meta_title            VARCHAR(70),
    meta_description      VARCHAR(160),
    -- Denormalized: recomputed by app after each video_rating insert/update
    rating_average        NUMERIC(3,2)           NOT NULL DEFAULT 0,
    rating_count          INT                    NOT NULL DEFAULT 0,
    share_count           INT                    NOT NULL DEFAULT 0,
    created_at            TIMESTAMPTZ,
    created_by            TEXT,
    updated_at            TIMESTAMPTZ,
    updated_by            TEXT,

    CONSTRAINT uq_videos_slug            UNIQUE (slug),
    CONSTRAINT chk_videos_rating_average CHECK  (rating_average >= 0 AND rating_average <= 5),
    CONSTRAINT chk_videos_rating_count   CHECK  (rating_count >= 0),
    CONSTRAINT chk_videos_share_count    CHECK  (share_count >= 0)
);

COMMENT ON TABLE  content.videos
    IS 'Video content. All videos link to YouTube for playback. Short clips are in short_videos.';
COMMENT ON COLUMN content.videos.author_id
    IS 'Identity user UUID from JWT claims. No FK — cross-schema FK-free by design (same pattern as articles.author_id). Set automatically by the handler, never filled manually.';
COMMENT ON COLUMN content.videos.thumbnail_storage_key
    IS 'Provider-agnostic storage identifier for the thumbnail (e.g. Cloudinary public_id, S3 key). Named storage_key to avoid coupling the schema to a specific CDN. Used to delete old thumbnail when replaced or when video is hard deleted.';
COMMENT ON COLUMN content.videos.youtube_video_id
    IS 'Required gate before status = published. Set by admin after upload to YouTube.';
COMMENT ON COLUMN content.videos.shooting_scheduled_at
    IS 'Clients pay before filming. Records planned shoot date for pre-production tracking.';
COMMENT ON COLUMN content.videos.has_lyrics
    IS 'TRUE when a content.lyrics record is linked. Enables lyrics link on the video page.';
COMMENT ON COLUMN content.videos.rating_average
    IS 'Denormalized. App recomputes: AVG(stars) from video_ratings after each insert/update.';


-- ─────────────────────────────────────────────────────────────────────────────
-- short_videos
-- Reels-style teasers. Stored on Cloudinary, NOT on YouTube.
-- video_id links back to the parent full video (teaser for a show).
-- Scandal/gossip shorts are standalone: video_id = NULL, has_full_video = FALSE.
--
-- Example records:
--   Teaser for a 116 Le Focus episode (links to parent video):
--     id: 's1s2...', title: '🔥 Fally parle de ses beef — 116 Le Focus',
--     video_url: 'https://res.cloudinary.com/.../short_fally_focus.mp4',
--     video_id: 'v3v4...',         ← parent full video
--     has_full_video: true,
--     view_count: 8432, like_count: 1203
--
--   Standalone gossip clip (no parent video):
--     id: 's3s4...', title: 'Le clash entre Ferre et Fally expliqué',
--     video_url: 'https://res.cloudinary.com/.../clash_gossip.mp4',
--     video_id: NULL,
--     has_full_video: false,
--     view_count: 21000
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.short_videos (
    id             UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    title          VARCHAR(200) NOT NULL,
    video_url      VARCHAR(500) NOT NULL,
    -- Provider-agnostic storage identifier for the video file asset.
    -- Named storage_key (not cloudinary_public_id) to avoid coupling the schema to a
    -- specific CDN provider (S3 key, Cloudinary public_id, etc. all map to this field).
    -- Required: the video file must always be trackable for deletion on hard delete.
    video_storage_key     TEXT         NOT NULL,
    thumbnail_url         VARCHAR(500),
    -- Provider-agnostic storage identifier for the thumbnail image.
    -- NULL until a thumbnail is uploaded. Same naming rationale as video_storage_key.
    thumbnail_storage_key TEXT,
    video_id       UUID         REFERENCES content.videos (id) ON DELETE SET NULL,
    has_full_video BOOLEAN      NOT NULL DEFAULT FALSE,
    is_active      BOOLEAN      NOT NULL DEFAULT TRUE,
    view_count     INT          NOT NULL DEFAULT 0,
    like_count     INT          NOT NULL DEFAULT 0,
    share_count    INT          NOT NULL DEFAULT 0,
    bookmark_count INT          NOT NULL DEFAULT 0,
    created_at     TIMESTAMPTZ,
    created_by     TEXT,
    updated_at     TIMESTAMPTZ,
    updated_by     TEXT,

    CONSTRAINT chk_short_videos_view_count     CHECK (view_count >= 0),
    CONSTRAINT chk_short_videos_like_count     CHECK (like_count >= 0),
    CONSTRAINT chk_short_videos_share_count    CHECK (share_count >= 0),
    CONSTRAINT chk_short_videos_bookmark_count CHECK (bookmark_count >= 0)
);

COMMENT ON COLUMN content.short_videos.video_url
    IS 'Cloudinary URL. Short clips are hosted here, not on YouTube.';
COMMENT ON COLUMN content.short_videos.video_id
    IS 'NULL for standalone gossip/scandal clips. Non-null = teaser for a full show episode.';
COMMENT ON COLUMN content.short_videos.has_full_video
    IS 'Controls "View Full Video" CTA button on the frontend. FALSE for gossip, TRUE for show teasers.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics
-- SEO-optimised pages. Can link to a video, an article, or be standalone.
-- Both FKs being NULL = standalone lyrics page (valid — like genius.com).
-- Both FKs being non-null at the same time = not allowed (check constraint).
--
-- Example records:
--   Linked to a music video:
--     id: 'l1l2...', video_id: 'v1v2...', article_id: NULL,
--     song_title: 'Bolingo Ya Vie', artist_name: 'Gaz Mawete',
--     language: 'ln', lyrics_text: 'Nakozwa yo, nakozwa yo...',
--     meta_title: 'Gaz Mawete — Bolingo Ya Vie Lyrics'
--
--   Standalone lyrics page (no video, no article):
--     id: 'l3l4...', video_id: NULL, article_id: NULL,
--     song_title: 'Oza Wapi', artist_name: 'Fally Ipupa',
--     language: 'ln', meta_keywords: 'fally ipupa, oza wapi, lyrics, paroles'
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics (
    id               UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    video_id         UUID         REFERENCES content.videos (id) ON DELETE CASCADE,
    article_id       UUID         REFERENCES content.articles (id) ON DELETE CASCADE,
    song_title       VARCHAR(200) NOT NULL,
    artist_name      VARCHAR(100) NOT NULL,
    lyrics_text      TEXT         NOT NULL,
    language         VARCHAR(5)   NOT NULL DEFAULT 'fr',
    meta_title       VARCHAR(70),
    meta_description VARCHAR(160),
    meta_keywords    VARCHAR(300),
    structured_data  JSONB,
    created_at       TIMESTAMPTZ,
    created_by       TEXT,
    updated_at       TIMESTAMPTZ,
    updated_by       TEXT,

    CONSTRAINT chk_lyrics_one_parent CHECK (
        NOT (video_id IS NOT NULL AND article_id IS NOT NULL)
    )
);

COMMENT ON TABLE  content.lyrics
    IS 'SEO-optimised lyrics pages. Can be linked to a video, an article, or standalone.';
COMMENT ON COLUMN content.lyrics.structured_data
    IS 'schema.org MusicRecording JSON-LD for Google rich snippets on lyrics pages.';
COMMENT ON COLUMN content.lyrics.language
    IS 'ISO 639-1 code. Common values for this platform: fr (French), ln (Lingala), en (English).';


-- =============================================================================
-- 6. COMMERCE TABLES — ORDER PATTERN
--
-- This is the core of the package vs individual purchase solution.
--
-- ONE order = ONE purchase event = ONE payment = ONE receipt.
-- Whether it covers 1 item (individual) or 5 items (package) does not matter.
-- The order is the unit of commerce.
--
-- content_orders           ← the purchase (individual or package)
--   └── content_order_items  ← one row per content item in the order
--         └── content_item_tiers ← which pricing tiers were selected per item
-- content_payments         ← one payment per order (one receipt)
--
-- The polymorphic content_kind discriminator appears ONLY in content_order_items.
-- It does NOT appear in content_item_tiers or content_payments — those simply
-- follow the order chain with clean FK references.
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- content_orders
-- The top-level purchase record.
-- package_id = NULL  → individual à-la-carte purchase
-- package_id = non-null → package deal; total = package.flat_price_usd
--
-- Example — Individual (Gaz Mawete buys Artist Profile + social boost):
--   id: 'o1o2...', customer_id: 'gaz-mawete-uuid',
--   package_id: NULL, status: 'paid',
--   total_amount_usd: 40.00   (25 base + 15 social)
--
-- Example — Package (Team Paiya buys Label Pro Pack):
--   id: 'o3o4...', customer_id: 'team-paiya-uuid',
--   package_id: 'label-pro-pack-uuid', status: 'paid',
--   total_amount_usd: 400.00  (flat package price)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.content_orders (
    id               UUID                  NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    customer_id      UUID                  NOT NULL REFERENCES content.customers (id) ON DELETE RESTRICT,
    package_id       UUID                  REFERENCES content.packages (id) ON DELETE RESTRICT,
    status           content.order_status  NOT NULL DEFAULT 'draft',
    -- Snapshot of total at order creation time.
    -- For individual: sum of item tiers + promotion prices.
    -- For package: package.flat_price_usd at time of order.
    total_amount_usd NUMERIC(10,2)         NOT NULL DEFAULT 0,
    created_at       TIMESTAMPTZ,
    created_by       TEXT,
    updated_at       TIMESTAMPTZ,
    updated_by       TEXT,

    CONSTRAINT chk_content_orders_total CHECK (total_amount_usd >= 0)
);

COMMENT ON TABLE  content.content_orders
    IS 'Top-level purchase unit. One order = one payment = one receipt. Covers both individual and package purchases.';
COMMENT ON COLUMN content.content_orders.package_id
    IS 'NULL = individual à-la-carte. Non-null = package deal; total = package.flat_price_usd.';
COMMENT ON COLUMN content.content_orders.total_amount_usd
    IS 'Snapshot at order creation. Individual: sum of tiers + promo prices. Package: flat_price_usd.';


-- ─────────────────────────────────────────────────────────────────────────────
-- content_order_items
-- One row per commissioned content item in the order.
-- Records what was purchased BEFORE the content exists.
-- content_kind + category_id identify what the admin needs to create.
-- promotion_level_id: the placement upgrade the customer selected (if any).
-- promo_price_snapshot_usd: promotion_levels.price_usd frozen at order creation.
-- social_boost: the customer's purchase decision. App stamps this onto the
--   article/video (article.social_boost / video.social_boost) once content is created.
--
-- The article or video created to fulfil this item points back via order_item_id.
--
-- Example — Individual order (Gaz Mawete, 1 article):
--   id: 'i1i2...', order_id: 'o1o2...',
--   content_kind: 'article', category_id: 'artist-profile-uuid',
--   promotion_level_id: 'featured-7d-uuid', promo_price_snapshot_usd: 20.00,
--   social_boost: true
--   → article created after order paid: articles.order_item_id = 'i1i2...'
--
-- Example — Package order (Team Paiya, 3 items):
--   row 1: order_id: 'o3o4...', content_kind: 'article', category_id: 'sponsored-spotlight-uuid', is_bonus: false
--   row 2: order_id: 'o3o4...', content_kind: 'video',   category_id: '116-interview-uuid',       is_bonus: false
--   row 3: order_id: 'o3o4...', content_kind: 'video',   category_id: '116-flexbeat-uuid',        is_bonus: true
--   → each content item created after order paid, each pointing back via order_item_id
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.content_order_items (
    id                       UUID                 NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    order_id                 UUID                 NOT NULL REFERENCES content.content_orders (id) ON DELETE CASCADE,
    -- What was commissioned: kind + category fully identify the deliverable.
    content_kind             content.content_kind NOT NULL,
    category_id              UUID                 NOT NULL REFERENCES content.categories (id) ON DELETE RESTRICT,
    promotion_level_id       UUID                 REFERENCES content.promotion_levels (id) ON DELETE RESTRICT,
    -- Frozen at order creation from promotion_levels.price_usd.
    -- NULL when no promotion level is selected.
    promo_price_snapshot_usd NUMERIC(10,2),
    -- The customer's purchase decision for this item.
    -- App stamps this onto article.social_boost / video.social_boost once content is created.
    social_boost             BOOLEAN              NOT NULL DEFAULT FALSE,
    -- TRUE for optional/bonus slots in a package that the client chose to fill.
    is_bonus                 BOOLEAN              NOT NULL DEFAULT FALSE,
    created_at               TIMESTAMPTZ,
    created_by               TEXT,
    updated_at               TIMESTAMPTZ,
    updated_by               TEXT,

    CONSTRAINT chk_order_items_promo_price CHECK (
        promo_price_snapshot_usd IS NULL OR promo_price_snapshot_usd >= 0
    )
);

COMMENT ON TABLE  content.content_order_items
    IS 'One row per commissioned item per order. Records what to create BEFORE the content exists. Content points back via order_item_id.';
COMMENT ON COLUMN content.content_order_items.content_kind
    IS 'Discriminator: article or video. Tells admin what type of content to create for this item.';
COMMENT ON COLUMN content.content_order_items.category_id
    IS 'The category commissioned. Together with content_kind, fully identifies what needs to be created.';
COMMENT ON COLUMN content.content_order_items.promotion_level_id
    IS 'Placement upgrade for this item. Priced separately from creation tiers.';
COMMENT ON COLUMN content.content_order_items.promo_price_snapshot_usd
    IS 'Frozen promotion price at order creation. NULL when no promotion level selected.';
COMMENT ON COLUMN content.content_order_items.social_boost
    IS 'Purchase decision. App stamps article.social_boost / video.social_boost when content is created after order is paid.';
COMMENT ON COLUMN content.content_order_items.is_bonus
    IS 'TRUE for optional slots in a package that the client chose to fill (non-required package_slots).';


-- ─────────────────────────────────────────────────────────────────────────────
-- Back-references: articles and videos point to the order item that
-- commissioned them. Added here via ALTER TABLE because content_order_items
-- is defined after articles/videos (forward reference).
-- NULL for free/editorial content (Chronique Sale, Buzz, short_videos, lyrics).
-- ─────────────────────────────────────────────────────────────────────────────
ALTER TABLE content.articles
    ADD CONSTRAINT fk_articles_order_item_id
    FOREIGN KEY (order_item_id)
    REFERENCES content.content_order_items (id)
    ON DELETE SET NULL;

ALTER TABLE content.videos
    ADD CONSTRAINT fk_videos_order_item_id
    FOREIGN KEY (order_item_id)
    REFERENCES content.content_order_items (id)
    ON DELETE SET NULL;

COMMENT ON COLUMN content.articles.order_item_id
    IS 'NULL for free/editorial content. Set by admin when creating the article after order payment is verified.';
COMMENT ON COLUMN content.videos.order_item_id
    IS 'NULL for free/editorial content. Set by admin when creating the video after order payment is verified.';


-- ─────────────────────────────────────────────────────────────────────────────
-- content_item_tiers
-- Pricing tier breakdown per order item. Clean FK — no discriminator needed.
-- price_snapshot_usd is frozen at order creation (immutable after that).
-- For package orders these are informational; the actual billed amount comes
-- from content_orders.total_amount_usd (the flat package price).
--
-- Example — Gaz Mawete individual order (Artist Profile + social boost):
--   row 1: order_item_id: 'i1i2...', pricing_tier: 'base_upload',  price_snapshot: 25.00
--   row 2: order_item_id: 'i1i2...', pricing_tier: 'social_boost', price_snapshot: 15.00
--   → subtotal for this item: 40.00 (+ 20.00 promo = 60.00 total for the order)
--
-- Example — Team Paiya package (tiers are informational breakdown):
--   row 1: order_item_id: 'i3i4...', pricing_tier: 'base_upload', price_snapshot: 25.00
--   row 2: order_item_id: 'i5i6...', pricing_tier: 'base_upload', price_snapshot: 80.00
--   → billing ignores these totals; order.total_amount_usd = 400.00 (flat package)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.content_item_tiers (
    id                 UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    order_item_id      UUID          NOT NULL REFERENCES content.content_order_items (id) ON DELETE CASCADE,
    pricing_tier_id    UUID          NOT NULL REFERENCES content.pricing_tiers (id) ON DELETE RESTRICT,
    price_snapshot_usd NUMERIC(10,2) NOT NULL,
    created_at         TIMESTAMPTZ,
    created_by         TEXT,
    updated_at         TIMESTAMPTZ,
    updated_by         TEXT,

    CONSTRAINT uq_item_tiers_per_order_item UNIQUE (order_item_id, pricing_tier_id),
    CONSTRAINT chk_item_tiers_price         CHECK  (price_snapshot_usd >= 0)
);

COMMENT ON TABLE  content.content_item_tiers
    IS 'Pricing tier breakdown per order item. No discriminator — clean FK chain. Price frozen at order creation.';
COMMENT ON COLUMN content.content_item_tiers.price_snapshot_usd
    IS 'Immutable. Frozen from category_pricing.price_usd at order creation. Admin price changes do not affect this.';
COMMENT ON COLUMN content.content_item_tiers.order_item_id
    IS 'Routes through content_order_items — no article_id/video_id columns needed here.';


-- ─────────────────────────────────────────────────────────────────────────────
-- content_payments
-- One payment per order. UNIQUE constraint on order_id enforces this.
-- amount_usd = content_orders.total_amount_usd (snapshot, same value).
-- verified_by references identity.users — cross-schema, no FK intentionally.
--
-- Example — Gaz Mawete individual order, paid by MoMo:
--   id: 'p1p2...', order_id: 'o1o2...',
--   amount_usd: 60.00, payment_method: 'mobile_money',
--   payment_proof_url: 'https://res.cloudinary.com/.../gaz_receipt_march.jpg',
--   status: 'verified', verified_at: '2026-03-01 14:23:00+00',
--   receipt_url: 'https://res.cloudinary.com/.../receipt_o1o2.pdf'
--
-- Example — Team Paiya package, paid by bank transfer:
--   id: 'p3p4...', order_id: 'o3o4...',
--   amount_usd: 400.00, payment_method: 'bank_transfer',
--   payment_proof_url: 'https://res.cloudinary.com/.../teampaiya_virement.jpg',
--   status: 'pending'   ← admin has not verified yet
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.content_payments (
    id                UUID                   NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    order_id          UUID                   NOT NULL REFERENCES content.content_orders (id) ON DELETE CASCADE,
    amount_usd        NUMERIC(10,2)          NOT NULL,
    payment_method    content.payment_method,
    payment_proof_url VARCHAR(500),
    status            content.payment_status NOT NULL DEFAULT 'pending',
    -- Cross-schema ref to identity.users (admin who verified). No FK by design.
    verified_by       UUID,
    verified_at       TIMESTAMPTZ,
    receipt_url       VARCHAR(500),
    notes             VARCHAR(500),
    created_at        TIMESTAMPTZ,
    created_by        TEXT,
    updated_at        TIMESTAMPTZ,
    updated_by        TEXT,

    -- One payment per order. Prevents duplicate payment records.
    CONSTRAINT uq_content_payments_order_id UNIQUE (order_id),
    CONSTRAINT chk_content_payments_amount  CHECK  (amount_usd >= 0)
);

COMMENT ON TABLE  content.content_payments
    IS 'One payment per order (enforced by UNIQUE on order_id). One receipt covers all items in the order.';
COMMENT ON COLUMN content.content_payments.order_id
    IS 'UNIQUE: one payment record per order. Whether 1 item or 5, the customer gets one receipt.';
COMMENT ON COLUMN content.content_payments.payment_proof_url
    IS 'Cloudinary URL of the bank/MoMo receipt photo uploaded by admin.';
COMMENT ON COLUMN content.content_payments.verified_by
    IS 'References identity.users.id — cross-schema, enforced at application level.';
COMMENT ON COLUMN content.content_payments.receipt_url
    IS 'Generated PDF receipt stored on Cloudinary. Set after status transitions to verified.';


-- =============================================================================
-- 7. TAGGING JUNCTION TABLES
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- article_tags / video_tags
-- Example: article "Gaz Mawete: La voix de Bandal" tagged with:
--   (article_id: 'c3d4...', tag_id: gaz-mawete-tag-uuid)
--   (article_id: 'c3d4...', tag_id: kinshasa-tag-uuid)
--   (article_id: 'c3d4...', tag_id: afrobeats-tag-uuid)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_tags (
    article_id UUID NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    tag_id     UUID NOT NULL REFERENCES content.tags (id) ON DELETE CASCADE,
    PRIMARY KEY (article_id, tag_id)
);

CREATE TABLE content.video_tags (
    video_id UUID NOT NULL REFERENCES content.videos (id) ON DELETE CASCADE,
    tag_id   UUID NOT NULL REFERENCES content.tags (id) ON DELETE CASCADE,
    PRIMARY KEY (video_id, tag_id)
);


-- =============================================================================
-- 8. USER INTERACTION TABLES
-- user_id references identity.users.id — no FK constraint (cross-schema).
-- Enforced at application level. This keeps content schema microservice-ready.
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- article_likes
-- Toggle: INSERT to like, DELETE to unlike. Composite PK prevents duplicates.
-- App decrements articles.like_count on DELETE, increments on INSERT.
--
-- Example: user 'u1u2...' likes article 'c3d4...' on 2026-03-01
--   (user_id: 'u1u2...', article_id: 'c3d4...', created_at: '2026-03-01 10:15:00+00')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_likes (
    user_id    UUID        NOT NULL,
    article_id UUID        NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, article_id)
);

COMMENT ON TABLE content.article_likes
    IS 'Toggle like. Composite PK prevents duplicate likes. App maintains articles.like_count.';


-- ─────────────────────────────────────────────────────────────────────────────
-- article_bookmarks
-- "My Library" feature. Same pattern as likes.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_bookmarks (
    user_id    UUID        NOT NULL,
    article_id UUID        NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, article_id)
);


-- ─────────────────────────────────────────────────────────────────────────────
-- article_shares
-- user_id nullable — anonymous and social shares are counted too.
--
-- Example: logged-in user shares to WhatsApp:
--   (id: 'sh1...', user_id: 'u1u2...', article_id: 'c3d4...', created_at: now())
-- Example: anonymous share via link:
--   (id: 'sh2...', user_id: NULL, article_id: 'c3d4...', created_at: now())
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_shares (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID,
    article_id UUID        NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON COLUMN content.article_shares.user_id
    IS 'NULL for anonymous/social shares. Non-null for logged-in users.';


-- ─────────────────────────────────────────────────────────────────────────────
-- article_comments
-- Soft-deleted to preserve thread structure (deleted rows stay, body hidden).
--
-- Example:
--   (id: 'cm1...', user_id: 'u1u2...', article_id: 'c3d4...',
--    body: 'Excellent article sur Gaz Mawete!', is_deleted: false)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.article_comments (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID        NOT NULL,
    article_id UUID        NOT NULL REFERENCES content.articles (id) ON DELETE CASCADE,
    body       TEXT        NOT NULL,
    is_deleted BOOLEAN     NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT
);

COMMENT ON COLUMN content.article_comments.is_deleted
    IS 'Soft delete. Row stays to preserve thread structure; frontend shows "[deleted]" placeholder.';


-- ─────────────────────────────────────────────────────────────────────────────
-- video_ratings
-- One rating per user per video (UNIQUE constraint). Updatable.
-- App recomputes videos.rating_average after each insert/update.
--
-- Example:
--   (id: 'vr1...', user_id: 'u1u2...', video_id: 'v1v2...', stars: 5)
--   (id: 'vr2...', user_id: 'u3u4...', video_id: 'v1v2...', stars: 4)
--   → App updates: videos.rating_average = (5+4)/2 = 4.5, videos.rating_count = 2
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.video_ratings (
    id         UUID     NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID     NOT NULL,
    video_id   UUID     NOT NULL REFERENCES content.videos (id) ON DELETE CASCADE,
    stars      SMALLINT NOT NULL,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT uq_video_ratings_user_video UNIQUE (user_id, video_id),
    CONSTRAINT chk_video_ratings_stars     CHECK  (stars >= 1 AND stars <= 5)
);

COMMENT ON TABLE content.video_ratings
    IS 'One rating per user per video. Updating replaces the existing row (UNIQUE on user+video).';


-- ─────────────────────────────────────────────────────────────────────────────
-- video_shares — anonymous allowed (same pattern as article_shares)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.video_shares (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID,
    video_id   UUID        NOT NULL REFERENCES content.videos (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON COLUMN content.video_shares.user_id
    IS 'NULL for anonymous/social shares.';


-- ─────────────────────────────────────────────────────────────────────────────
-- playlists / playlist_videos
-- Personal video playlists. sort_order controls display order.
--
-- Example:
--   playlist: (id: 'pl1...', user_id: 'u1u2...', name: 'Mes videos préférées')
--   items:    (playlist_id: 'pl1...', video_id: 'v1v2...', sort_order: 1)
--             (playlist_id: 'pl1...', video_id: 'v3v4...', sort_order: 2)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.playlists (
    id         UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID         NOT NULL,
    name       VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT
);

CREATE TABLE content.playlist_videos (
    playlist_id UUID NOT NULL REFERENCES content.playlists (id) ON DELETE CASCADE,
    video_id    UUID NOT NULL REFERENCES content.videos (id) ON DELETE CASCADE,
    sort_order  INT  NOT NULL DEFAULT 0,
    PRIMARY KEY (playlist_id, video_id)
);


-- ─────────────────────────────────────────────────────────────────────────────
-- short_video_likes / short_video_bookmarks / short_video_shares
-- Same patterns as article interactions.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.short_video_likes (
    user_id        UUID        NOT NULL,
    short_video_id UUID        NOT NULL REFERENCES content.short_videos (id) ON DELETE CASCADE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, short_video_id)
);

CREATE TABLE content.short_video_bookmarks (
    user_id        UUID        NOT NULL,
    short_video_id UUID        NOT NULL REFERENCES content.short_videos (id) ON DELETE CASCADE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, short_video_id)
);

CREATE TABLE content.short_video_shares (
    id             UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id        UUID,
    short_video_id UUID        NOT NULL REFERENCES content.short_videos (id) ON DELETE CASCADE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON COLUMN content.short_video_shares.user_id
    IS 'NULL for anonymous/social shares.';


-- =============================================================================
-- 9. INDEXES
-- =============================================================================

-- Content status & publish feed (heaviest queries)
CREATE INDEX ix_articles_status       ON content.articles (status);
CREATE INDEX ix_articles_published_at ON content.articles (published_at DESC)
    WHERE status = 'published';
CREATE INDEX ix_articles_promoted     ON content.articles (is_promoted, promoted_until)
    WHERE is_promoted = TRUE;
CREATE INDEX ix_articles_category_id  ON content.articles (category_id);
CREATE INDEX ix_articles_customer_id  ON content.articles (customer_id);

CREATE INDEX ix_videos_status         ON content.videos (status);
CREATE INDEX ix_videos_published_at   ON content.videos (published_at DESC)
    WHERE status = 'published';
CREATE INDEX ix_videos_promoted       ON content.videos (is_promoted, promoted_until)
    WHERE is_promoted = TRUE;
CREATE INDEX ix_videos_category_id    ON content.videos (category_id);
CREATE INDEX ix_videos_customer_id    ON content.videos (customer_id);

CREATE INDEX ix_short_videos_active   ON content.short_videos (is_active);
CREATE INDEX ix_short_videos_video_id ON content.short_videos (video_id);

-- Commerce queries
CREATE INDEX ix_orders_customer_id       ON content.content_orders (customer_id);
CREATE INDEX ix_orders_status            ON content.content_orders (status);
CREATE INDEX ix_orders_package_id        ON content.content_orders (package_id)
    WHERE package_id IS NOT NULL;
CREATE INDEX ix_order_items_order_id     ON content.content_order_items (order_id);
CREATE INDEX ix_order_items_category_id  ON content.content_order_items (category_id);
CREATE INDEX ix_item_tiers_order_item_id ON content.content_item_tiers (order_item_id);
-- Back-reference indexes: quickly find the content item created for a given order item
CREATE INDEX ix_articles_order_item_id   ON content.articles (order_item_id)
    WHERE order_item_id IS NOT NULL;
CREATE INDEX ix_videos_order_item_id     ON content.videos (order_item_id)
    WHERE order_item_id IS NOT NULL;
CREATE INDEX ix_payments_status          ON content.content_payments (status);

-- Config lookups
CREATE INDEX ix_categories_type_id    ON content.categories (content_type_id);
CREATE INDEX ix_categories_is_active  ON content.categories (is_active);
CREATE INDEX ix_cat_pricing_cat_id    ON content.category_pricing (category_id);

-- Interaction feed queries
CREATE INDEX ix_article_comments_article ON content.article_comments (article_id)
    WHERE is_deleted = FALSE;
CREATE INDEX ix_video_ratings_video_id   ON content.video_ratings (video_id);
CREATE INDEX ix_playlists_user_id        ON content.playlists (user_id);
CREATE INDEX ix_playlist_videos_video_id ON content.playlist_videos (video_id);


-- =============================================================================
-- 10. FULL-TEXT SEARCH INDEXES
-- Unified search endpoint: GET /api/v1/public/search?q=&type=&category=
-- Language config: 'french' (platform primary language).
-- Partial indexes on status = 'published' keep the index lean.
-- =============================================================================

CREATE INDEX ix_articles_fts ON content.articles
    USING GIN (
        to_tsvector('french',
            coalesce(title, '') || ' ' ||
            coalesce(body, '')  || ' ' ||
            coalesce(author_name, '')
        )
    )
    WHERE status = 'published';

CREATE INDEX ix_videos_fts ON content.videos
    USING GIN (
        to_tsvector('french',
            coalesce(title, '') || ' ' ||
            coalesce(description, '')
        )
    )
    WHERE status = 'published';

CREATE INDEX ix_lyrics_fts ON content.lyrics
    USING GIN (
        to_tsvector('french',
            coalesce(song_title, '')   || ' ' ||
            coalesce(artist_name, '')  || ' ' ||
            coalesce(lyrics_text, '')
        )
    );


-- =============================================================================
-- END OF CONTENT MODULE SCHEMA
-- =============================================================================
--
-- TABLES (30 total)
-- ──────────────────────────────────────────────────────────────────────────────
-- Reference / lookup (3):
--   content_types, pricing_tiers, promotion_levels
--
-- Admin configuration (6):
--   categories, category_pricing, customers, packages, package_slots, tags
--
-- Main content (4):
--   articles, videos, short_videos, lyrics
--
-- Commerce / order pattern (4):
--   content_orders, content_order_items, content_item_tiers, content_payments
--
-- Tagging junctions (2):
--   article_tags, video_tags
--
-- User interactions (11):
--   article_likes, article_bookmarks, article_shares, article_comments
--   video_ratings, video_shares
--   playlists, playlist_videos
--   short_video_likes, short_video_bookmarks, short_video_shares
--
-- CROSS-SCHEMA DEPENDENCIES (no FK, enforced at app level)
-- ──────────────────────────────────────────────────────────────────────────────
--   content.customers              ← distinct from identity.users (B2B vs B2C)
--   content.*.user_id              → identity.users.id
--   content.content_payments.verified_by → identity.users.id
-- =============================================================================