-- =============================================================================
-- 116 Platform — Lyrics Feature SQL Schema (full improvement)
-- Schema:   content
-- Database: PostgreSQL 15+
--
-- This file is a companion to ../../docs/CONTENT_SCHEMA.sql, scoped to the
-- lyrics feature described in this folder's specs (00-overview.md, specs/01–14).
-- It is written as a delta on top of the CURRENTLY IMPLEMENTED content.lyrics
-- table (matching the real C# LyricsEntity — see specs/01, "What exists today"
-- in 00-overview.md), not the older article_id/video_id planning shape in
-- CONTENT_SCHEMA.sql — the real entity only ever had video_id.
--
-- DESIGN PRINCIPLES (same as CONTENT_SCHEMA.sql, restated for this file)
-- ─────────────────────────────────────────────────────────────────────
-- 1. No explicit multi-statement transactions anywhere in this feature.
--    Every write is a single aggregate mutation per statement/commit —
--    see 00-overview.md's ACID note. Append-only history tables (revisions,
--    votes) are INSERT-only; corrections are new rows, not UPDATEs.
-- 2. Cross-schema FKs to core.files(id) follow the exact pattern already
--    established for content.articles.cover_image_file_id /
--    content.videos.thumbnail_file_id (see docs/file-entity-migration/) —
--    same database, different schema, ON DELETE SET NULL, no DbSet navigation
--    needed on the Content side.
-- 3. Cross-schema references to identity.users stay FK-free (UUID column,
--    no REFERENCES), matching every existing user_id/author_id column in
--    CONTENT_SCHEMA.sql — enforced at the application level only.
-- 4. Uniqueness and idempotency are enforced by constraints, not application
--    read-then-write logic (UNIQUE(revision_id, user_id) for votes, etc.).
-- 5. A free-text display column (artist_name, album) always coexists with its
--    optional FK to a real entity (artist_id → content.artists, album_id →
--    content.albums) — linking is additive, never a breaking migration.
--
-- READ ORDER
-- ──────────
-- Section 1  — Enum types (new)
-- Section 2  — content.lyrics: ALTER for category/customer/order-item (full
--              parity with articles/videos), slug, editorial status, cover,
--              credits, counters, artist/album links (specs 01,03,04,08,12)
-- Section 3  — content.videos: ALTER for artist_id (spec 08)
-- Section 4  — New entities: artists, albums, streaming_links (spec 08, 09)
-- Section 5  — Tags join: lyrics_tags (spec 07, reuses existing content.tags)
-- Section 6  — Interactions: lyrics_likes, lyrics_shares, lyrics_view_events (spec 04, 05)
-- Section 7  — Translations & review: lyrics_translations,
--              lyrics_translation_revisions, lyrics_translation_votes (spec 10)
-- Section 8  — Community submissions & corrections: lyrics_submissions,
--              lyrics_revisions, lyrics_revision_votes (spec 11)
-- Section 9  — Monetization: content.content_kind gains 'lyrics', content.lyrics
--              gains customer_id/order_item_id — reuses the existing Commerce
--              module, no new tables (spec 12)
-- Section 10 — Indexes
-- Section 11 — Full-text search index update
-- Section 12 — Summary
-- =============================================================================


-- =============================================================================
-- SECTION 1 — ENUM TYPES (new)
-- content.content_status already exists (CONTENT_SCHEMA.sql) and is reused
-- as-is for lyrics — no new status enum needed (spec 01).
-- =============================================================================

CREATE TYPE content.submission_status AS ENUM (
    'pending',
    'approved',
    'rejected',
    'needs_revision'
);

CREATE TYPE content.revision_status AS ENUM (
    'pending',
    'accepted',
    'rejected'
);

CREATE TYPE content.vote AS ENUM (
    'approve',
    'reject'
);

CREATE TYPE content.translation_source AS ENUM (
    'ai',
    'community'
);

CREATE TYPE content.streaming_platform AS ENUM (
    'spotify',
    'apple_music',
    'youtube_music',
    'tidal'
);

-- Monetization (spec 12) reuses the EXISTING content.content_kind enum (CONTENT_SCHEMA.sql)
-- and content_orders/content_order_items/content_payments/promotion_levels tables — see
-- Section 9. No new enum types needed for monetization; content.content_kind gains one
-- new value ('lyrics') via ALTER TYPE there instead.

COMMENT ON TYPE content.submission_status  IS 'Lifecycle of a community-submitted new song, before it becomes a real content.lyrics row (spec 11).';
COMMENT ON TYPE content.revision_status    IS 'Lifecycle of a single proposed correction (to a translation or to the canonical lyrics text) — specs 10, 11.';
COMMENT ON TYPE content.vote               IS 'A single community member''s vote on a pending revision — specs 10, 11.';
COMMENT ON TYPE content.translation_source IS 'Which origin produced a translation''s CURRENT published text — the row itself is not versioned, its revisions are (spec 10).';
COMMENT ON TYPE content.streaming_platform IS 'The four streaming platforms the "go to album" launcher supports (spec 09).';


-- =============================================================================
-- SECTION 2 — content.lyrics: full improvement ALTER
-- Matches the real, currently-implemented LyricsEntity plus every addition
-- from specs 01, 03, 04, 08, 12. Existing columns (id, video_id, song_title,
-- artist_name, lyrics_text, language, meta_title, meta_description,
-- structured_data, author_id, created_at/by, updated_at/by) are NOT restated
-- here — only what's new.
-- =============================================================================

-- ── Spec 01 — category, commerce fields, slug + editorial status workflow ──
-- Full parity with content.articles/content.videos: category_id is required
-- and determines free vs. paid via content.categories.is_free (existing
-- column, CONTENT_SCHEMA.sql section 4); customer_id/order_item_id are set
-- together, either at creation (CreatePaid) or later via an Update() call
-- that retroactively links an existing free lyrics page to a new order
-- (spec 12) — same mechanism content.articles.customer_id/order_item_id
-- already support.
ALTER TABLE content.lyrics
    ADD COLUMN category_id      UUID                    ,
    ADD COLUMN customer_id      UUID REFERENCES content.customers (id) ON DELETE SET NULL,
    -- FK to content.content_order_items — added in Section 9 (forward
    -- reference), same technique CONTENT_SCHEMA.sql itself uses for
    -- articles/videos → content_order_items.
    ADD COLUMN order_item_id    UUID                    ,
    ADD COLUMN slug             VARCHAR(220)           ,
    ADD COLUMN status           content.content_status NOT NULL DEFAULT 'draft',
    ADD COLUMN rejection_reason VARCHAR(500)           ,
    ADD COLUMN published_at     TIMESTAMPTZ            ;

-- Backfill existing rows before enforcing NOT NULL + UNIQUE constraints —
-- see spec 01 §6. category_id backfill: assign every pre-existing row to a
-- single seeded free "Standard Lyrics" category (see Section 9) — admins can
-- re-categorize afterward through the normal edit flow, same as any other
-- category reassignment.
-- UPDATE content.lyrics
--    SET slug = lower(regexp_replace(artist_name || ' ' || song_title || ' lyrics', '[^a-zA-Z0-9]+', '-', 'g'))
--              || '-' || substr(id::text, 1, 8),
--        category_id = <standard-lyrics-category-id>
--  WHERE slug IS NULL;

ALTER TABLE content.lyrics
    ALTER COLUMN slug SET NOT NULL,
    ALTER COLUMN category_id SET NOT NULL;

ALTER TABLE content.lyrics
    ADD CONSTRAINT uq_lyrics_slug UNIQUE (slug);

ALTER TABLE content.lyrics
    ADD CONSTRAINT fk_lyrics_category_id
    FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE RESTRICT;

COMMENT ON COLUMN content.lyrics.category_id
    IS 'Required, exactly like content.articles.category_id / content.videos.category_id. Determines free vs. paid via the category''s own is_free flag — any number of lyrics categories can exist (spec 12), not a single fixed one.';
COMMENT ON COLUMN content.lyrics.customer_id
    IS 'The B2B customer who commissioned this lyrics page as a paid/promoted product. NULL for free content — the common case (admin-entered, community-submitted, verified-artist self-uploaded). Mirrors content.articles.customer_id exactly. Whether ArtistId (spec 08) is set is an independent concern — an artist-linked song is not required to be paid, and an unclaimed-artist song is not required to be free.';
COMMENT ON COLUMN content.lyrics.order_item_id
    IS 'Mirrors content.articles.order_item_id / content.videos.order_item_id. Set either at creation (a lyrics page commissioned as paid from the start) or later via an Update() call (an existing free/published song retroactively linked to a new order for promotion) — spec 12.';
COMMENT ON COLUMN content.lyrics.slug
    IS 'Genius-style single-segment slug: slugify(artist_name || song_title || '' lyrics''), lowercased, generated via the platform''s normal generateSlug(text, {unique:true}) convention — no special-cased collision handling. E.g. "fally-ipupa-mayday-lyrics".';
COMMENT ON COLUMN content.lyrics.status
    IS 'Reuses content.content_status. Free lyrics pages go draft → pending_review → approved → published (no payment gate); paid ones go draft → pending_payment → pending_review → … , exactly like content.articles. Every public read filters to published only (spec 01).';
COMMENT ON COLUMN content.lyrics.rejection_reason
    IS 'Set when status transitions to rejected. Same shape as content.articles.rejection_reason.';
COMMENT ON COLUMN content.lyrics.published_at
    IS 'Set once, when status first transitions to published. Null before that.';


-- ── Spec 03 — cover image + song credits ────────────────────────────────────
ALTER TABLE content.lyrics
    ADD COLUMN cover_image_file_id UUID          REFERENCES core.files (id) ON DELETE SET NULL,
    ADD COLUMN cover_image_url     VARCHAR(500)  ,
    ADD COLUMN album               VARCHAR(200)  ,
    ADD COLUMN release_year        SMALLINT      ,
    ADD COLUMN label               VARCHAR(100)  ,
    ADD COLUMN songwriter          VARCHAR(100)  ,
    ADD COLUMN producer            VARCHAR(100)  ,
    ADD CONSTRAINT chk_lyrics_release_year CHECK (
        release_year IS NULL
        OR (release_year >= 1900 AND release_year <= EXTRACT(YEAR FROM now())::SMALLINT + 1)
    );

COMMENT ON COLUMN content.lyrics.cover_image_file_id
    IS 'FK to core.files(id) — same cross-schema pattern as content.articles.cover_image_file_id. Null until an admin uploads a cover.';
COMMENT ON COLUMN content.lyrics.cover_image_url
    IS 'Denormalized from core.files, kept alongside cover_image_file_id to avoid a JOIN on every read — same pattern as content.articles.cover_image_url.';
COMMENT ON COLUMN content.lyrics.album
    IS 'Free-text album name, always present as the display fallback — see album_id below for the optional link to a real content.albums row (spec 08).';
COMMENT ON COLUMN content.lyrics.songwriter
    IS 'The credited songwriter (music-industry term for "who wrote the words") — distinct from author_id, which is CMS attribution (who entered the record), not a song credit. Named to avoid colliding with author_id.';
COMMENT ON COLUMN content.lyrics.producer
    IS 'The credited producer (who produced the instrumental/recording — the dominant credited role in this genre, e.g. "Prod. by X"). NOT the classical/PRO term "composer" — see specs/03 for why this doc set corrected that terminology.';


-- ── Spec 04 — view / like / share counters ──────────────────────────────────
ALTER TABLE content.lyrics
    ADD COLUMN view_count  INT NOT NULL DEFAULT 0,
    ADD COLUMN like_count  INT NOT NULL DEFAULT 0,
    ADD COLUMN share_count INT NOT NULL DEFAULT 0,
    ADD CONSTRAINT chk_lyrics_view_count  CHECK (view_count >= 0),
    ADD CONSTRAINT chk_lyrics_like_count  CHECK (like_count >= 0),
    ADD CONSTRAINT chk_lyrics_share_count CHECK (share_count >= 0);

COMMENT ON COLUMN content.lyrics.view_count
    IS 'Cached, incremented only by counted view events (content.lyrics_view_events.is_counted = TRUE) — gated by the read-time rule in spec 05, not a raw page-load counter.';


-- ── Spec 08 — artist / album links ──────────────────────────────────────────
-- FKs to content.artists / content.albums are added further below, after
-- those tables are created (Section 4) — forward reference, same technique
-- CONTENT_SCHEMA.sql uses for articles/videos → content_order_items.
ALTER TABLE content.lyrics
    ADD COLUMN artist_id UUID,
    ADD COLUMN album_id  UUID;

COMMENT ON COLUMN content.lyrics.artist_id
    IS 'Optional link to a claimed content.artists profile. Null for the common case of an unclaimed artist — artist_name remains the display fallback either way. FK added in Section 4.';
COMMENT ON COLUMN content.lyrics.album_id
    IS 'Optional link to a real content.albums row. Null for the common case — the free-text album column remains the display fallback. FK added in Section 4.';


-- ── Spec 12 — promoted "Top Lyrics" placement ───────────────────────────────
ALTER TABLE content.lyrics
    ADD COLUMN is_promoted    BOOLEAN     NOT NULL DEFAULT FALSE,
    ADD COLUMN promoted_until TIMESTAMPTZ ;

COMMENT ON COLUMN content.lyrics.is_promoted
    IS 'Mirrors content.articles.is_promoted exactly — stamped by the Commerce flow, never through ordinary lyrics endpoints. Deliberately excluded from every "Top Lyrics" sort branch (spec 13) — a promoted record is always a separate, clearly-labeled slot, never blended into the organic view/like/share ranking.';


-- =============================================================================
-- SECTION 3 — content.videos: artist_id (spec 08)
-- An artist page needs to show both their songs AND their videos, so videos
-- get the identical nullable link lyrics does.
-- =============================================================================

ALTER TABLE content.videos
    ADD COLUMN artist_id UUID;

COMMENT ON COLUMN content.videos.artist_id
    IS 'Same optional artist link as content.lyrics.artist_id — enables "their videos" on an artist''s public page. FK added in Section 4.';


-- =============================================================================
-- SECTION 4 — New entities: artists, albums, streaming_links (specs 08, 09)
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- artists
-- A real, addressable artist profile — distinct from the plain-text
-- artist_name column on lyrics/videos. Can exist unclaimed (staff-curated,
-- no linked account, the common case at launch) or claimed by a verified
-- artist account via user_id.
--
-- Example records:
--   Unclaimed, staff-curated (the common case):
--     (id: 'ar1...', name: 'Fally Ipupa', slug: 'fally-ipupa', user_id: NULL)
--
--   Claimed by a verified account:
--     (id: 'ar2...', name: 'Gaz Mawete', slug: 'gaz-mawete',
--      user_id: 'u1u2...', verified_at: '2026-06-01 09:00:00+00')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.artists (
    id              UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name            VARCHAR(100) NOT NULL,
    slug            VARCHAR(220) NOT NULL,
    bio             TEXT,
    avatar_file_id  UUID         REFERENCES core.files (id) ON DELETE SET NULL,
    avatar_url      VARCHAR(500),
    -- Cross-schema ref to identity.users — no FK, same pattern as every
    -- other user_id/author_id column in this schema. NULL = unclaimed.
    user_id         UUID,
    verified_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ,
    created_by      TEXT,
    updated_at      TIMESTAMPTZ,
    updated_by      TEXT,

    CONSTRAINT uq_artists_slug    UNIQUE (slug),
    CONSTRAINT uq_artists_user_id UNIQUE (user_id)
);

COMMENT ON TABLE  content.artists
    IS 'Real, addressable artist profiles. Most rows are unclaimed (user_id NULL) — staff-curated purely to group an artist''s existing catalog. See specs/08 for the full claim/verification design.';
COMMENT ON COLUMN content.artists.user_id
    IS 'The identity user UUID of the verified artist account that owns this profile, or NULL for an unclaimed, staff-curated profile. Once set, this is the identity gate spec 11''s verified-artist fast path checks — a submission from this exact user id is attributed to this profile directly, NEVER by comparing the submitted artist-name text (names change, get misspelled, and can collide between unrelated people). UNIQUE — one user can own at most one artist profile.';
COMMENT ON COLUMN content.artists.avatar_url
    IS 'Denormalized from core.files, same pattern as content.lyrics.cover_image_url.';


-- ─────────────────────────────────────────────────────────────────────────────
-- albums
-- A real, addressable album — distinct from the free-text content.lyrics.album
-- column. Groups songs for "more from this album" (spec 09) and carries
-- per-platform streaming links.
--
-- Example:
--   (id: 'al1...', name: 'Tokooos 2', artist_id: 'ar1...',
--    release_year: 2019, label: 'AZ Music')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.albums (
    id                UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name              VARCHAR(200) NOT NULL,
    artist_id         UUID         REFERENCES content.artists (id) ON DELETE SET NULL,
    cover_image_file_id UUID       REFERENCES core.files (id) ON DELETE SET NULL,
    cover_image_url   VARCHAR(500),
    release_year      SMALLINT,
    label             VARCHAR(100),
    created_at        TIMESTAMPTZ,
    created_by        TEXT,
    updated_at        TIMESTAMPTZ,
    updated_by        TEXT,

    CONSTRAINT chk_albums_release_year CHECK (
        release_year IS NULL
        OR (release_year >= 1900 AND release_year <= EXTRACT(YEAR FROM now())::SMALLINT + 1)
    )
);

COMMENT ON TABLE content.albums
    IS 'Real, addressable albums. artist_id is nullable — an album can exist before its artist is claimed, same relationship as content.lyrics.artist_id.';


-- Forward-reference FKs now that content.artists / content.albums exist
-- (same technique CONTENT_SCHEMA.sql uses for articles/videos → content_order_items).
ALTER TABLE content.lyrics
    ADD CONSTRAINT fk_lyrics_artist_id
    FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE SET NULL;

ALTER TABLE content.lyrics
    ADD CONSTRAINT fk_lyrics_album_id
    FOREIGN KEY (album_id) REFERENCES content.albums (id) ON DELETE SET NULL;

ALTER TABLE content.videos
    ADD CONSTRAINT fk_videos_artist_id
    FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE SET NULL;

-- ON DELETE SET NULL throughout: deleting an artist/album never cascades into
-- deleting the songs/videos that reference it — they fall back to their
-- plain-text artist_name/album display value.


-- ─────────────────────────────────────────────────────────────────────────────
-- streaming_links
-- Curated deep links to a *release* on a specific streaming platform. A
-- release is either an album or a standalone single (a song with no
-- AlbumId) — exactly how Spotify/Apple Music model it themselves; a
-- streaming link tied only to albums would leave every single with no
-- "go to Spotify" launcher at all, which is not acceptable given how many
-- releases on this platform are singles. Exactly one of album_id/lyrics_id
-- is set per row. Absence of a row for a given platform is expected — the
-- public endpoint falls back to a generated search URL (spec 09) — this
-- table is allowed to be sparse.
--
-- Examples:
--   (album_id: 'al1...', lyrics_id: NULL, platform: 'spotify', url: 'https://open.spotify.com/album/...')
--   (album_id: NULL, lyrics_id: 'ly1...', platform: 'spotify', url: 'https://open.spotify.com/track/...')
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.streaming_links (
    id         UUID                          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    album_id   UUID                          NULL REFERENCES content.albums (id) ON DELETE CASCADE,
    lyrics_id  UUID                          NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    platform   content.streaming_platform    NOT NULL,
    url        VARCHAR(500)                  NOT NULL,
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT ck_streaming_links_exactly_one_target CHECK (
        (album_id IS NOT NULL AND lyrics_id IS NULL) OR
        (album_id IS NULL AND lyrics_id IS NOT NULL)
    ),
    CONSTRAINT uq_streaming_links_album_platform UNIQUE (album_id, platform),
    CONSTRAINT uq_streaming_links_lyrics_platform UNIQUE (lyrics_id, platform)
);

COMMENT ON TABLE content.streaming_links
    IS 'One curated link per (release, platform) pair, at most, where a release is either an album or a standalone single (lyrics with no AlbumId). A missing row is normal — resolved to a generated search-query URL at read time, never a gap the frontend has to special-case.';


-- =============================================================================
-- SECTION 5 — Tags join: lyrics_tags (spec 07)
-- Reuses the EXISTING content.tags table (CONTENT_SCHEMA.sql section 3) —
-- no new tag system, only the third junction alongside article_tags/video_tags.
-- =============================================================================

CREATE TABLE content.lyrics_tags (
    lyrics_id UUID NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    tag_id    UUID NOT NULL REFERENCES content.tags (id) ON DELETE CASCADE,
    PRIMARY KEY (lyrics_id, tag_id)
);

COMMENT ON TABLE content.lyrics_tags
    IS 'Same many-to-many pattern as content.article_tags / content.video_tags, same content.tags pool — an "Afrobeat" tag applied to an article or video is the same row a lyrics page applies. Backs the shared-tags branch of the similar-lyrics query (spec 06).';


-- =============================================================================
-- SECTION 6 — Interactions: lyrics_likes, lyrics_shares, lyrics_view_events
-- Direct copies of short_video_likes / short_video_shares, plus a raw
-- view-event table (there is no short_video_view_events table in
-- CONTENT_SCHEMA.sql yet — it was added later directly in the actual
-- ShortVideoViewEventEntity/migration; mirrored here for lyrics) (specs 04, 05).
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_likes
-- Toggle: INSERT to like, DELETE to unlike. Composite PK prevents duplicates.
-- App increments/decrements content.lyrics.like_count on insert/delete.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_likes (
    user_id    UUID        NOT NULL,
    lyrics_id  UUID        NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, lyrics_id)
);

COMMENT ON TABLE content.lyrics_likes
    IS 'Same toggle pattern as content.article_likes / content.short_video_likes. Composite PK prevents duplicate likes.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_shares
-- user_id nullable — anonymous and social shares are counted too, same
-- pattern as content.article_shares / content.short_video_shares.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_shares (
    id         UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id    UUID,
    lyrics_id  UUID        NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON COLUMN content.lyrics_shares.user_id
    IS 'NULL for anonymous/social shares.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_view_events
-- Raw record of a single view, kept separately from the cached view_count so
-- views can be deduplicated per identity and audited later. Only events
-- flagged is_counted incremented the displayed count — gated by BOTH the
-- 24h dedup window AND the read-time rule (spec 05).
--
-- Example — a genuine read that counted:
--   (lyrics_id: 'l1l2...', user_id: 'u1u2...', dedup_key: 'user:u1u2...',
--    is_counted: true, dwell_ms: 42000, scroll_depth_ratio: 0.95)
--
-- Example — a bounce that did not count:
--   (lyrics_id: 'l1l2...', user_id: NULL, dedup_key: 'ip:41.72.x.x',
--    is_counted: false, dwell_ms: 400, scroll_depth_ratio: 0.1)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_view_events (
    id                 UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    lyrics_id          UUID         NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    -- Cross-schema ref to identity.users — no FK. NULL for anonymous views.
    user_id            UUID,
    -- Identity surrogate the view is deduplicated against, in priority order:
    -- user:{userId}, else device:{deviceId}, else ip:{address}, else 'unknown'.
    dedup_key          VARCHAR(100) NOT NULL,
    ip_address         VARCHAR(45),
    user_agent         VARCHAR(500),
    is_counted         BOOLEAN      NOT NULL DEFAULT FALSE,
    -- Read-time algorithm inputs (spec 05) — advisory, re-validated server-side
    -- against lyrics_text's own word count before is_counted is set TRUE.
    dwell_ms           INT          NOT NULL DEFAULT 0,
    scroll_depth_ratio REAL         NOT NULL DEFAULT 0,
    created_at         TIMESTAMPTZ  NOT NULL DEFAULT now(),

    CONSTRAINT chk_lyrics_view_events_dwell_ms CHECK (dwell_ms >= 0),
    CONSTRAINT chk_lyrics_view_events_scroll   CHECK (scroll_depth_ratio >= 0 AND scroll_depth_ratio <= 1)
);

COMMENT ON TABLE content.lyrics_view_events
    IS 'Raw view log, append-only. is_counted reflects BOTH the 24h dedup window (same as short-video views) and the read-time rule (spec 05) — a page load alone never counts, only a plausible read does.';
COMMENT ON COLUMN content.lyrics_view_events.dwell_ms
    IS 'Client-reported foreground dwell time on the lyrics body. Server recomputes an expected reading time from lyrics_text''s own word count and never trusts this value alone (spec 05).';


-- =============================================================================
-- SECTION 7 — Translations & review (spec 10)
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_translations
-- One published translation per (lyrics_id, language) — corrections update
-- this row's text via an accepted revision, they do not create a second row.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_translations (
    id         UUID                          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    lyrics_id  UUID                          NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    language   VARCHAR(10)                   NOT NULL,
    text       TEXT                          NOT NULL,
    source     content.translation_source    NOT NULL DEFAULT 'ai',
    created_at TIMESTAMPTZ,
    created_by TEXT,
    updated_at TIMESTAMPTZ,
    updated_by TEXT,

    CONSTRAINT uq_lyrics_translations_lang UNIQUE (lyrics_id, language)
);

COMMENT ON TABLE content.lyrics_translations
    IS 'AI-generated on first request (source=ai), shown immediately — not gated behind review. source flips to community once an accepted revision updates text.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_translation_revisions
-- A proposed correction. Never mutates the translation directly — only an
-- Accept() call (application-level) updates lyrics_translations.text, once
-- this row's status flips to accepted.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_translation_revisions (
    id               UUID                     NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    translation_id   UUID                     NOT NULL REFERENCES content.lyrics_translations (id) ON DELETE CASCADE,
    proposed_text    TEXT                     NOT NULL,
    edit_summary     VARCHAR(300),
    -- Cross-schema ref to identity.users — no FK.
    proposed_by_user_id UUID                  NOT NULL,
    status           content.revision_status  NOT NULL DEFAULT 'pending',
    decided_by_user_id UUID,
    created_at       TIMESTAMPTZ,
    created_by       TEXT,
    updated_at       TIMESTAMPTZ,
    updated_by       TEXT
);

COMMENT ON TABLE content.lyrics_translation_revisions
    IS 'Wikipedia-style revision history for a translation. decided_by_user_id is NULL for a threshold auto-accept, set for a direct admin/moderator decision.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_translation_votes
-- One vote per user per revision — enforced by the database, not app-level
-- dedup logic (this feature's ACID posture).
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_translation_votes (
    id          UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    revision_id UUID          NOT NULL REFERENCES content.lyrics_translation_revisions (id) ON DELETE CASCADE,
    -- Cross-schema ref to identity.users — no FK.
    user_id     UUID          NOT NULL,
    vote        content.vote  NOT NULL,
    comment     VARCHAR(300),
    created_at  TIMESTAMPTZ   NOT NULL DEFAULT now(),

    CONSTRAINT uq_lyrics_translation_votes_revision_user UNIQUE (revision_id, user_id)
);

COMMENT ON TABLE content.lyrics_translation_votes
    IS 'The UNIQUE(revision_id, user_id) constraint is the actual enforcement of "one vote per user per revision" — a repeat vote is rejected by Postgres, not caught by application logic.';


-- =============================================================================
-- SECTION 8 — Community submissions & corrections (spec 11)
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_submissions
-- A community-submitted new song, pending moderation before it becomes a
-- real content.lyrics row. Distinct from the editorial "draft" status on
-- lyrics itself — a submission isn't a lyrics record yet, it's a proposal
-- to create one.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_submissions (
    id                    UUID                        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    song_title            VARCHAR(200)                NOT NULL,
    artist_name           VARCHAR(100)                NOT NULL,
    lyrics_text           TEXT                        NOT NULL,
    language              VARCHAR(5)                  NOT NULL DEFAULT 'fr',
    -- Cross-schema ref to identity.users — no FK.
    submitted_by_user_id  UUID                        NOT NULL,
    status                content.submission_status   NOT NULL DEFAULT 'pending',
    reviewed_by_user_id   UUID,
    review_note           VARCHAR(500),
    -- Set once approval creates the real row — see the two-step apply
    -- sequence in specs/11 (create lyrics, then link this column).
    published_lyrics_id   UUID                        REFERENCES content.lyrics (id) ON DELETE SET NULL,
    created_at            TIMESTAMPTZ,
    created_by            TEXT,
    updated_at            TIMESTAMPTZ,
    updated_by            TEXT
);

COMMENT ON TABLE content.lyrics_submissions
    IS 'Moderation queue for new-song proposals from users with no claimed artist profile. A user who owns a claimed content.artists row (artists.user_id) skips this table entirely (spec 11''s verified-artist fast path) — never routed here.';
COMMENT ON COLUMN content.lyrics_submissions.published_lyrics_id
    IS 'NULL until approved. If the process is interrupted after the content.lyrics row is created but before this column is set, the submission is left pending with a detectable, repairable inconsistency (spec 11) — never data corruption.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_revisions
-- A proposed correction to an EXISTING, published song's canonical text.
-- Same shape as lyrics_translation_revisions, targeting lyrics.lyrics_text
-- instead of a translation.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_revisions (
    id                  UUID                     NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    lyrics_id           UUID                     NOT NULL REFERENCES content.lyrics (id) ON DELETE CASCADE,
    proposed_text       TEXT                     NOT NULL,
    edit_summary        VARCHAR(300),
    proposed_by_user_id UUID                     NOT NULL,
    status              content.revision_status  NOT NULL DEFAULT 'pending',
    decided_by_user_id  UUID,
    created_at          TIMESTAMPTZ,
    created_by          TEXT,
    updated_at          TIMESTAMPTZ,
    updated_by          TEXT
);

COMMENT ON TABLE content.lyrics_revisions
    IS 'The canonical-text edit history — every proposed correction to a published song''s lyrics_text, whether accepted, rejected, or still pending.';


-- ─────────────────────────────────────────────────────────────────────────────
-- lyrics_revision_votes
-- Mirrors lyrics_translation_votes exactly, FK'd to lyrics_revisions instead.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE content.lyrics_revision_votes (
    id          UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    revision_id UUID          NOT NULL REFERENCES content.lyrics_revisions (id) ON DELETE CASCADE,
    user_id     UUID          NOT NULL,
    vote        content.vote  NOT NULL,
    comment     VARCHAR(300),
    created_at  TIMESTAMPTZ   NOT NULL DEFAULT now(),

    CONSTRAINT uq_lyrics_revision_votes_revision_user UNIQUE (revision_id, user_id)
);


-- =============================================================================
-- SECTION 9 — Monetization (spec 12)
-- Deliberately scoped to three streams: advertising (no schema — reuses the
-- existing ad-serving infrastructure), streaming-affiliate links (no schema —
-- a URL-parameter change inside spec 09's ResolveStreamingLinks, applied at
-- the point those URLs are already generated), and label/artist-paid
-- promoted placement, which is NOT new commerce infrastructure — it reuses
-- the EXISTING content_orders / content_order_items / content_item_tiers /
-- content_payments / promotion_levels tables (CONTENT_SCHEMA.sql section 6)
-- that already drive promoted placement for articles and videos today, with
-- full category parity (Section 2's category_id, this section's content_kind
-- value) — not a single hardcoded commerce-only category.
--
-- Premium subscriptions, a per-artist revenue-share ledger, and data/API
-- licensing are explicitly OUT of scope — they assume payment/payout
-- infrastructure (recurring card billing, cross-border creator payouts) this
-- platform's actual markets don't reliably have. Not built here.
-- =============================================================================

-- ── content.content_kind gains a third value ────────────────────────────────
-- Existing enum (CONTENT_SCHEMA.sql section 2): 'article' | 'video'.
ALTER TYPE content.content_kind ADD VALUE 'lyrics';

-- ── Lyrics categories — full parity with articles/videos, any number, each
-- independently free or paid via the existing content.categories.is_free ──
-- Lyrics categories are NOT a commerce-only concept and NOT a single fixed
-- row — admins create as many as they want through the exact same category
-- CRUD articles/videos already use, once the Lyrics content type exists.
-- content.lyrics_tags (Section 5) remains a SEPARATE, additional concern —
-- discovery/similarity, not editorial/commerce classification. A lyrics
-- record has both a required category_id (this section, via Section 2's
-- ALTER) and a many-to-many tag set, exactly like articles/videos do.
--
-- INSERT INTO content.content_types (id, name) VALUES (<seed-id>, 'Lyrics');
--
-- At minimum, one free default category is needed for the overwhelming
-- majority of lyrics (admin-entered, community-submitted, verified-artist
-- self-uploads) and for backfilling existing rows (Section 2):
-- INSERT INTO content.categories (id, content_type_id, name, slug, description, is_free)
--   VALUES (<seed-id>, <lyrics-content-type-id>, 'Standard Lyrics',
--           'standard-lyrics', 'Default, free category for ordinary lyrics pages.', TRUE);
--
-- Paid categories are created the same way, whenever a commercial product is
-- needed (e.g. sponsored/promoted lyrics placements) — no fixed name or count:
-- INSERT INTO content.categories (id, content_type_id, name, slug, description, is_free)
--   VALUES (<seed-id>, <lyrics-content-type-id>, 'Promoted Lyrics',
--           'promoted-lyrics', 'Paid category for label/artist-commissioned promoted placements.', FALSE);

-- ── content.lyrics.order_item_id → content.content_order_items ─────────────
-- category_id/customer_id/order_item_id columns themselves are added in
-- Section 2 (spec 01) alongside slug/status — this FK is added here, once
-- content.content_order_items exists, as a forward reference (same technique
-- CONTENT_SCHEMA.sql itself uses for articles/videos → content_order_items).
ALTER TABLE content.lyrics
    ADD CONSTRAINT fk_lyrics_order_item_id
    FOREIGN KEY (order_item_id)
    REFERENCES content.content_order_items (id)
    ON DELETE SET NULL;


-- =============================================================================
-- SECTION 10 — INDEXES
-- =============================================================================

-- content.lyrics — status & publish feed (heaviest queries, same shape as
-- the existing ix_articles_status / ix_articles_published_at)
CREATE INDEX ix_lyrics_status        ON content.lyrics (status);
CREATE INDEX ix_lyrics_category_id   ON content.lyrics (category_id);
CREATE INDEX ix_lyrics_published_at  ON content.lyrics (published_at DESC)
    WHERE status = 'published';
CREATE INDEX ix_lyrics_promoted      ON content.lyrics (is_promoted, promoted_until)
    WHERE is_promoted = TRUE;
CREATE INDEX ix_lyrics_artist_id     ON content.lyrics (artist_id)
    WHERE artist_id IS NOT NULL;
CREATE INDEX ix_lyrics_album_id      ON content.lyrics (album_id)
    WHERE album_id IS NOT NULL;
CREATE INDEX ix_lyrics_video_id      ON content.lyrics (video_id)
    WHERE video_id IS NOT NULL;

-- "Top Lyrics" homepage tabs (spec 13) — one partial index per counter,
-- scoped to published so an unpublished draft never leaks into the ranking.
CREATE INDEX ix_lyrics_view_count_desc  ON content.lyrics (view_count DESC)
    WHERE status = 'published';
CREATE INDEX ix_lyrics_like_count_desc  ON content.lyrics (like_count DESC)
    WHERE status = 'published';
CREATE INDEX ix_lyrics_share_count_desc ON content.lyrics (share_count DESC)
    WHERE status = 'published';

-- Artists / albums
CREATE INDEX ix_artists_slug     ON content.artists (slug);
CREATE INDEX ix_artists_user_id  ON content.artists (user_id)
    WHERE user_id IS NOT NULL;
CREATE INDEX ix_albums_artist_id ON content.albums (artist_id);
CREATE INDEX ix_videos_artist_id ON content.videos (artist_id)
    WHERE artist_id IS NOT NULL;

-- Tags
CREATE INDEX ix_lyrics_tags_tag_id ON content.lyrics_tags (tag_id);

-- Interactions
CREATE INDEX ix_lyrics_view_events_lyrics_id  ON content.lyrics_view_events (lyrics_id);
CREATE INDEX ix_lyrics_view_events_dedup      ON content.lyrics_view_events (lyrics_id, dedup_key, created_at)
    WHERE is_counted = TRUE;

-- Translations & review — the review queue's primary query shape
CREATE INDEX ix_lyrics_translations_lyrics_id       ON content.lyrics_translations (lyrics_id);
CREATE INDEX ix_translation_revisions_pending        ON content.lyrics_translation_revisions (translation_id, status)
    WHERE status = 'pending';
CREATE INDEX ix_lyrics_revisions_pending             ON content.lyrics_revisions (lyrics_id, status)
    WHERE status = 'pending';

-- Community submissions
CREATE INDEX ix_lyrics_submissions_status ON content.lyrics_submissions (status);

-- Monetization — back-reference index, same shape as the existing
-- ix_articles_order_item_id / ix_videos_order_item_id in CONTENT_SCHEMA.sql
CREATE INDEX ix_lyrics_order_item_id ON content.lyrics (order_item_id)
    WHERE order_item_id IS NOT NULL;
CREATE INDEX ix_lyrics_customer_id   ON content.lyrics (customer_id)
    WHERE customer_id IS NOT NULL;


-- =============================================================================
-- SECTION 11 — FULL-TEXT SEARCH INDEX UPDATE
-- content.lyrics already has ix_lyrics_fts (CONTENT_SCHEMA.sql section 10),
-- built over song_title/artist_name/lyrics_text. No change needed — none of
-- those three columns changed shape in this file. Left here as a pointer so
-- a reader scanning this file for "did the FTS index change" gets a direct
-- answer: no.
-- =============================================================================


-- =============================================================================
-- SECTION 12 — SUMMARY
-- =============================================================================
--
-- ALTERED (3):
--   content.lyrics — category_id/customer_id/order_item_id (full parity with
--                    articles/videos), slug, status workflow, cover+credits,
--                    counters, artist_id/album_id, is_promoted/promoted_until
--                    (specs 01, 03, 04, 08, 12)
--   content.videos — artist_id (spec 08)
--   content.content_kind (existing enum) — gains 'lyrics' value (spec 12)
--
-- NEW TABLES (13):
--   Domain (3):
--     artists, albums, streaming_links
--   Tags (1):
--     lyrics_tags
--   Interactions (3):
--     lyrics_likes, lyrics_shares, lyrics_view_events
--   Translations & review (3):
--     lyrics_translations, lyrics_translation_revisions, lyrics_translation_votes
--   Community submissions & corrections (3):
--     lyrics_submissions, lyrics_revisions, lyrics_revision_votes
--
-- Monetization (spec 12) adds NO new tables — advertising and streaming-affiliate
-- links are code/config changes, not schema; promoted placement reuses the
-- existing content_orders/content_order_items/content_item_tiers/content_payments/
-- promotion_levels tables (CONTENT_SCHEMA.sql) with full category parity —
-- any number of lyrics categories, each independently free or paid via the
-- existing content.categories.is_free, exactly like articles/videos. Premium
-- subscriptions and a per-artist revenue-share ledger were considered and
-- deliberately dropped from scope — see Section 9's header comment.
--
-- NEW ENUM TYPES (5):
--   submission_status, revision_status, vote, translation_source,
--   streaming_platform
--
-- CROSS-SCHEMA DEPENDENCIES (no FK, enforced at app level — same convention
-- as CONTENT_SCHEMA.sql's own footer)
-- ──────────────────────────────────────────────────────────────────────────
--   content.artists.user_id                        → identity.users.id
--   content.*.user_id / *_by_user_id / submitted_by_user_id → identity.users.id
--
-- CROSS-SCHEMA DEPENDENCIES (real FK — same database, different schema,
-- matching content.articles.cover_image_file_id's existing precedent)
-- ──────────────────────────────────────────────────────────────────────────
--   content.lyrics.cover_image_file_id  → core.files.id
--   content.artists.avatar_file_id      → core.files.id
--   content.albums.cover_image_file_id  → core.files.id
-- =============================================================================
