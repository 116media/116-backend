CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'content') THEN
        CREATE SCHEMA content;
    END IF;
END $EF$;

CREATE TABLE content.content_types (
    id uuid NOT NULL,
    name character varying(30) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_content_types PRIMARY KEY (id)
);

CREATE TABLE content.customers (
    id uuid NOT NULL,
    full_name character varying(100) NOT NULL,
    email character varying(200) NOT NULL,
    phone character varying(30),
    company character varying(100),
    notes character varying(500),
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_customers PRIMARY KEY (id)
);

CREATE TABLE content.packages (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500) NOT NULL,
    flat_price_usd numeric(10,2) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_packages PRIMARY KEY (id)
);

CREATE TABLE content.playlists (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(100) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_playlists PRIMARY KEY (id)
);

CREATE TABLE content.pricing_tiers (
    id uuid NOT NULL,
    name character varying(40) NOT NULL,
    description character varying(200) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_pricing_tiers PRIMARY KEY (id)
);

CREATE TABLE content.promotion_levels (
    id uuid NOT NULL,
    name character varying(40) NOT NULL,
    duration_days integer NOT NULL,
    price_usd numeric(10,2) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_promotion_levels PRIMARY KEY (id)
);

CREATE TABLE content.tags (
    id uuid NOT NULL,
    name character varying(50) NOT NULL,
    slug character varying(60) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_tags PRIMARY KEY (id)
);

CREATE TABLE content.categories (
    id uuid NOT NULL,
    content_type_id uuid NOT NULL,
    name character varying(60) NOT NULL,
    slug character varying(80) NOT NULL,
    description character varying(300) NOT NULL,
    is_free boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_categories PRIMARY KEY (id),
    CONSTRAINT fk_categories_content_types_content_type_id FOREIGN KEY (content_type_id) REFERENCES content.content_types (id) ON DELETE RESTRICT
);

CREATE TABLE content.content_orders (
    id uuid NOT NULL,
    customer_id uuid NOT NULL,
    package_id uuid,
    total_amount_usd numeric(10,2) NOT NULL,
    status integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_content_orders PRIMARY KEY (id),
    CONSTRAINT fk_content_orders_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers (id) ON DELETE RESTRICT,
    CONSTRAINT fk_content_orders_packages_package_id FOREIGN KEY (package_id) REFERENCES content.packages (id) ON DELETE RESTRICT
);

CREATE TABLE content.articles (
    id uuid NOT NULL,
    customer_id uuid,
    order_item_id uuid,
    category_id uuid NOT NULL,
    title character varying(100) NOT NULL,
    slug character varying(220) NOT NULL,
    headline character varying(300) NOT NULL DEFAULT '',
    body text NOT NULL DEFAULT '',
    cover_image_url character varying(500),
    author_id uuid NOT NULL,
    social_boost boolean NOT NULL DEFAULT FALSE,
    is_featured boolean NOT NULL DEFAULT FALSE,
    featured_until timestamp with time zone,
    status text NOT NULL DEFAULT 'Draft',
    rejection_reason character varying(500),
    published_at timestamp with time zone,
    meta_title character varying(70),
    meta_description character varying(160),
    like_count integer NOT NULL DEFAULT 0,
    comment_count integer NOT NULL DEFAULT 0,
    share_count integer NOT NULL DEFAULT 0,
    bookmark_count integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_articles PRIMARY KEY (id),
    CONSTRAINT fk_articles_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_articles_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers (id) ON DELETE SET NULL
);

CREATE TABLE content.category_pricing (
    id uuid NOT NULL,
    category_id uuid NOT NULL,
    pricing_tier_id uuid NOT NULL,
    price_usd numeric(10,2) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_category_pricing PRIMARY KEY (id),
    CONSTRAINT fk_category_pricing_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE CASCADE,
    CONSTRAINT fk_category_pricing_pricing_tiers_pricing_tier_id FOREIGN KEY (pricing_tier_id) REFERENCES content.pricing_tiers (id) ON DELETE RESTRICT
);

CREATE TABLE content.package_slots (
    id uuid NOT NULL,
    package_id uuid NOT NULL,
    category_id uuid,
    is_required boolean NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_package_slots PRIMARY KEY (id),
    CONSTRAINT fk_package_slots_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE SET NULL,
    CONSTRAINT fk_package_slots_packages_package_id FOREIGN KEY (package_id) REFERENCES content.packages (id) ON DELETE CASCADE
);

CREATE TABLE content.videos (
    id uuid NOT NULL,
    customer_id uuid,
    order_item_id uuid,
    category_id uuid NOT NULL,
    author_id uuid NOT NULL,
    title character varying(100) NOT NULL,
    slug character varying(220) NOT NULL,
    description text NOT NULL,
    thumbnail_url character varying(500),
    thumbnail_storage_key text,
    youtube_video_id character varying(20),
    social_boost boolean NOT NULL DEFAULT FALSE,
    is_featured boolean NOT NULL DEFAULT FALSE,
    featured_until timestamp with time zone,
    has_lyrics boolean NOT NULL DEFAULT FALSE,
    status text NOT NULL DEFAULT 'Draft',
    rejection_reason character varying(500),
    shooting_scheduled_at timestamp with time zone,
    published_at timestamp with time zone,
    meta_title character varying(70),
    meta_description character varying(160),
    rating_average numeric(3,2) NOT NULL DEFAULT 0.0,
    rating_count integer NOT NULL DEFAULT 0,
    share_count integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_videos PRIMARY KEY (id),
    CONSTRAINT fk_videos_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_videos_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers (id) ON DELETE SET NULL
);

CREATE TABLE content.content_order_items (
    id uuid NOT NULL,
    order_id uuid NOT NULL,
    content_kind integer NOT NULL,
    category_id uuid NOT NULL,
    promotion_level_id uuid,
    promo_price_snapshot_usd numeric(10,2),
    social_boost boolean NOT NULL,
    is_bonus boolean NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_content_order_items PRIMARY KEY (id),
    CONSTRAINT fk_content_order_items_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_content_order_items_content_orders_order_id FOREIGN KEY (order_id) REFERENCES content.content_orders (id) ON DELETE CASCADE,
    CONSTRAINT fk_content_order_items_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels (id) ON DELETE RESTRICT
);

CREATE TABLE content.content_payments (
    id uuid NOT NULL,
    order_id uuid NOT NULL,
    amount_usd numeric(10,2) NOT NULL,
    payment_method integer,
    payment_proof_file_id uuid,
    status integer NOT NULL,
    verified_by_id uuid,
    verified_at timestamp with time zone,
    receipt_url character varying(500),
    notes character varying(1000),
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_content_payments PRIMARY KEY (id),
    CONSTRAINT fk_content_payments_content_orders_order_id FOREIGN KEY (order_id) REFERENCES content.content_orders (id) ON DELETE CASCADE
);

CREATE TABLE content.article_bookmarks (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_bookmarks PRIMARY KEY (id),
    CONSTRAINT fk_article_bookmarks_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE
);

CREATE TABLE content.article_comments (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    body character varying(1000) NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_comments PRIMARY KEY (id),
    CONSTRAINT fk_article_comments_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE
);

CREATE TABLE content.article_images (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    storage_key text NOT NULL,
    url character varying(500) NOT NULL,
    image_type text NOT NULL DEFAULT 'Cover',
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_images PRIMARY KEY (id),
    CONSTRAINT fk_article_images_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE
);

CREATE TABLE content.article_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_likes PRIMARY KEY (id),
    CONSTRAINT fk_article_likes_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE
);

CREATE TABLE content.article_shares (
    id uuid NOT NULL,
    user_id uuid,
    article_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_shares PRIMARY KEY (id),
    CONSTRAINT fk_article_shares_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE
);

CREATE TABLE content.article_tags (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_tags PRIMARY KEY (id),
    CONSTRAINT fk_article_tags_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE,
    CONSTRAINT fk_article_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics (
    id uuid NOT NULL,
    author_id uuid NOT NULL,
    video_id uuid,
    song_title character varying(200) NOT NULL,
    artist_name character varying(100) NOT NULL,
    lyrics_text text NOT NULL,
    language character varying(5) NOT NULL DEFAULT 'fr',
    meta_title character varying(70),
    meta_description character varying(160),
    meta_keywords character varying(300),
    structured_data jsonb,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE SET NULL
);

CREATE TABLE content.playlist_videos (
    id uuid NOT NULL,
    playlist_id uuid NOT NULL,
    video_id uuid NOT NULL,
    sort_order integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_playlist_videos PRIMARY KEY (id),
    CONSTRAINT fk_playlist_videos_playlists_playlist_id FOREIGN KEY (playlist_id) REFERENCES content.playlists (id) ON DELETE CASCADE,
    CONSTRAINT fk_playlist_videos_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE CASCADE
);

CREATE TABLE content.short_videos (
    id uuid NOT NULL,
    title character varying(200) NOT NULL,
    slug character varying(220) NOT NULL,
    video_url character varying(500) NOT NULL,
    video_storage_key text NOT NULL,
    thumbnail_url character varying(500),
    thumbnail_storage_key text,
    video_id uuid,
    has_full_video boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    view_count integer NOT NULL DEFAULT 0,
    like_count integer NOT NULL DEFAULT 0,
    share_count integer NOT NULL DEFAULT 0,
    bookmark_count integer NOT NULL DEFAULT 0,
    author_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_short_videos PRIMARY KEY (id),
    CONSTRAINT fk_short_videos_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE SET NULL
);

CREATE TABLE content.video_ratings (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    video_id uuid NOT NULL,
    stars smallint NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_video_ratings PRIMARY KEY (id),
    CONSTRAINT fk_video_ratings_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE CASCADE
);

CREATE TABLE content.video_shares (
    id uuid NOT NULL,
    user_id uuid,
    video_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_video_shares PRIMARY KEY (id),
    CONSTRAINT fk_video_shares_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE CASCADE
);

CREATE TABLE content.video_tags (
    id uuid NOT NULL,
    video_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_video_tags PRIMARY KEY (id),
    CONSTRAINT fk_video_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags (id) ON DELETE CASCADE,
    CONSTRAINT fk_video_tags_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos (id) ON DELETE CASCADE
);

CREATE TABLE content.content_item_tiers (
    id uuid NOT NULL,
    order_item_id uuid NOT NULL,
    pricing_tier_id uuid NOT NULL,
    price_snapshot_usd numeric(10,2) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_content_item_tiers PRIMARY KEY (id),
    CONSTRAINT fk_content_item_tiers_content_order_items_order_item_id FOREIGN KEY (order_item_id) REFERENCES content.content_order_items (id) ON DELETE CASCADE,
    CONSTRAINT fk_content_item_tiers_pricing_tiers_pricing_tier_id FOREIGN KEY (pricing_tier_id) REFERENCES content.pricing_tiers (id) ON DELETE RESTRICT
);

CREATE TABLE content.short_video_bookmarks (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_short_video_bookmarks PRIMARY KEY (id),
    CONSTRAINT fk_short_video_bookmarks_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos (id) ON DELETE CASCADE
);

CREATE TABLE content.short_video_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_short_video_likes PRIMARY KEY (id),
    CONSTRAINT fk_short_video_likes_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos (id) ON DELETE CASCADE
);

CREATE TABLE content.short_video_shares (
    id uuid NOT NULL,
    user_id uuid,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_short_video_shares PRIMARY KEY (id),
    CONSTRAINT fk_short_video_shares_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos (id) ON DELETE CASCADE
);

CREATE INDEX ix_article_bookmarks_article_id ON content.article_bookmarks (article_id);

CREATE UNIQUE INDEX ix_article_bookmarks_user_id_article_id ON content.article_bookmarks (user_id, article_id);

CREATE INDEX ix_article_comments_article ON content.article_comments (article_id);

CREATE INDEX ix_article_images_article_id ON content.article_images (article_id);

CREATE INDEX ix_article_likes_article_id ON content.article_likes (article_id);

CREATE UNIQUE INDEX ix_article_likes_user_id_article_id ON content.article_likes (user_id, article_id);

CREATE INDEX ix_article_shares_article_id ON content.article_shares (article_id);

CREATE UNIQUE INDEX ix_article_tags_article_id_tag_id ON content.article_tags (article_id, tag_id);

CREATE INDEX ix_article_tags_tag_id ON content.article_tags (tag_id);

CREATE INDEX ix_articles_category_id ON content.articles (category_id);

CREATE INDEX ix_articles_customer_id ON content.articles (customer_id);

CREATE UNIQUE INDEX ix_articles_slug ON content.articles (slug);

CREATE UNIQUE INDEX ix_articles_title ON content.articles (title);

CREATE INDEX ix_categories_content_type_id ON content.categories (content_type_id);

CREATE UNIQUE INDEX ix_categories_name ON content.categories (name);

CREATE UNIQUE INDEX ix_categories_slug ON content.categories (slug);

CREATE INDEX ix_category_pricing_pricing_tier_id ON content.category_pricing (pricing_tier_id);

CREATE UNIQUE INDEX uq_category_pricing_category_tier ON content.category_pricing (category_id, pricing_tier_id);

CREATE INDEX ix_content_item_tiers_order_item_id ON content.content_item_tiers (order_item_id);

CREATE INDEX ix_content_item_tiers_pricing_tier_id ON content.content_item_tiers (pricing_tier_id);

CREATE INDEX ix_content_order_items_category_id ON content.content_order_items (category_id);

CREATE INDEX ix_content_order_items_order_id ON content.content_order_items (order_id);

CREATE INDEX ix_content_order_items_promotion_level_id ON content.content_order_items (promotion_level_id);

CREATE INDEX ix_content_orders_customer_id ON content.content_orders (customer_id);

CREATE INDEX ix_content_orders_package_id ON content.content_orders (package_id);

CREATE UNIQUE INDEX ix_content_payments_order_id ON content.content_payments (order_id);

CREATE UNIQUE INDEX ix_content_types_name ON content.content_types (name);

CREATE UNIQUE INDEX ix_customers_email ON content.customers (email);

CREATE INDEX ix_lyrics_video_id ON content.lyrics (video_id);

CREATE INDEX ix_package_slots_category_id ON content.package_slots (category_id);

CREATE INDEX ix_package_slots_package_id ON content.package_slots (package_id);

CREATE UNIQUE INDEX ix_playlist_videos_playlist_id_video_id ON content.playlist_videos (playlist_id, video_id);

CREATE INDEX ix_playlist_videos_video_id ON content.playlist_videos (video_id);

CREATE INDEX ix_playlists_user ON content.playlists (user_id);

CREATE UNIQUE INDEX ix_pricing_tiers_name ON content.pricing_tiers (name);

CREATE UNIQUE INDEX ix_promotion_levels_name ON content.promotion_levels (name);

CREATE INDEX ix_short_video_bookmarks_short_video_id ON content.short_video_bookmarks (short_video_id);

CREATE UNIQUE INDEX ix_short_video_bookmarks_user_id_short_video_id ON content.short_video_bookmarks (user_id, short_video_id);

CREATE INDEX ix_short_video_likes_short_video_id ON content.short_video_likes (short_video_id);

CREATE UNIQUE INDEX ix_short_video_likes_user_id_short_video_id ON content.short_video_likes (user_id, short_video_id);

CREATE INDEX ix_short_video_shares_short_video_id ON content.short_video_shares (short_video_id);

CREATE UNIQUE INDEX ix_short_videos_slug ON content.short_videos (slug);

CREATE UNIQUE INDEX ix_short_videos_title ON content.short_videos (title);

CREATE INDEX ix_short_videos_video_id ON content.short_videos (video_id);

CREATE UNIQUE INDEX ix_tags_name ON content.tags (name);

CREATE UNIQUE INDEX ix_tags_slug ON content.tags (slug);

CREATE UNIQUE INDEX ix_video_ratings_user_video ON content.video_ratings (user_id, video_id);

CREATE INDEX ix_video_ratings_video_id ON content.video_ratings (video_id);

CREATE INDEX ix_video_shares_video_id ON content.video_shares (video_id);

CREATE INDEX ix_video_tags_tag_id ON content.video_tags (tag_id);

CREATE UNIQUE INDEX ix_video_tags_video_id_tag_id ON content.video_tags (video_id, tag_id);

CREATE INDEX ix_videos_category_id ON content.videos (category_id);

CREATE INDEX ix_videos_customer_id ON content.videos (customer_id);

CREATE UNIQUE INDEX ix_videos_slug ON content.videos (slug);

CREATE UNIQUE INDEX ix_videos_title ON content.videos (title);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260512213038_InitContentSchema', '9.0.4');

ALTER TABLE content.packages DROP COLUMN flat_price_usd;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260517173714_RemovePackageFlatPriceUsd', '9.0.4');

ALTER TABLE content.videos DROP COLUMN youtube_video_id;

ALTER TABLE content.videos ADD youtube_video_url character varying(200);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260519162512_UpdateYoutubeIdToFullUrl', '9.0.4');

ALTER TABLE content.articles ALTER COLUMN headline TYPE character varying(500);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260522123052_IncreaseArticleHeadlineMaxLength', '9.0.4');

ALTER TABLE content.lyrics DROP COLUMN meta_keywords;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260524105213_RemoveMetaKeywordsFromLyrics', '9.0.4');

ALTER TABLE content.videos RENAME COLUMN is_featured TO is_promoted;

ALTER TABLE content.videos RENAME COLUMN featured_until TO unpromoted_at;

ALTER TABLE content.articles RENAME COLUMN is_featured TO is_promoted;

ALTER TABLE content.articles RENAME COLUMN featured_until TO unpromoted_at;

ALTER TABLE content.videos ADD promoted_until timestamp with time zone;

ALTER TABLE content.videos ADD unpromoted_by text;

ALTER TABLE content.videos ADD unpromoted_reason character varying(500);

ALTER TABLE content.articles ADD promoted_until timestamp with time zone;

ALTER TABLE content.articles ADD unpromoted_by text;

ALTER TABLE content.articles ADD unpromoted_reason character varying(500);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260531212631_IntroducePromotionFieldsAndForceUnpromoteAudit', '9.0.4');

ALTER TABLE content.videos ADD promotion_level_id uuid;

ALTER TABLE content.promotion_levels ADD spot_priority integer;

ALTER TABLE content.categories ADD is_gossip_fallback boolean NOT NULL DEFAULT FALSE;

ALTER TABLE content.articles ADD promotion_level_id uuid;

CREATE INDEX ix_videos_promotion_level_id ON content.videos (promotion_level_id);

ALTER TABLE content.promotion_levels ADD CONSTRAINT ck_promotion_levels_spot_priority CHECK (spot_priority IS NULL OR spot_priority IN (1, 2, 3));

CREATE UNIQUE INDEX ix_categories_is_gossip_fallback ON content.categories (is_gossip_fallback) WHERE is_gossip_fallback = true;

CREATE INDEX ix_articles_promotion_level_id ON content.articles (promotion_level_id);

ALTER TABLE content.articles ADD CONSTRAINT fk_articles_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels (id) ON DELETE SET NULL;

ALTER TABLE content.videos ADD CONSTRAINT fk_videos_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260604122849_AddSpotPriorityAndPromotionFeedSchema', '9.0.4');

ALTER TABLE content.videos DROP COLUMN thumbnail_storage_key;

ALTER TABLE content.videos DROP COLUMN thumbnail_url;

ALTER TABLE content.short_videos DROP COLUMN thumbnail_storage_key;

ALTER TABLE content.short_videos DROP COLUMN thumbnail_url;

ALTER TABLE content.short_videos DROP COLUMN video_storage_key;

ALTER TABLE content.short_videos DROP COLUMN video_url;

ALTER TABLE content.articles DROP COLUMN cover_image_url;

ALTER TABLE content.videos ADD thumbnail_file_id uuid;

ALTER TABLE content.short_videos ADD thumbnail_file_id uuid;

ALTER TABLE content.short_videos ADD video_file_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE content.articles ADD cover_image_file_id uuid;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260618215539_AddFileIdColumnsToContentEntities', '9.0.4');

ALTER TABLE content.categories ADD is_exclusive boolean NOT NULL DEFAULT FALSE;

ALTER TABLE content.categories ADD poster_file_id uuid;

CREATE UNIQUE INDEX ix_categories_is_exclusive ON content.categories (is_exclusive) WHERE is_exclusive = true;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260619115234_AddPosterAndExclusiveToCategory', '9.0.4');

ALTER TABLE content.categories ADD pinned_to_feed_at timestamp with time zone;

CREATE INDEX ix_categories_pinned_to_feed_at ON content.categories (pinned_to_feed_at) WHERE pinned_to_feed_at IS NOT NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260623195125_AddPinnedToFeedToCategory', '9.0.4');

ALTER TABLE content.short_videos ALTER COLUMN video_file_id DROP NOT NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260624151354_MakeShortVideoFileIdNullable', '9.0.4');

ALTER TABLE content.article_comments ADD like_count integer NOT NULL DEFAULT 0;

ALTER TABLE content.article_comments ADD parent_comment_id uuid;

CREATE TABLE content.article_comment_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    comment_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_comment_likes PRIMARY KEY (id),
    CONSTRAINT fk_article_comment_likes_article_comments_comment_id FOREIGN KEY (comment_id) REFERENCES content.article_comments (id) ON DELETE CASCADE
);

CREATE INDEX ix_article_comments_parent ON content.article_comments (parent_comment_id);

CREATE UNIQUE INDEX ix_article_comment_likes_comment_user ON content.article_comment_likes (comment_id, user_id);

ALTER TABLE content.article_comments ADD CONSTRAINT fk_article_comments_article_comments_parent_comment_id FOREIGN KEY (parent_comment_id) REFERENCES content.article_comments (id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260705193231_AddArticleCommentThreadingAndLikes', '9.0.4');

ALTER TABLE content.video_shares ADD platform character varying(50);

ALTER TABLE content.short_video_shares ADD platform character varying(50);

ALTER TABLE content.article_shares ADD platform character varying(50);

CREATE TABLE content.short_video_view_events (
    id uuid NOT NULL,
    short_video_id uuid NOT NULL,
    user_id uuid,
    dedup_key character varying(100) NOT NULL,
    ip_address character varying(64),
    user_agent character varying(500),
    is_counted boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_short_video_view_events PRIMARY KEY (id),
    CONSTRAINT fk_short_video_view_events_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos (id) ON DELETE CASCADE
);

CREATE INDEX ix_short_video_view_events_short_video_id_dedup_key_created_at ON content.short_video_view_events (short_video_id, dedup_key, created_at);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260715152320_AddSharePlatformAndShortVideoViewEvents', '9.0.4');

ALTER TABLE content.video_shares RENAME COLUMN platform TO share_channel;

ALTER TABLE content.short_video_shares RENAME COLUMN platform TO share_channel;

ALTER TABLE content.article_shares RENAME COLUMN platform TO share_channel;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260716132227_RenameSharePlatformToShareChannel', '9.0.4');

CREATE INDEX ix_short_videos_is_active_created_at ON content.short_videos (is_active, created_at);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260716170722_AddShortVideoActiveCreatedAtIndex', '9.0.4');

ALTER TABLE content.short_videos ADD feed_rank bigint NOT NULL DEFAULT 0;

UPDATE content.short_videos SET feed_rank = ('x' || substr(md5(id::text), 1, 16))::bit(64)::bigint;

CREATE UNIQUE INDEX ix_short_videos_feed_rank ON content.short_videos (feed_rank);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260717175525_AddShortVideoFeedRank', '9.0.4');

CREATE INDEX ix_video_shares_user_created_video ON content.video_shares (user_id, created_at DESC, video_id) WHERE user_id IS NOT NULL;

CREATE INDEX ix_short_video_shares_user_created_short ON content.short_video_shares (user_id, created_at DESC, short_video_id) WHERE user_id IS NOT NULL;

CREATE INDEX ix_article_shares_user_created_article ON content.article_shares (user_id, created_at DESC, article_id) WHERE user_id IS NOT NULL;

CREATE INDEX ix_article_comments_user_deleted_created_article ON content.article_comments (user_id, is_deleted, created_at DESC, article_id);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260718225835_AddFavoriteCollectionReadIndexes', '9.0.4');

ALTER TABLE content.lyrics ADD category_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE content.lyrics ADD customer_id uuid;

ALTER TABLE content.lyrics ADD order_item_id uuid;

ALTER TABLE content.lyrics ADD published_at timestamp with time zone;

ALTER TABLE content.lyrics ADD rejection_reason character varying(500);

ALTER TABLE content.lyrics ADD slug character varying(220) NOT NULL DEFAULT '';

ALTER TABLE content.lyrics ADD status text NOT NULL DEFAULT 'Draft';

UPDATE content.lyrics SET category_id = (SELECT id FROM content.categories ORDER BY id LIMIT 1) WHERE category_id = '00000000-0000-0000-0000-000000000000' AND EXISTS (SELECT 1 FROM content.categories);

UPDATE content.lyrics SET slug = trim(both '-' from regexp_replace(lower(song_title || '-' || artist_name), '[^a-z0-9]+', '-', 'g')) || '-' || substr(id::text, 1, 8) WHERE slug = '';

CREATE INDEX ix_lyrics_category_id ON content.lyrics (category_id);

CREATE INDEX ix_lyrics_customer_id ON content.lyrics (customer_id);

CREATE UNIQUE INDEX ix_lyrics_slug ON content.lyrics (slug);

ALTER TABLE content.lyrics ADD CONSTRAINT fk_lyrics_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories (id) ON DELETE RESTRICT;

ALTER TABLE content.lyrics ADD CONSTRAINT fk_lyrics_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260729161151_AddLyricsSlugCategoryAndEditorialWorkflow', '9.0.4');

ALTER TABLE content.lyrics ADD album character varying(200);

ALTER TABLE content.lyrics ADD cover_image_file_id uuid;

ALTER TABLE content.lyrics ADD label character varying(100);

ALTER TABLE content.lyrics ADD producer character varying(100);

ALTER TABLE content.lyrics ADD release_year smallint;

ALTER TABLE content.lyrics ADD songwriter character varying(100);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260729173306_AddSongMetadataAndCoverToLyrics', '9.0.4');

CREATE TABLE content.lyrics_tags (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_tags PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_tags_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE,
    CONSTRAINT fk_lyrics_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_lyrics_tags_lyrics_id_tag_id ON content.lyrics_tags (lyrics_id, tag_id);

CREATE INDEX ix_lyrics_tags_tag_id ON content.lyrics_tags (tag_id);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260729173349_AddLyricsTags', '9.0.4');

ALTER TABLE content.videos ADD artist_id uuid;

ALTER TABLE content.lyrics ADD album_id uuid;

ALTER TABLE content.lyrics ADD artist_id uuid;

CREATE TABLE content.artists (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    slug character varying(220) NOT NULL,
    bio text,
    avatar_file_id uuid,
    user_id uuid,
    verified_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_artists PRIMARY KEY (id)
);

CREATE TABLE content.albums (
    id uuid NOT NULL,
    name character varying(200) NOT NULL,
    artist_id uuid,
    cover_image_file_id uuid,
    release_year smallint,
    label character varying(100),
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_albums PRIMARY KEY (id),
    CONSTRAINT fk_albums_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE SET NULL
);

CREATE INDEX ix_videos_artist_id ON content.videos (artist_id);

CREATE INDEX ix_lyrics_album_id ON content.lyrics (album_id);

CREATE INDEX ix_lyrics_artist_id ON content.lyrics (artist_id);

CREATE INDEX ix_albums_artist_id ON content.albums (artist_id);

CREATE UNIQUE INDEX ix_artists_slug ON content.artists (slug);

CREATE UNIQUE INDEX ix_artists_user_id ON content.artists (user_id) WHERE user_id IS NOT NULL;

ALTER TABLE content.lyrics ADD CONSTRAINT fk_lyrics_albums_album_id FOREIGN KEY (album_id) REFERENCES content.albums (id) ON DELETE SET NULL;

ALTER TABLE content.lyrics ADD CONSTRAINT fk_lyrics_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE SET NULL;

ALTER TABLE content.videos ADD CONSTRAINT fk_videos_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260730091445_AddArtistEntityAndLinks', '9.0.4');

CREATE TABLE content.streaming_links (
    id uuid NOT NULL,
    album_id uuid,
    lyrics_id uuid,
    platform integer NOT NULL,
    url character varying(500) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_streaming_links PRIMARY KEY (id),
    CONSTRAINT ck_streaming_links_exactly_one_target CHECK ((album_id IS NOT NULL AND lyrics_id IS NULL) OR (album_id IS NULL AND lyrics_id IS NOT NULL)),
    CONSTRAINT fk_streaming_links_albums_album_id FOREIGN KEY (album_id) REFERENCES content.albums (id) ON DELETE CASCADE,
    CONSTRAINT fk_streaming_links_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_streaming_links_album_id_platform ON content.streaming_links (album_id, platform);

CREATE UNIQUE INDEX ix_streaming_links_lyrics_id_platform ON content.streaming_links (lyrics_id, platform);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260730171346_AddStreamingLinksAndAlbumLink', '9.0.4');

ALTER TABLE content.lyrics ADD is_promoted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE content.lyrics ADD promoted_until timestamp with time zone;

ALTER TABLE content.lyrics ADD unpromoted_at timestamp with time zone;

ALTER TABLE content.lyrics ADD unpromoted_by text;

ALTER TABLE content.lyrics ADD unpromoted_reason character varying(500);

ALTER TABLE content.categories ADD is_default_for_lyrics boolean NOT NULL DEFAULT FALSE;

CREATE UNIQUE INDEX ix_categories_is_default_for_lyrics ON content.categories (is_default_for_lyrics) WHERE is_default_for_lyrics = true;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260730171426_AddLyricsPromotedPlacement', '9.0.4');

ALTER TABLE content.lyrics ADD like_count integer NOT NULL DEFAULT 0;

ALTER TABLE content.lyrics ADD share_count integer NOT NULL DEFAULT 0;

ALTER TABLE content.lyrics ADD view_count integer NOT NULL DEFAULT 0;

CREATE TABLE content.lyrics_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_likes PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_likes_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics_shares (
    id uuid NOT NULL,
    user_id uuid,
    lyrics_id uuid NOT NULL,
    share_channel character varying(50),
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_shares PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_shares_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics_view_events (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    user_id uuid,
    dedup_key character varying(100) NOT NULL,
    ip_address character varying(64),
    user_agent character varying(500),
    is_counted boolean NOT NULL,
    dwell_ms integer NOT NULL,
    scroll_depth_ratio double precision NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_view_events PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_view_events_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE INDEX ix_lyrics_likes_lyrics_id ON content.lyrics_likes (lyrics_id);

CREATE UNIQUE INDEX ix_lyrics_likes_user_id_lyrics_id ON content.lyrics_likes (user_id, lyrics_id);

CREATE INDEX ix_lyrics_shares_lyrics_id ON content.lyrics_shares (lyrics_id);

CREATE INDEX ix_lyrics_shares_user_created_lyrics ON content.lyrics_shares (user_id, created_at DESC, lyrics_id) WHERE user_id IS NOT NULL;

CREATE INDEX ix_lyrics_view_events_lyrics_id_dedup_key_created_at ON content.lyrics_view_events (lyrics_id, dedup_key, created_at);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260731102102_AddLyricsInteractions', '9.0.4');

CREATE TABLE content.lyrics_translations (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    language text NOT NULL,
    text text NOT NULL,
    source text NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_translations PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_translations_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics_translation_revisions (
    id uuid NOT NULL,
    translation_id uuid NOT NULL,
    proposed_text text NOT NULL,
    edit_summary text,
    proposed_by_user_id uuid NOT NULL,
    status text NOT NULL DEFAULT 'Pending',
    decided_by_user_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_translation_revisions PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_translation_revisions_lyrics_translations_translatio FOREIGN KEY (translation_id) REFERENCES content.lyrics_translations (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics_translation_votes (
    id uuid NOT NULL,
    revision_id uuid NOT NULL,
    user_id uuid NOT NULL,
    vote text NOT NULL,
    comment text,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_translation_votes PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_translation_votes_lyrics_translation_revisions_revis FOREIGN KEY (revision_id) REFERENCES content.lyrics_translation_revisions (id) ON DELETE CASCADE
);

CREATE INDEX ix_lyrics_translation_revisions_translation_id ON content.lyrics_translation_revisions (translation_id);

CREATE UNIQUE INDEX ix_lyrics_translation_votes_revision_id_user_id ON content.lyrics_translation_votes (revision_id, user_id);

CREATE UNIQUE INDEX ix_lyrics_translations_lyrics_id_language ON content.lyrics_translations (lyrics_id, language);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260731115203_AddLyricsTranslationsAndReview', '9.0.4');

CREATE TABLE content.lyrics_revisions (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    proposed_text text NOT NULL,
    edit_summary text,
    proposed_by_user_id uuid NOT NULL,
    status text NOT NULL DEFAULT 'Pending',
    decided_by_user_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_revisions PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_revisions_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics (id) ON DELETE CASCADE
);

CREATE TABLE content.lyrics_submissions (
    id uuid NOT NULL,
    song_title character varying(200) NOT NULL,
    artist_name character varying(100) NOT NULL,
    lyrics_text text NOT NULL,
    language character varying(5) NOT NULL,
    submitted_by_user_id uuid NOT NULL,
    status text NOT NULL DEFAULT 'Pending',
    reviewed_by_user_id uuid,
    review_note text,
    published_lyrics_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_submissions PRIMARY KEY (id)
);

CREATE TABLE content.lyrics_revision_votes (
    id uuid NOT NULL,
    revision_id uuid NOT NULL,
    user_id uuid NOT NULL,
    vote text NOT NULL,
    comment text,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_lyrics_revision_votes PRIMARY KEY (id),
    CONSTRAINT fk_lyrics_revision_votes_lyrics_revisions_revision_id FOREIGN KEY (revision_id) REFERENCES content.lyrics_revisions (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_lyrics_revision_votes_revision_id_user_id ON content.lyrics_revision_votes (revision_id, user_id);

CREATE INDEX ix_lyrics_revisions_lyrics_id ON content.lyrics_revisions (lyrics_id);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260731115247_AddLyricsSubmissionsAndRevisions', '9.0.4');

DROP INDEX content.ix_videos_artist_id;

DROP INDEX content.ix_lyrics_artist_id;

DROP INDEX content.ix_albums_artist_id;

ALTER TABLE content.artists ADD aliases text[] NOT NULL DEFAULT ('{}'::text[]);

ALTER TABLE content.artists ADD birthdate date;

ALTER TABLE content.artists ADD hometown character varying(120);

ALTER TABLE content.artists ADD initial_letter character varying(1) NOT NULL DEFAULT '';

ALTER TABLE content.artists ADD name_folded character varying(100) NOT NULL DEFAULT '';

ALTER TABLE content.artists ADD real_name character varying(150);

ALTER TABLE content.albums ADD release_type integer NOT NULL DEFAULT 0;

CREATE TABLE content.article_artists (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    artist_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_article_artists PRIMARY KEY (id),
    CONSTRAINT fk_article_artists_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles (id) ON DELETE CASCADE,
    CONSTRAINT fk_article_artists_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE CASCADE
);

CREATE TABLE content.artist_social_links (
    id uuid NOT NULL,
    artist_id uuid NOT NULL,
    platform integer NOT NULL,
    url character varying(500) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_artist_social_links PRIMARY KEY (id),
    CONSTRAINT fk_artist_social_links_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists (id) ON DELETE CASCADE
);

CREATE INDEX ix_videos_artist_id_status ON content.videos (artist_id, status);

CREATE INDEX ix_lyrics_artist_id_status ON content.lyrics (artist_id, status);

CREATE INDEX ix_artists_initial_letter_name_folded ON content.artists (initial_letter, name_folded);

CREATE INDEX ix_artists_name_folded ON content.artists (name_folded);

CREATE INDEX ix_articles_status ON content.articles (status);

CREATE INDEX ix_albums_artist_id_release_type ON content.albums (artist_id, release_type);

CREATE UNIQUE INDEX ix_article_artists_article_id_artist_id ON content.article_artists (article_id, artist_id);

CREATE INDEX ix_article_artists_artist_id ON content.article_artists (artist_id);

CREATE UNIQUE INDEX ix_artist_social_links_artist_id_platform ON content.artist_social_links (artist_id, platform);

UPDATE content.artists
SET name_folded = upper(
        regexp_replace(
            translate(
                trim(name),
                'àáâãäåçèéêëìíîïñòóôõöùúûüýÿÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝŸ',
                'aaaaaaceeeeiiiinooooouuuuyyAAAAAACEEEEIIIINOOOOOUUUUYY'
            ),
            '\s+', ' ', 'g'
        )
    );

UPDATE content.artists
SET initial_letter = CASE
        WHEN name_folded ~ '^[A-Z]' THEN left(name_folded, 1)
        ELSE '#'
    END;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260812163757_AddArtistPageFeature', '9.0.4');

CREATE TABLE content.artist_claim_requests (
    id uuid NOT NULL,
    artist_id uuid NOT NULL,
    user_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_artist_claim_requests PRIMARY KEY (id)
);

CREATE INDEX ix_artist_claim_requests_artist_id ON content.artist_claim_requests (artist_id);

CREATE INDEX ix_artist_claim_requests_user_id ON content.artist_claim_requests (user_id);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260817202830_AddArtistClaimRequests', '9.0.4');

CREATE UNIQUE INDEX ix_artist_claim_requests_artist_id_user_id ON content.artist_claim_requests (artist_id, user_id);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260818094853_AddArtistClaimRequestUniqueIndex', '9.0.4');

COMMIT;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_status_published_at
    ON content.articles (status, published_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_category_status_published_at
    ON content.articles (category_id, status, published_at);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_promoted_published_at
    ON content.articles (published_at DESC)
    WHERE is_promoted = true;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_videos_status_published_at
    ON content.videos (status, published_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_short_video_view_events_uncounted_created_at
    ON content.short_video_view_events (created_at)
    WHERE is_counted = false;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_tags_name_lower
    ON content.tags (LOWER(name));

DROP INDEX CONCURRENTLY IF EXISTS content.ix_articles_category_id;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260906153753_AddContentReadIndexes', '9.0.4');

START TRANSACTION;
CREATE TABLE content.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer NOT NULL DEFAULT 0,
    last_error text,
    CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id)
);

CREATE TABLE content.processed_domain_events (
    event_id uuid NOT NULL,
    handler_name character varying(200) NOT NULL,
    processed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_processed_domain_events PRIMARY KEY (event_id, handler_name)
);

CREATE INDEX ix_domain_event_outbox_pending ON content.domain_event_outbox (occurred_on) WHERE dispatched_at IS NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260909134446_AddDomainEventOutboxAndProcessedEvents', '9.0.4');

DROP TABLE content.processed_domain_events;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260910172727_DropRedundantProcessedDomainEvents', '9.0.4');

COMMIT;

