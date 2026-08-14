-- ============================================================================
-- Artist Page — full schema delta
-- ============================================================================
-- Reference shape only. The authoritative source is the EF Core entity
-- configurations; migrations are generated from those, never from this file.
-- Kept in sync so the whole delta is readable in one place.
--
-- Schema: content
-- Naming: snake_case (EFCore.NamingConventions)
-- ============================================================================


-- ----------------------------------------------------------------------------
-- Spec 01 — Artist identity fields                                     (Gap 7)
-- ----------------------------------------------------------------------------
-- Four nullable columns on the existing artists table. No backfill required:
-- the identity block renders whatever exists and hides the rest.
--
-- birthdate is DATE, never TIMESTAMPTZ. A timestamp converted across timezones
-- moves a birthday by a day.
-- aliases is text[] — Npgsql maps List<string> natively. Display-only and
-- never queried, so a join table would be ceremony.

ALTER TABLE content.artists ADD COLUMN real_name  VARCHAR(150);
ALTER TABLE content.artists ADD COLUMN aliases    TEXT[] NOT NULL DEFAULT '{}';
ALTER TABLE content.artists ADD COLUMN birthdate  DATE;
ALTER TABLE content.artists ADD COLUMN hometown   VARCHAR(120);


-- ----------------------------------------------------------------------------
-- Spec 07 — Directory sort/bucket/search columns                 (Gap 1, 1b, 1c)
-- ----------------------------------------------------------------------------
-- Derived from Name by the domain on create and rename. Not generated columns,
-- and deliberately not unaccent(): see 00-overview.md.
--
-- name_folded    — uppercase, accent-stripped. Drives ORDER BY and search.
-- initial_letter — first char of name_folded when A-Z, otherwise '#'.
--                  Drives the letter filter and availableLetters.

ALTER TABLE content.artists ADD COLUMN name_folded    VARCHAR(100) NOT NULL DEFAULT '';
ALTER TABLE content.artists ADD COLUMN initial_letter CHAR(1)      NOT NULL DEFAULT '#';

CREATE INDEX ix_artists_name_folded    ON content.artists (name_folded);
CREATE INDEX ix_artists_initial_letter ON content.artists (initial_letter, name_folded);

-- Backfill for rows that predate the columns is performed in the migration's
-- Up() as raw SQL, then the DEFAULTs are irrelevant — the domain always writes
-- both values explicitly.


-- ----------------------------------------------------------------------------
-- Spec 02 — Artist social links                                        (Gap 8)
-- ----------------------------------------------------------------------------
-- Child table keyed by platform, mirroring content.streaming_links exactly.
-- Not N nullable URL columns: adding a platform becomes an enum member rather
-- than a migration, and (artist_id, platform) uniqueness is expressible.

CREATE TABLE content.artist_social_links (
    id          UUID         PRIMARY KEY,
    artist_id   UUID         NOT NULL,
    platform    INTEGER      NOT NULL,   -- EnumSocialPlatform
    url         VARCHAR(500) NOT NULL,

    created_at  TIMESTAMPTZ  NOT NULL,
    updated_at  TIMESTAMPTZ,
    created_by  TEXT,
    updated_by  TEXT,

    CONSTRAINT fk_artist_social_links_artist
        FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ux_artist_social_links_artist_platform
    ON content.artist_social_links (artist_id, platform);

-- EnumSocialPlatform
--   0 Instagram
--   1 X
--   2 Facebook
--   3 YouTube
--   4 TikTok
--   5 Website


-- ----------------------------------------------------------------------------
-- Spec 03 — Release-type discriminator                                 (Gap 4)
-- ----------------------------------------------------------------------------
-- Albums and mixtapes are the same table split by one column. EP and Single
-- exist in the enum from the start — adding an enum member later is cheap,
-- re-classifying live rows is not.
--
-- Existing rows backfill to Album (0) and editors correct from there, rather
-- than blocking the migration on a full catalog audit.

ALTER TABLE content.albums ADD COLUMN release_type INTEGER NOT NULL DEFAULT 0;

CREATE INDEX ix_albums_artist_id_release_type
    ON content.albums (artist_id, release_type);

-- EnumReleaseType
--   0 Album
--   1 Mixtape
--   2 EP
--   3 Single


-- ----------------------------------------------------------------------------
-- Spec 05 — Article → artist tagging                                   (Gap 5)
-- ----------------------------------------------------------------------------
-- Join table, not a single FK: an article routinely covers several artists,
-- and a single FK would force an arbitrary choice. Mirrors
-- content.article_tags / content.lyrics_tags.

CREATE TABLE content.article_artists (
    id          UUID        PRIMARY KEY,
    article_id  UUID        NOT NULL,
    artist_id   UUID        NOT NULL,

    created_at  TIMESTAMPTZ NOT NULL,
    updated_at  TIMESTAMPTZ,
    created_by  TEXT,
    updated_by  TEXT,

    CONSTRAINT fk_article_artists_article
        FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE,
    CONSTRAINT fk_article_artists_artist
        FOREIGN KEY (artist_id)  REFERENCES content.artists  (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ux_article_artists_article_artist
    ON content.article_artists (article_id, artist_id);

-- Drives "every published article tagged to this artist", so the artist side
-- is the hot lookup direction and needs its own index.
CREATE INDEX ix_article_artists_artist_id
    ON content.article_artists (artist_id);


-- ----------------------------------------------------------------------------
-- Spec 06 — Supporting indexes for the content predicate         (Gap 1, 1a)
-- ----------------------------------------------------------------------------
-- contentCount and the directory filter run the same correlated subqueries per
-- artist row. Each surface needs its (artist_id, status) lookup to be an index
-- hit, or the directory becomes the slowest route on the site.

CREATE INDEX ix_lyrics_artist_id_status   ON content.lyrics   (artist_id, status);
CREATE INDEX ix_videos_artist_id_status   ON content.videos   (artist_id, status);
CREATE INDEX ix_articles_status           ON content.articles (status);
-- albums: covered by ix_albums_artist_id_release_type above.


-- ----------------------------------------------------------------------------
-- Spec 09 — Video artist slug                                          (Gap 6)
-- ----------------------------------------------------------------------------
-- No schema change. ArtistSlug is resolved server-side through the existing
-- videos.artist_id FK, mirroring what the lyrics detail response already does.


-- ============================================================================
-- Migration
-- ============================================================================
-- The whole delta ships as ONE migration, `AddArtistPageFeature`, generated
-- against ContentDbContext and left unapplied (matching every prior phase in
-- this module). The specs land together in one phase, so splitting the model
-- diff into six migrations would be ceremony EF cannot even express once the
-- model changes are all in place.
--
-- The migration's Up() also backfills name_folded and initial_letter for rows
-- that predate the columns, using Postgres translate() over the accented
-- characters occurring in Latin-script names; the domain maintains both
-- columns from that point on.
-- ============================================================================
