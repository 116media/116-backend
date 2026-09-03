--
-- PostgreSQL database dump
--

\restrict GY5N81qlbFFYiwZ55USYbNMohA0SGSEpmhLeafzB68u907ycVAmOqDM68KNxfpV

-- Dumped from database version 18.4 (Debian 18.4-1.pgdg13+1)
-- Dumped by pg_dump version 18.4 (Debian 18.4-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: content; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA content;


--
-- Name: core; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA core;


--
-- Name: identity; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA identity;


--
-- Name: mailer; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA mailer;


--
-- Name: quartz; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA quartz;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: albums; Type: TABLE; Schema: content; Owner: -
--

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
    release_type integer DEFAULT 0 NOT NULL
);


--
-- Name: article_artists; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_artists (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    artist_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: article_bookmarks; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_bookmarks (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: article_comment_likes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_comment_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    comment_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: article_comments; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_comments (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    body character varying(1000) NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    deleted_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    like_count integer DEFAULT 0 NOT NULL,
    parent_comment_id uuid
);


--
-- Name: article_images; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_images (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    storage_key text NOT NULL,
    url character varying(500) NOT NULL,
    image_type text DEFAULT 'Cover'::text NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: article_likes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    article_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: article_shares; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_shares (
    id uuid NOT NULL,
    user_id uuid,
    article_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    share_channel character varying(50)
);


--
-- Name: article_tags; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.article_tags (
    id uuid NOT NULL,
    article_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: articles; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.articles (
    id uuid NOT NULL,
    customer_id uuid,
    order_item_id uuid,
    category_id uuid NOT NULL,
    title character varying(100) NOT NULL,
    slug character varying(220) NOT NULL,
    headline character varying(500) DEFAULT ''::character varying NOT NULL,
    body text DEFAULT ''::text NOT NULL,
    author_id uuid NOT NULL,
    social_boost boolean DEFAULT false NOT NULL,
    is_promoted boolean DEFAULT false CONSTRAINT articles_is_featured_not_null NOT NULL,
    unpromoted_at timestamp with time zone,
    status text DEFAULT 'Draft'::text NOT NULL,
    rejection_reason character varying(500),
    published_at timestamp with time zone,
    meta_title character varying(70),
    meta_description character varying(160),
    like_count integer DEFAULT 0 NOT NULL,
    comment_count integer DEFAULT 0 NOT NULL,
    share_count integer DEFAULT 0 NOT NULL,
    bookmark_count integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    promoted_until timestamp with time zone,
    unpromoted_by text,
    unpromoted_reason character varying(500),
    promotion_level_id uuid,
    cover_image_file_id uuid
);


--
-- Name: artist_claim_requests; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.artist_claim_requests (
    id uuid NOT NULL,
    artist_id uuid NOT NULL,
    user_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: artist_social_links; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.artist_social_links (
    id uuid NOT NULL,
    artist_id uuid NOT NULL,
    platform integer NOT NULL,
    url character varying(500) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: artists; Type: TABLE; Schema: content; Owner: -
--

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
    aliases text[] DEFAULT '{}'::text[] NOT NULL,
    birthdate date,
    hometown character varying(120),
    initial_letter character varying(1) DEFAULT ''::character varying NOT NULL,
    name_folded character varying(100) DEFAULT ''::character varying NOT NULL,
    real_name character varying(150)
);


--
-- Name: categories; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.categories (
    id uuid NOT NULL,
    content_type_id uuid NOT NULL,
    name character varying(60) NOT NULL,
    slug character varying(80) NOT NULL,
    description character varying(300) NOT NULL,
    is_free boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    is_gossip_fallback boolean DEFAULT false NOT NULL,
    is_exclusive boolean DEFAULT false NOT NULL,
    poster_file_id uuid,
    pinned_to_feed_at timestamp with time zone,
    is_default_for_lyrics boolean DEFAULT false NOT NULL
);


--
-- Name: category_pricing; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.category_pricing (
    id uuid NOT NULL,
    category_id uuid NOT NULL,
    pricing_tier_id uuid NOT NULL,
    price_usd numeric(10,2) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: content_item_tiers; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.content_item_tiers (
    id uuid NOT NULL,
    order_item_id uuid NOT NULL,
    pricing_tier_id uuid NOT NULL,
    price_snapshot_usd numeric(10,2) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: content_order_items; Type: TABLE; Schema: content; Owner: -
--

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
    updated_by text
);


--
-- Name: content_orders; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.content_orders (
    id uuid NOT NULL,
    customer_id uuid NOT NULL,
    package_id uuid,
    total_amount_usd numeric(10,2) NOT NULL,
    status integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: content_payments; Type: TABLE; Schema: content; Owner: -
--

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
    updated_by text
);


--
-- Name: content_types; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.content_types (
    id uuid NOT NULL,
    name character varying(30) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: customers; Type: TABLE; Schema: content; Owner: -
--

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
    updated_by text
);


--
-- Name: domain_event_outbox; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer DEFAULT 0 NOT NULL,
    last_error text
);


--
-- Name: lyrics; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics (
    id uuid NOT NULL,
    author_id uuid NOT NULL,
    video_id uuid,
    song_title character varying(200) NOT NULL,
    artist_name character varying(100) NOT NULL,
    lyrics_text text NOT NULL,
    language character varying(5) DEFAULT 'fr'::character varying NOT NULL,
    meta_title character varying(70),
    meta_description character varying(160),
    structured_data jsonb,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    category_id uuid DEFAULT '00000000-0000-0000-0000-000000000000'::uuid NOT NULL,
    customer_id uuid,
    order_item_id uuid,
    published_at timestamp with time zone,
    rejection_reason character varying(500),
    slug character varying(220) DEFAULT ''::character varying NOT NULL,
    status text DEFAULT 'Draft'::text NOT NULL,
    album character varying(200),
    cover_image_file_id uuid,
    label character varying(100),
    producer character varying(100),
    release_year smallint,
    songwriter character varying(100),
    album_id uuid,
    artist_id uuid,
    is_promoted boolean DEFAULT false NOT NULL,
    promoted_until timestamp with time zone,
    unpromoted_at timestamp with time zone,
    unpromoted_by text,
    unpromoted_reason character varying(500),
    like_count integer DEFAULT 0 NOT NULL,
    share_count integer DEFAULT 0 NOT NULL,
    view_count integer DEFAULT 0 NOT NULL
);


--
-- Name: lyrics_likes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_revision_votes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_revision_votes (
    id uuid NOT NULL,
    revision_id uuid NOT NULL,
    user_id uuid NOT NULL,
    vote text NOT NULL,
    comment text,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_revisions; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_revisions (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    proposed_text text NOT NULL,
    edit_summary text,
    proposed_by_user_id uuid NOT NULL,
    status text DEFAULT 'Pending'::text NOT NULL,
    decided_by_user_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_shares; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_shares (
    id uuid NOT NULL,
    user_id uuid,
    lyrics_id uuid NOT NULL,
    share_channel character varying(50),
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_submissions; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_submissions (
    id uuid NOT NULL,
    song_title character varying(200) NOT NULL,
    artist_name character varying(100) NOT NULL,
    lyrics_text text NOT NULL,
    language character varying(5) NOT NULL,
    submitted_by_user_id uuid NOT NULL,
    status text DEFAULT 'Pending'::text NOT NULL,
    reviewed_by_user_id uuid,
    review_note text,
    published_lyrics_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_tags; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_tags (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_translation_revisions; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_translation_revisions (
    id uuid NOT NULL,
    translation_id uuid NOT NULL,
    proposed_text text NOT NULL,
    edit_summary text,
    proposed_by_user_id uuid NOT NULL,
    status text DEFAULT 'Pending'::text NOT NULL,
    decided_by_user_id uuid,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_translation_votes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_translation_votes (
    id uuid NOT NULL,
    revision_id uuid NOT NULL,
    user_id uuid NOT NULL,
    vote text NOT NULL,
    comment text,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_translations; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.lyrics_translations (
    id uuid NOT NULL,
    lyrics_id uuid NOT NULL,
    language text NOT NULL,
    text text NOT NULL,
    source text NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: lyrics_view_events; Type: TABLE; Schema: content; Owner: -
--

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
    updated_by text
);


--
-- Name: package_slots; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.package_slots (
    id uuid NOT NULL,
    package_id uuid NOT NULL,
    category_id uuid,
    is_required boolean NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: packages; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.packages (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: playlist_videos; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.playlist_videos (
    id uuid NOT NULL,
    playlist_id uuid NOT NULL,
    video_id uuid NOT NULL,
    sort_order integer NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: playlists; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.playlists (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(100) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: pricing_tiers; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.pricing_tiers (
    id uuid NOT NULL,
    name character varying(40) NOT NULL,
    description character varying(200) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: promotion_levels; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.promotion_levels (
    id uuid NOT NULL,
    name character varying(40) NOT NULL,
    duration_days integer NOT NULL,
    price_usd numeric(10,2) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    spot_priority integer,
    CONSTRAINT ck_promotion_levels_spot_priority CHECK (((spot_priority IS NULL) OR (spot_priority = ANY (ARRAY[1, 2, 3]))))
);


--
-- Name: short_video_bookmarks; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.short_video_bookmarks (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: short_video_likes; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.short_video_likes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: short_video_shares; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.short_video_shares (
    id uuid NOT NULL,
    user_id uuid,
    short_video_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    share_channel character varying(50)
);


--
-- Name: short_video_view_events; Type: TABLE; Schema: content; Owner: -
--

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
    updated_by text
);


--
-- Name: short_videos; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.short_videos (
    id uuid NOT NULL,
    title character varying(200) NOT NULL,
    slug character varying(220) NOT NULL,
    video_id uuid,
    has_full_video boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    view_count integer DEFAULT 0 NOT NULL,
    like_count integer DEFAULT 0 NOT NULL,
    share_count integer DEFAULT 0 NOT NULL,
    bookmark_count integer DEFAULT 0 NOT NULL,
    author_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    thumbnail_file_id uuid,
    video_file_id uuid DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
    feed_rank bigint DEFAULT 0 NOT NULL
);


--
-- Name: streaming_links; Type: TABLE; Schema: content; Owner: -
--

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
    CONSTRAINT ck_streaming_links_exactly_one_target CHECK ((((album_id IS NOT NULL) AND (lyrics_id IS NULL)) OR ((album_id IS NULL) AND (lyrics_id IS NOT NULL))))
);


--
-- Name: tags; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.tags (
    id uuid NOT NULL,
    name character varying(50) NOT NULL,
    slug character varying(60) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: video_ratings; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.video_ratings (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    video_id uuid NOT NULL,
    stars smallint NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: video_shares; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.video_shares (
    id uuid NOT NULL,
    user_id uuid,
    video_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    share_channel character varying(50)
);


--
-- Name: video_tags; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.video_tags (
    id uuid NOT NULL,
    video_id uuid NOT NULL,
    tag_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: videos; Type: TABLE; Schema: content; Owner: -
--

CREATE TABLE content.videos (
    id uuid NOT NULL,
    customer_id uuid,
    order_item_id uuid,
    category_id uuid NOT NULL,
    author_id uuid NOT NULL,
    title character varying(100) NOT NULL,
    slug character varying(220) NOT NULL,
    description text NOT NULL,
    social_boost boolean DEFAULT false NOT NULL,
    is_promoted boolean DEFAULT false CONSTRAINT videos_is_featured_not_null NOT NULL,
    unpromoted_at timestamp with time zone,
    has_lyrics boolean DEFAULT false NOT NULL,
    status text DEFAULT 'Draft'::text NOT NULL,
    rejection_reason character varying(500),
    shooting_scheduled_at timestamp with time zone,
    published_at timestamp with time zone,
    meta_title character varying(70),
    meta_description character varying(160),
    rating_average numeric(3,2) DEFAULT 0.0 NOT NULL,
    rating_count integer DEFAULT 0 NOT NULL,
    share_count integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    youtube_video_url character varying(200),
    promoted_until timestamp with time zone,
    unpromoted_by text,
    unpromoted_reason character varying(500),
    promotion_level_id uuid,
    thumbnail_file_id uuid,
    artist_id uuid
);


--
-- Name: domain_event_outbox; Type: TABLE; Schema: core; Owner: -
--

CREATE TABLE core.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer DEFAULT 0 NOT NULL,
    last_error text
);


--
-- Name: files; Type: TABLE; Schema: core; Owner: -
--

CREATE TABLE core.files (
    id uuid NOT NULL,
    file_name character varying(255) NOT NULL,
    original_file_name character varying(255) NOT NULL,
    mime_type character varying(100) NOT NULL,
    storage_url character varying(2048) NOT NULL,
    size_in_bytes bigint NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    deleted_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    storage_key character varying(100),
    dominant_color_hex character varying(7),
    foreground_color_hex character varying(7),
    claimed_at timestamp with time zone
);


--
-- Name: processed_domain_events; Type: TABLE; Schema: core; Owner: -
--

CREATE TABLE core.processed_domain_events (
    event_id uuid NOT NULL,
    handler_name character varying(200) NOT NULL,
    processed_at timestamp with time zone NOT NULL
);


--
-- Name: domain_event_outbox; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer DEFAULT 0 NOT NULL,
    last_error text
);


--
-- Name: otps; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.otps (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    code_hash character varying(100) CONSTRAINT otps_code_not_null NOT NULL,
    purpose text NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    attempt_count integer DEFAULT 0 NOT NULL,
    is_used boolean DEFAULT false NOT NULL,
    used_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    consumed_at timestamp with time zone
);


--
-- Name: permissions; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.permissions (
    id uuid NOT NULL,
    resource character varying(15) NOT NULL,
    action character varying(15) NOT NULL,
    description character varying(300) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    deleted_at timestamp with time zone,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: role_permissions; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.role_permissions (
    id uuid NOT NULL,
    role_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: roles; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.roles (
    id uuid NOT NULL,
    name character varying(20) NOT NULL,
    description character varying(300) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    deleted_at timestamp with time zone,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: sessions; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.sessions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    device_id character varying(64) NOT NULL,
    refresh_token_hash character varying(500) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    ip_address character varying(45),
    user_agent character varying(500),
    browser text NOT NULL,
    device text NOT NULL,
    platform text NOT NULL,
    client text NOT NULL,
    is_revoked boolean DEFAULT false NOT NULL,
    revoked_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    absolute_expires_at timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL
);


--
-- Name: user_otp_state; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.user_otp_state (
    user_id uuid NOT NULL,
    failed_attempts integer DEFAULT 0 NOT NULL,
    locked_until timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: user_roles; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.user_roles (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: user_token_state; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.user_token_state (
    user_id uuid NOT NULL,
    security_stamp uuid NOT NULL,
    token_version bigint DEFAULT 0 NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: users; Type: TABLE; Schema: identity; Owner: -
--

CREATE TABLE identity.users (
    id uuid NOT NULL,
    email character varying(254),
    user_name character varying(20) NOT NULL,
    password_hash text,
    auth_provider text NOT NULL,
    is_verified boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    avatar_file_id uuid,
    avatar_source text NOT NULL,
    country_name character varying(100),
    country_iso_code character varying(3),
    country_dial_code character varying(10),
    partial_phone_number character varying(50),
    full_phone_number character varying(20),
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    provider_subject_id character varying(255),
    failed_login_attempts integer DEFAULT 0 NOT NULL,
    locked_until timestamp with time zone
);


--
-- Name: domain_event_outbox; Type: TABLE; Schema: mailer; Owner: -
--

CREATE TABLE mailer.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer DEFAULT 0 NOT NULL,
    last_error text
);


--
-- Name: newsletter_subscribers; Type: TABLE; Schema: mailer; Owner: -
--

CREATE TABLE mailer.newsletter_subscribers (
    id uuid NOT NULL,
    email character varying(320) NOT NULL,
    status character varying(30) NOT NULL,
    confirmation_token character varying(64) NOT NULL,
    unsubscribe_token character varying(64) NOT NULL,
    confirmed_at timestamp with time zone,
    unsubscribed_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: notifications; Type: TABLE; Schema: mailer; Owner: -
--

CREATE TABLE mailer.notifications (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    type character varying(50) NOT NULL,
    title character varying(200) NOT NULL,
    body character varying(500) NOT NULL,
    link_path character varying(300),
    read_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text
);


--
-- Name: outbox_emails; Type: TABLE; Schema: mailer; Owner: -
--

CREATE TABLE mailer.outbox_emails (
    id uuid NOT NULL,
    recipient_address character varying(320) NOT NULL,
    recipient_name character varying(200),
    subject character varying(500) NOT NULL,
    html_body text NOT NULL,
    text_body text NOT NULL,
    template character varying(100) NOT NULL,
    status character varying(20) NOT NULL,
    attempt_count integer NOT NULL,
    next_attempt_at timestamp with time zone NOT NULL,
    last_error character varying(1000),
    sent_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    lease_expires_at timestamp with time zone
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);


--
-- Name: qrtz_blob_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_blob_triggers (
    sched_name text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    blob_data bytea
);


--
-- Name: qrtz_calendars; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_calendars (
    sched_name text NOT NULL,
    calendar_name text NOT NULL,
    calendar bytea NOT NULL
);


--
-- Name: qrtz_cron_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_cron_triggers (
    sched_name text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    cron_expression text NOT NULL,
    time_zone_id text
);


--
-- Name: qrtz_fired_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_fired_triggers (
    sched_name text NOT NULL,
    entry_id text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    instance_name text NOT NULL,
    fired_time bigint NOT NULL,
    sched_time bigint NOT NULL,
    priority integer NOT NULL,
    state text NOT NULL,
    job_name text,
    job_group text,
    is_nonconcurrent boolean NOT NULL,
    requests_recovery boolean,
    execution_group character varying(200)
);


--
-- Name: qrtz_job_details; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_job_details (
    sched_name text NOT NULL,
    job_name text NOT NULL,
    job_group text NOT NULL,
    description text,
    job_class_name text NOT NULL,
    is_durable boolean NOT NULL,
    is_nonconcurrent boolean NOT NULL,
    is_update_data boolean NOT NULL,
    requests_recovery boolean NOT NULL,
    job_data bytea
);


--
-- Name: qrtz_locks; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_locks (
    sched_name text NOT NULL,
    lock_name text NOT NULL
);


--
-- Name: qrtz_paused_job_grps; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_paused_job_grps (
    sched_name text NOT NULL,
    job_group text NOT NULL
);


--
-- Name: qrtz_paused_trigger_grps; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_paused_trigger_grps (
    sched_name text NOT NULL,
    trigger_group text NOT NULL
);


--
-- Name: qrtz_scheduler_state; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_scheduler_state (
    sched_name text NOT NULL,
    instance_name text NOT NULL,
    last_checkin_time bigint NOT NULL,
    checkin_interval bigint NOT NULL
);


--
-- Name: qrtz_simple_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_simple_triggers (
    sched_name text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    repeat_count bigint NOT NULL,
    repeat_interval bigint NOT NULL,
    times_triggered bigint NOT NULL
);


--
-- Name: qrtz_simprop_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_simprop_triggers (
    sched_name text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    str_prop_1 text,
    str_prop_2 text,
    str_prop_3 text,
    int_prop_1 integer,
    int_prop_2 integer,
    long_prop_1 bigint,
    long_prop_2 bigint,
    dec_prop_1 numeric,
    dec_prop_2 numeric,
    bool_prop_1 boolean,
    bool_prop_2 boolean,
    time_zone_id text
);


--
-- Name: qrtz_triggers; Type: TABLE; Schema: quartz; Owner: -
--

CREATE TABLE quartz.qrtz_triggers (
    sched_name text NOT NULL,
    trigger_name text NOT NULL,
    trigger_group text NOT NULL,
    job_name text NOT NULL,
    job_group text NOT NULL,
    description text,
    next_fire_time bigint,
    prev_fire_time bigint,
    priority integer,
    trigger_state text NOT NULL,
    trigger_type text NOT NULL,
    start_time bigint NOT NULL,
    end_time bigint,
    calendar_name text,
    misfire_instr smallint,
    misfire_orig_fire_time bigint,
    execution_group character varying(200),
    preferred_node character varying(200),
    preferred_node_auto boolean DEFAULT false NOT NULL,
    retry_policy character varying(250),
    retry_attempt integer,
    job_data bytea
);


--
-- Name: albums pk_albums; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.albums
    ADD CONSTRAINT pk_albums PRIMARY KEY (id);


--
-- Name: article_artists pk_article_artists; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_artists
    ADD CONSTRAINT pk_article_artists PRIMARY KEY (id);


--
-- Name: article_bookmarks pk_article_bookmarks; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_bookmarks
    ADD CONSTRAINT pk_article_bookmarks PRIMARY KEY (id);


--
-- Name: article_comment_likes pk_article_comment_likes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_comment_likes
    ADD CONSTRAINT pk_article_comment_likes PRIMARY KEY (id);


--
-- Name: article_comments pk_article_comments; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_comments
    ADD CONSTRAINT pk_article_comments PRIMARY KEY (id);


--
-- Name: article_images pk_article_images; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_images
    ADD CONSTRAINT pk_article_images PRIMARY KEY (id);


--
-- Name: article_likes pk_article_likes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_likes
    ADD CONSTRAINT pk_article_likes PRIMARY KEY (id);


--
-- Name: article_shares pk_article_shares; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_shares
    ADD CONSTRAINT pk_article_shares PRIMARY KEY (id);


--
-- Name: article_tags pk_article_tags; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_tags
    ADD CONSTRAINT pk_article_tags PRIMARY KEY (id);


--
-- Name: articles pk_articles; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.articles
    ADD CONSTRAINT pk_articles PRIMARY KEY (id);


--
-- Name: artist_claim_requests pk_artist_claim_requests; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.artist_claim_requests
    ADD CONSTRAINT pk_artist_claim_requests PRIMARY KEY (id);


--
-- Name: artist_social_links pk_artist_social_links; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.artist_social_links
    ADD CONSTRAINT pk_artist_social_links PRIMARY KEY (id);


--
-- Name: artists pk_artists; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.artists
    ADD CONSTRAINT pk_artists PRIMARY KEY (id);


--
-- Name: categories pk_categories; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.categories
    ADD CONSTRAINT pk_categories PRIMARY KEY (id);


--
-- Name: category_pricing pk_category_pricing; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.category_pricing
    ADD CONSTRAINT pk_category_pricing PRIMARY KEY (id);


--
-- Name: content_item_tiers pk_content_item_tiers; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_item_tiers
    ADD CONSTRAINT pk_content_item_tiers PRIMARY KEY (id);


--
-- Name: content_order_items pk_content_order_items; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_order_items
    ADD CONSTRAINT pk_content_order_items PRIMARY KEY (id);


--
-- Name: content_orders pk_content_orders; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_orders
    ADD CONSTRAINT pk_content_orders PRIMARY KEY (id);


--
-- Name: content_payments pk_content_payments; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_payments
    ADD CONSTRAINT pk_content_payments PRIMARY KEY (id);


--
-- Name: content_types pk_content_types; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_types
    ADD CONSTRAINT pk_content_types PRIMARY KEY (id);


--
-- Name: customers pk_customers; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.customers
    ADD CONSTRAINT pk_customers PRIMARY KEY (id);


--
-- Name: domain_event_outbox pk_domain_event_outbox; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.domain_event_outbox
    ADD CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id);


--
-- Name: lyrics pk_lyrics; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT pk_lyrics PRIMARY KEY (id);


--
-- Name: lyrics_likes pk_lyrics_likes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_likes
    ADD CONSTRAINT pk_lyrics_likes PRIMARY KEY (id);


--
-- Name: lyrics_revision_votes pk_lyrics_revision_votes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_revision_votes
    ADD CONSTRAINT pk_lyrics_revision_votes PRIMARY KEY (id);


--
-- Name: lyrics_revisions pk_lyrics_revisions; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_revisions
    ADD CONSTRAINT pk_lyrics_revisions PRIMARY KEY (id);


--
-- Name: lyrics_shares pk_lyrics_shares; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_shares
    ADD CONSTRAINT pk_lyrics_shares PRIMARY KEY (id);


--
-- Name: lyrics_submissions pk_lyrics_submissions; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_submissions
    ADD CONSTRAINT pk_lyrics_submissions PRIMARY KEY (id);


--
-- Name: lyrics_tags pk_lyrics_tags; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_tags
    ADD CONSTRAINT pk_lyrics_tags PRIMARY KEY (id);


--
-- Name: lyrics_translation_revisions pk_lyrics_translation_revisions; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translation_revisions
    ADD CONSTRAINT pk_lyrics_translation_revisions PRIMARY KEY (id);


--
-- Name: lyrics_translation_votes pk_lyrics_translation_votes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translation_votes
    ADD CONSTRAINT pk_lyrics_translation_votes PRIMARY KEY (id);


--
-- Name: lyrics_translations pk_lyrics_translations; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translations
    ADD CONSTRAINT pk_lyrics_translations PRIMARY KEY (id);


--
-- Name: lyrics_view_events pk_lyrics_view_events; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_view_events
    ADD CONSTRAINT pk_lyrics_view_events PRIMARY KEY (id);


--
-- Name: package_slots pk_package_slots; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.package_slots
    ADD CONSTRAINT pk_package_slots PRIMARY KEY (id);


--
-- Name: packages pk_packages; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.packages
    ADD CONSTRAINT pk_packages PRIMARY KEY (id);


--
-- Name: playlist_videos pk_playlist_videos; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.playlist_videos
    ADD CONSTRAINT pk_playlist_videos PRIMARY KEY (id);


--
-- Name: playlists pk_playlists; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.playlists
    ADD CONSTRAINT pk_playlists PRIMARY KEY (id);


--
-- Name: pricing_tiers pk_pricing_tiers; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.pricing_tiers
    ADD CONSTRAINT pk_pricing_tiers PRIMARY KEY (id);


--
-- Name: promotion_levels pk_promotion_levels; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.promotion_levels
    ADD CONSTRAINT pk_promotion_levels PRIMARY KEY (id);


--
-- Name: short_video_bookmarks pk_short_video_bookmarks; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_bookmarks
    ADD CONSTRAINT pk_short_video_bookmarks PRIMARY KEY (id);


--
-- Name: short_video_likes pk_short_video_likes; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_likes
    ADD CONSTRAINT pk_short_video_likes PRIMARY KEY (id);


--
-- Name: short_video_shares pk_short_video_shares; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_shares
    ADD CONSTRAINT pk_short_video_shares PRIMARY KEY (id);


--
-- Name: short_video_view_events pk_short_video_view_events; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_view_events
    ADD CONSTRAINT pk_short_video_view_events PRIMARY KEY (id);


--
-- Name: short_videos pk_short_videos; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_videos
    ADD CONSTRAINT pk_short_videos PRIMARY KEY (id);


--
-- Name: streaming_links pk_streaming_links; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.streaming_links
    ADD CONSTRAINT pk_streaming_links PRIMARY KEY (id);


--
-- Name: tags pk_tags; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.tags
    ADD CONSTRAINT pk_tags PRIMARY KEY (id);


--
-- Name: video_ratings pk_video_ratings; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_ratings
    ADD CONSTRAINT pk_video_ratings PRIMARY KEY (id);


--
-- Name: video_shares pk_video_shares; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_shares
    ADD CONSTRAINT pk_video_shares PRIMARY KEY (id);


--
-- Name: video_tags pk_video_tags; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_tags
    ADD CONSTRAINT pk_video_tags PRIMARY KEY (id);


--
-- Name: videos pk_videos; Type: CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.videos
    ADD CONSTRAINT pk_videos PRIMARY KEY (id);


--
-- Name: domain_event_outbox pk_domain_event_outbox; Type: CONSTRAINT; Schema: core; Owner: -
--

ALTER TABLE ONLY core.domain_event_outbox
    ADD CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id);


--
-- Name: files pk_files; Type: CONSTRAINT; Schema: core; Owner: -
--

ALTER TABLE ONLY core.files
    ADD CONSTRAINT pk_files PRIMARY KEY (id);


--
-- Name: processed_domain_events pk_processed_domain_events; Type: CONSTRAINT; Schema: core; Owner: -
--

ALTER TABLE ONLY core.processed_domain_events
    ADD CONSTRAINT pk_processed_domain_events PRIMARY KEY (event_id, handler_name);


--
-- Name: domain_event_outbox pk_domain_event_outbox; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.domain_event_outbox
    ADD CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id);


--
-- Name: otps pk_otps; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.otps
    ADD CONSTRAINT pk_otps PRIMARY KEY (id);


--
-- Name: permissions pk_permissions; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.permissions
    ADD CONSTRAINT pk_permissions PRIMARY KEY (id);


--
-- Name: role_permissions pk_role_permissions; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.role_permissions
    ADD CONSTRAINT pk_role_permissions PRIMARY KEY (id);


--
-- Name: roles pk_roles; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.roles
    ADD CONSTRAINT pk_roles PRIMARY KEY (id);


--
-- Name: sessions pk_sessions; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.sessions
    ADD CONSTRAINT pk_sessions PRIMARY KEY (id);


--
-- Name: user_otp_state pk_user_otp_state; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_otp_state
    ADD CONSTRAINT pk_user_otp_state PRIMARY KEY (user_id);


--
-- Name: user_roles pk_user_roles; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_roles
    ADD CONSTRAINT pk_user_roles PRIMARY KEY (id);


--
-- Name: user_token_state pk_user_token_state; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_token_state
    ADD CONSTRAINT pk_user_token_state PRIMARY KEY (user_id);


--
-- Name: users pk_users; Type: CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.users
    ADD CONSTRAINT pk_users PRIMARY KEY (id);


--
-- Name: domain_event_outbox pk_domain_event_outbox; Type: CONSTRAINT; Schema: mailer; Owner: -
--

ALTER TABLE ONLY mailer.domain_event_outbox
    ADD CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id);


--
-- Name: newsletter_subscribers pk_newsletter_subscribers; Type: CONSTRAINT; Schema: mailer; Owner: -
--

ALTER TABLE ONLY mailer.newsletter_subscribers
    ADD CONSTRAINT pk_newsletter_subscribers PRIMARY KEY (id);


--
-- Name: notifications pk_notifications; Type: CONSTRAINT; Schema: mailer; Owner: -
--

ALTER TABLE ONLY mailer.notifications
    ADD CONSTRAINT pk_notifications PRIMARY KEY (id);


--
-- Name: outbox_emails pk_outbox_emails; Type: CONSTRAINT; Schema: mailer; Owner: -
--

ALTER TABLE ONLY mailer.outbox_emails
    ADD CONSTRAINT pk_outbox_emails PRIMARY KEY (id);


--
-- Name: __EFMigrationsHistory pk___ef_migrations_history; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);


--
-- Name: qrtz_blob_triggers qrtz_blob_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_blob_triggers
    ADD CONSTRAINT qrtz_blob_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_calendars qrtz_calendars_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_calendars
    ADD CONSTRAINT qrtz_calendars_pkey PRIMARY KEY (sched_name, calendar_name);


--
-- Name: qrtz_cron_triggers qrtz_cron_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_cron_triggers
    ADD CONSTRAINT qrtz_cron_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_fired_triggers qrtz_fired_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_fired_triggers
    ADD CONSTRAINT qrtz_fired_triggers_pkey PRIMARY KEY (sched_name, entry_id);


--
-- Name: qrtz_job_details qrtz_job_details_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_job_details
    ADD CONSTRAINT qrtz_job_details_pkey PRIMARY KEY (sched_name, job_name, job_group);


--
-- Name: qrtz_locks qrtz_locks_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_locks
    ADD CONSTRAINT qrtz_locks_pkey PRIMARY KEY (sched_name, lock_name);


--
-- Name: qrtz_paused_job_grps qrtz_paused_job_grps_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_paused_job_grps
    ADD CONSTRAINT qrtz_paused_job_grps_pkey PRIMARY KEY (sched_name, job_group);


--
-- Name: qrtz_paused_trigger_grps qrtz_paused_trigger_grps_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_paused_trigger_grps
    ADD CONSTRAINT qrtz_paused_trigger_grps_pkey PRIMARY KEY (sched_name, trigger_group);


--
-- Name: qrtz_scheduler_state qrtz_scheduler_state_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_scheduler_state
    ADD CONSTRAINT qrtz_scheduler_state_pkey PRIMARY KEY (sched_name, instance_name);


--
-- Name: qrtz_simple_triggers qrtz_simple_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_simple_triggers
    ADD CONSTRAINT qrtz_simple_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_simprop_triggers qrtz_simprop_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_simprop_triggers
    ADD CONSTRAINT qrtz_simprop_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_triggers qrtz_triggers_pkey; Type: CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_triggers
    ADD CONSTRAINT qrtz_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: ix_albums_artist_id_release_type; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_albums_artist_id_release_type ON content.albums USING btree (artist_id, release_type);


--
-- Name: ix_article_artists_article_id_artist_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_article_artists_article_id_artist_id ON content.article_artists USING btree (article_id, artist_id);


--
-- Name: ix_article_artists_artist_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_artists_artist_id ON content.article_artists USING btree (artist_id);


--
-- Name: ix_article_bookmarks_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_bookmarks_article_id ON content.article_bookmarks USING btree (article_id);


--
-- Name: ix_article_bookmarks_user_id_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_article_bookmarks_user_id_article_id ON content.article_bookmarks USING btree (user_id, article_id);


--
-- Name: ix_article_comment_likes_comment_user; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_article_comment_likes_comment_user ON content.article_comment_likes USING btree (comment_id, user_id);


--
-- Name: ix_article_comments_article; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_comments_article ON content.article_comments USING btree (article_id);


--
-- Name: ix_article_comments_parent; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_comments_parent ON content.article_comments USING btree (parent_comment_id);


--
-- Name: ix_article_comments_user_deleted_created_article; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_comments_user_deleted_created_article ON content.article_comments USING btree (user_id, is_deleted, created_at DESC, article_id);


--
-- Name: ix_article_images_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_images_article_id ON content.article_images USING btree (article_id);


--
-- Name: ix_article_likes_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_likes_article_id ON content.article_likes USING btree (article_id);


--
-- Name: ix_article_likes_user_id_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_article_likes_user_id_article_id ON content.article_likes USING btree (user_id, article_id);


--
-- Name: ix_article_shares_article_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_shares_article_id ON content.article_shares USING btree (article_id);


--
-- Name: ix_article_shares_user_created_article; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_shares_user_created_article ON content.article_shares USING btree (user_id, created_at DESC, article_id) WHERE (user_id IS NOT NULL);


--
-- Name: ix_article_tags_article_id_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_article_tags_article_id_tag_id ON content.article_tags USING btree (article_id, tag_id);


--
-- Name: ix_article_tags_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_article_tags_tag_id ON content.article_tags USING btree (tag_id);


--
-- Name: ix_articles_category_status_published_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_category_status_published_at ON content.articles USING btree (category_id, status, published_at);


--
-- Name: ix_articles_customer_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_customer_id ON content.articles USING btree (customer_id);


--
-- Name: ix_articles_promoted_published_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_promoted_published_at ON content.articles USING btree (published_at DESC) WHERE (is_promoted = true);


--
-- Name: ix_articles_promotion_level_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_promotion_level_id ON content.articles USING btree (promotion_level_id);


--
-- Name: ix_articles_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_articles_slug ON content.articles USING btree (slug);


--
-- Name: ix_articles_status; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_status ON content.articles USING btree (status);


--
-- Name: ix_articles_status_published_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_articles_status_published_at ON content.articles USING btree (status, published_at DESC);


--
-- Name: ix_articles_title; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_articles_title ON content.articles USING btree (title);


--
-- Name: ix_artist_claim_requests_artist_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_artist_claim_requests_artist_id ON content.artist_claim_requests USING btree (artist_id);


--
-- Name: ix_artist_claim_requests_artist_id_user_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_artist_claim_requests_artist_id_user_id ON content.artist_claim_requests USING btree (artist_id, user_id);


--
-- Name: ix_artist_claim_requests_user_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_artist_claim_requests_user_id ON content.artist_claim_requests USING btree (user_id);


--
-- Name: ix_artist_social_links_artist_id_platform; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_artist_social_links_artist_id_platform ON content.artist_social_links USING btree (artist_id, platform);


--
-- Name: ix_artists_initial_letter_name_folded; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_artists_initial_letter_name_folded ON content.artists USING btree (initial_letter, name_folded);


--
-- Name: ix_artists_name_folded; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_artists_name_folded ON content.artists USING btree (name_folded);


--
-- Name: ix_artists_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_artists_slug ON content.artists USING btree (slug);


--
-- Name: ix_artists_user_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_artists_user_id ON content.artists USING btree (user_id) WHERE (user_id IS NOT NULL);


--
-- Name: ix_categories_content_type_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_categories_content_type_id ON content.categories USING btree (content_type_id);


--
-- Name: ix_categories_is_default_for_lyrics; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_categories_is_default_for_lyrics ON content.categories USING btree (is_default_for_lyrics) WHERE (is_default_for_lyrics = true);


--
-- Name: ix_categories_is_exclusive; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_categories_is_exclusive ON content.categories USING btree (is_exclusive) WHERE (is_exclusive = true);


--
-- Name: ix_categories_is_gossip_fallback; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_categories_is_gossip_fallback ON content.categories USING btree (is_gossip_fallback) WHERE (is_gossip_fallback = true);


--
-- Name: ix_categories_name; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_categories_name ON content.categories USING btree (name);


--
-- Name: ix_categories_pinned_to_feed_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_categories_pinned_to_feed_at ON content.categories USING btree (pinned_to_feed_at) WHERE (pinned_to_feed_at IS NOT NULL);


--
-- Name: ix_categories_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_categories_slug ON content.categories USING btree (slug);


--
-- Name: ix_category_pricing_pricing_tier_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_category_pricing_pricing_tier_id ON content.category_pricing USING btree (pricing_tier_id);


--
-- Name: ix_content_item_tiers_order_item_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_item_tiers_order_item_id ON content.content_item_tiers USING btree (order_item_id);


--
-- Name: ix_content_item_tiers_pricing_tier_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_item_tiers_pricing_tier_id ON content.content_item_tiers USING btree (pricing_tier_id);


--
-- Name: ix_content_order_items_category_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_order_items_category_id ON content.content_order_items USING btree (category_id);


--
-- Name: ix_content_order_items_order_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_order_items_order_id ON content.content_order_items USING btree (order_id);


--
-- Name: ix_content_order_items_promotion_level_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_order_items_promotion_level_id ON content.content_order_items USING btree (promotion_level_id);


--
-- Name: ix_content_orders_customer_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_orders_customer_id ON content.content_orders USING btree (customer_id);


--
-- Name: ix_content_orders_package_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_content_orders_package_id ON content.content_orders USING btree (package_id);


--
-- Name: ix_content_payments_order_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_content_payments_order_id ON content.content_payments USING btree (order_id);


--
-- Name: ix_content_types_name; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_content_types_name ON content.content_types USING btree (name);


--
-- Name: ix_customers_email; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_customers_email ON content.customers USING btree (email);


--
-- Name: ix_domain_event_outbox_pending; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_domain_event_outbox_pending ON content.domain_event_outbox USING btree (occurred_on) WHERE (dispatched_at IS NULL);


--
-- Name: ix_lyrics_album_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_album_id ON content.lyrics USING btree (album_id);


--
-- Name: ix_lyrics_artist_id_status; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_artist_id_status ON content.lyrics USING btree (artist_id, status);


--
-- Name: ix_lyrics_category_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_category_id ON content.lyrics USING btree (category_id);


--
-- Name: ix_lyrics_customer_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_customer_id ON content.lyrics USING btree (customer_id);


--
-- Name: ix_lyrics_likes_lyrics_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_likes_lyrics_id ON content.lyrics_likes USING btree (lyrics_id);


--
-- Name: ix_lyrics_likes_user_id_lyrics_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_likes_user_id_lyrics_id ON content.lyrics_likes USING btree (user_id, lyrics_id);


--
-- Name: ix_lyrics_revision_votes_revision_id_user_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_revision_votes_revision_id_user_id ON content.lyrics_revision_votes USING btree (revision_id, user_id);


--
-- Name: ix_lyrics_revisions_lyrics_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_revisions_lyrics_id ON content.lyrics_revisions USING btree (lyrics_id);


--
-- Name: ix_lyrics_shares_lyrics_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_shares_lyrics_id ON content.lyrics_shares USING btree (lyrics_id);


--
-- Name: ix_lyrics_shares_user_created_lyrics; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_shares_user_created_lyrics ON content.lyrics_shares USING btree (user_id, created_at DESC, lyrics_id) WHERE (user_id IS NOT NULL);


--
-- Name: ix_lyrics_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_slug ON content.lyrics USING btree (slug);


--
-- Name: ix_lyrics_tags_lyrics_id_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_tags_lyrics_id_tag_id ON content.lyrics_tags USING btree (lyrics_id, tag_id);


--
-- Name: ix_lyrics_tags_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_tags_tag_id ON content.lyrics_tags USING btree (tag_id);


--
-- Name: ix_lyrics_translation_revisions_translation_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_translation_revisions_translation_id ON content.lyrics_translation_revisions USING btree (translation_id);


--
-- Name: ix_lyrics_translation_votes_revision_id_user_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_translation_votes_revision_id_user_id ON content.lyrics_translation_votes USING btree (revision_id, user_id);


--
-- Name: ix_lyrics_translations_lyrics_id_language; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_lyrics_translations_lyrics_id_language ON content.lyrics_translations USING btree (lyrics_id, language);


--
-- Name: ix_lyrics_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_video_id ON content.lyrics USING btree (video_id);


--
-- Name: ix_lyrics_view_events_lyrics_id_dedup_key_created_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_lyrics_view_events_lyrics_id_dedup_key_created_at ON content.lyrics_view_events USING btree (lyrics_id, dedup_key, created_at);


--
-- Name: ix_package_slots_category_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_package_slots_category_id ON content.package_slots USING btree (category_id);


--
-- Name: ix_package_slots_package_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_package_slots_package_id ON content.package_slots USING btree (package_id);


--
-- Name: ix_playlist_videos_playlist_id_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_playlist_videos_playlist_id_video_id ON content.playlist_videos USING btree (playlist_id, video_id);


--
-- Name: ix_playlist_videos_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_playlist_videos_video_id ON content.playlist_videos USING btree (video_id);


--
-- Name: ix_playlists_user; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_playlists_user ON content.playlists USING btree (user_id);


--
-- Name: ix_pricing_tiers_name; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_pricing_tiers_name ON content.pricing_tiers USING btree (name);


--
-- Name: ix_promotion_levels_name; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_promotion_levels_name ON content.promotion_levels USING btree (name);


--
-- Name: ix_short_video_bookmarks_short_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_bookmarks_short_video_id ON content.short_video_bookmarks USING btree (short_video_id);


--
-- Name: ix_short_video_bookmarks_user_id_short_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_short_video_bookmarks_user_id_short_video_id ON content.short_video_bookmarks USING btree (user_id, short_video_id);


--
-- Name: ix_short_video_likes_short_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_likes_short_video_id ON content.short_video_likes USING btree (short_video_id);


--
-- Name: ix_short_video_likes_user_id_short_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_short_video_likes_user_id_short_video_id ON content.short_video_likes USING btree (user_id, short_video_id);


--
-- Name: ix_short_video_shares_short_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_shares_short_video_id ON content.short_video_shares USING btree (short_video_id);


--
-- Name: ix_short_video_shares_user_created_short; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_shares_user_created_short ON content.short_video_shares USING btree (user_id, created_at DESC, short_video_id) WHERE (user_id IS NOT NULL);


--
-- Name: ix_short_video_view_events_short_video_id_dedup_key_created_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_view_events_short_video_id_dedup_key_created_at ON content.short_video_view_events USING btree (short_video_id, dedup_key, created_at);


--
-- Name: ix_short_video_view_events_uncounted_created_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_video_view_events_uncounted_created_at ON content.short_video_view_events USING btree (created_at) WHERE (is_counted = false);


--
-- Name: ix_short_videos_feed_rank; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_short_videos_feed_rank ON content.short_videos USING btree (feed_rank);


--
-- Name: ix_short_videos_is_active_created_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_videos_is_active_created_at ON content.short_videos USING btree (is_active, created_at);


--
-- Name: ix_short_videos_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_short_videos_slug ON content.short_videos USING btree (slug);


--
-- Name: ix_short_videos_title; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_short_videos_title ON content.short_videos USING btree (title);


--
-- Name: ix_short_videos_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_short_videos_video_id ON content.short_videos USING btree (video_id);


--
-- Name: ix_streaming_links_album_id_platform; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_streaming_links_album_id_platform ON content.streaming_links USING btree (album_id, platform);


--
-- Name: ix_streaming_links_lyrics_id_platform; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_streaming_links_lyrics_id_platform ON content.streaming_links USING btree (lyrics_id, platform);


--
-- Name: ix_tags_name; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_tags_name ON content.tags USING btree (name);


--
-- Name: ix_tags_name_lower; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_tags_name_lower ON content.tags USING btree (lower((name)::text));


--
-- Name: ix_tags_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_tags_slug ON content.tags USING btree (slug);


--
-- Name: ix_video_ratings_user_video; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_video_ratings_user_video ON content.video_ratings USING btree (user_id, video_id);


--
-- Name: ix_video_ratings_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_video_ratings_video_id ON content.video_ratings USING btree (video_id);


--
-- Name: ix_video_shares_user_created_video; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_video_shares_user_created_video ON content.video_shares USING btree (user_id, created_at DESC, video_id) WHERE (user_id IS NOT NULL);


--
-- Name: ix_video_shares_video_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_video_shares_video_id ON content.video_shares USING btree (video_id);


--
-- Name: ix_video_tags_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_video_tags_tag_id ON content.video_tags USING btree (tag_id);


--
-- Name: ix_video_tags_video_id_tag_id; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_video_tags_video_id_tag_id ON content.video_tags USING btree (video_id, tag_id);


--
-- Name: ix_videos_artist_id_status; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_videos_artist_id_status ON content.videos USING btree (artist_id, status);


--
-- Name: ix_videos_category_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_videos_category_id ON content.videos USING btree (category_id);


--
-- Name: ix_videos_customer_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_videos_customer_id ON content.videos USING btree (customer_id);


--
-- Name: ix_videos_promotion_level_id; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_videos_promotion_level_id ON content.videos USING btree (promotion_level_id);


--
-- Name: ix_videos_slug; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_videos_slug ON content.videos USING btree (slug);


--
-- Name: ix_videos_status_published_at; Type: INDEX; Schema: content; Owner: -
--

CREATE INDEX ix_videos_status_published_at ON content.videos USING btree (status, published_at DESC);


--
-- Name: ix_videos_title; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX ix_videos_title ON content.videos USING btree (title);


--
-- Name: uq_category_pricing_category_tier; Type: INDEX; Schema: content; Owner: -
--

CREATE UNIQUE INDEX uq_category_pricing_category_tier ON content.category_pricing USING btree (category_id, pricing_tier_id);


--
-- Name: ix_domain_event_outbox_pending; Type: INDEX; Schema: core; Owner: -
--

CREATE INDEX ix_domain_event_outbox_pending ON core.domain_event_outbox USING btree (occurred_on) WHERE (dispatched_at IS NULL);


--
-- Name: ix_files_created_at; Type: INDEX; Schema: core; Owner: -
--

CREATE INDEX ix_files_created_at ON core.files USING btree (created_at) WHERE ((claimed_at IS NULL) AND (is_deleted = false));


--
-- Name: ix_files_file_name; Type: INDEX; Schema: core; Owner: -
--

CREATE UNIQUE INDEX ix_files_file_name ON core.files USING btree (file_name) WHERE (is_deleted = false);


--
-- Name: ix_files_is_deleted; Type: INDEX; Schema: core; Owner: -
--

CREATE INDEX ix_files_is_deleted ON core.files USING btree (is_deleted);


--
-- Name: IX_Otps_ExpiresAt; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_Otps_ExpiresAt" ON identity.otps USING btree (expires_at);


--
-- Name: IX_Otps_Purpose_ExpiresAt; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_Otps_Purpose_ExpiresAt" ON identity.otps USING btree (purpose, expires_at);


--
-- Name: IX_Otps_UserId_Purpose; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_Otps_UserId_Purpose" ON identity.otps USING btree (user_id, purpose);


--
-- Name: IX_permissions_is_active; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_permissions_is_active" ON identity.permissions USING btree (is_active);


--
-- Name: IX_permissions_is_deleted; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_permissions_is_deleted" ON identity.permissions USING btree (is_deleted);


--
-- Name: IX_permissions_resource_action; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX "IX_permissions_resource_action" ON identity.permissions USING btree (resource, action);


--
-- Name: IX_role_permissions_role_id_permission_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX "IX_role_permissions_role_id_permission_id" ON identity.role_permissions USING btree (role_id, permission_id);


--
-- Name: IX_roles_is_active; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_roles_is_active" ON identity.roles USING btree (is_active);


--
-- Name: IX_roles_is_deleted; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX "IX_roles_is_deleted" ON identity.roles USING btree (is_deleted);


--
-- Name: IX_user_roles_user_id_role_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX "IX_user_roles_user_id_role_id" ON identity.user_roles USING btree (user_id, role_id);


--
-- Name: ix_domain_event_outbox_pending; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_domain_event_outbox_pending ON identity.domain_event_outbox USING btree (occurred_on) WHERE (dispatched_at IS NULL);


--
-- Name: ix_role_permissions_permission_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_role_permissions_permission_id ON identity.role_permissions USING btree (permission_id);


--
-- Name: ix_roles_name; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX ix_roles_name ON identity.roles USING btree (name);


--
-- Name: ix_sessions_device_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_sessions_device_id ON identity.sessions USING btree (device_id);


--
-- Name: ix_sessions_expires_at; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_sessions_expires_at ON identity.sessions USING btree (expires_at);


--
-- Name: ix_sessions_is_revoked; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_sessions_is_revoked ON identity.sessions USING btree (is_revoked);


--
-- Name: ix_sessions_refresh_token_hash; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_sessions_refresh_token_hash ON identity.sessions USING btree (refresh_token_hash);


--
-- Name: ix_sessions_user_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_sessions_user_id ON identity.sessions USING btree (user_id);


--
-- Name: ix_sessions_user_id_device_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX ix_sessions_user_id_device_id ON identity.sessions USING btree (user_id, device_id) WHERE (is_revoked = false);


--
-- Name: ix_user_roles_role_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE INDEX ix_user_roles_role_id ON identity.user_roles USING btree (role_id);


--
-- Name: ix_users_auth_provider_provider_subject_id; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX ix_users_auth_provider_provider_subject_id ON identity.users USING btree (auth_provider, provider_subject_id) WHERE (provider_subject_id IS NOT NULL);


--
-- Name: ix_users_email; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX ix_users_email ON identity.users USING btree (email);


--
-- Name: ix_users_user_name; Type: INDEX; Schema: identity; Owner: -
--

CREATE UNIQUE INDEX ix_users_user_name ON identity.users USING btree (user_name);


--
-- Name: ix_domain_event_outbox_pending; Type: INDEX; Schema: mailer; Owner: -
--

CREATE INDEX ix_domain_event_outbox_pending ON mailer.domain_event_outbox USING btree (occurred_on) WHERE (dispatched_at IS NULL);


--
-- Name: ix_newsletter_subscribers_confirmation_token; Type: INDEX; Schema: mailer; Owner: -
--

CREATE UNIQUE INDEX ix_newsletter_subscribers_confirmation_token ON mailer.newsletter_subscribers USING btree (confirmation_token);


--
-- Name: ix_newsletter_subscribers_email; Type: INDEX; Schema: mailer; Owner: -
--

CREATE UNIQUE INDEX ix_newsletter_subscribers_email ON mailer.newsletter_subscribers USING btree (email);


--
-- Name: ix_newsletter_subscribers_unsubscribe_token; Type: INDEX; Schema: mailer; Owner: -
--

CREATE UNIQUE INDEX ix_newsletter_subscribers_unsubscribe_token ON mailer.newsletter_subscribers USING btree (unsubscribe_token);


--
-- Name: ix_notifications_user_id_created_at; Type: INDEX; Schema: mailer; Owner: -
--

CREATE INDEX ix_notifications_user_id_created_at ON mailer.notifications USING btree (user_id, created_at DESC);


--
-- Name: ix_notifications_user_id_read_at; Type: INDEX; Schema: mailer; Owner: -
--

CREATE INDEX ix_notifications_user_id_read_at ON mailer.notifications USING btree (user_id, read_at);


--
-- Name: ix_outbox_emails_lease_expires_at; Type: INDEX; Schema: mailer; Owner: -
--

CREATE INDEX ix_outbox_emails_lease_expires_at ON mailer.outbox_emails USING btree (lease_expires_at);


--
-- Name: ix_outbox_emails_status_next_attempt_at; Type: INDEX; Schema: mailer; Owner: -
--

CREATE INDEX ix_outbox_emails_status_next_attempt_at ON mailer.outbox_emails USING btree (status, next_attempt_at);


--
-- Name: idx_qrtz_ft_inst_job_req_rcvry; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_ft_inst_job_req_rcvry ON quartz.qrtz_fired_triggers USING btree (sched_name, instance_name, requests_recovery);


--
-- Name: idx_qrtz_ft_j_g; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_ft_j_g ON quartz.qrtz_fired_triggers USING btree (sched_name, job_name, job_group);


--
-- Name: idx_qrtz_ft_t_g; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_ft_t_g ON quartz.qrtz_fired_triggers USING btree (sched_name, trigger_name, trigger_group);


--
-- Name: idx_qrtz_j_g_n; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_j_g_n ON quartz.qrtz_job_details USING btree (sched_name, job_group, job_name);


--
-- Name: idx_qrtz_t_c; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_t_c ON quartz.qrtz_triggers USING btree (sched_name, calendar_name);


--
-- Name: idx_qrtz_t_g_n; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_t_g_n ON quartz.qrtz_triggers USING btree (sched_name, trigger_group, trigger_name);


--
-- Name: idx_qrtz_t_j; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_t_j ON quartz.qrtz_triggers USING btree (sched_name, job_name, job_group);


--
-- Name: idx_qrtz_t_nft_st; Type: INDEX; Schema: quartz; Owner: -
--

CREATE INDEX idx_qrtz_t_nft_st ON quartz.qrtz_triggers USING btree (sched_name, trigger_state, next_fire_time, priority DESC, misfire_instr);


--
-- Name: albums fk_albums_artists_artist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.albums
    ADD CONSTRAINT fk_albums_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists(id) ON DELETE SET NULL;


--
-- Name: article_artists fk_article_artists_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_artists
    ADD CONSTRAINT fk_article_artists_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_artists fk_article_artists_artists_artist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_artists
    ADD CONSTRAINT fk_article_artists_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists(id) ON DELETE CASCADE;


--
-- Name: article_bookmarks fk_article_bookmarks_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_bookmarks
    ADD CONSTRAINT fk_article_bookmarks_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_comment_likes fk_article_comment_likes_article_comments_comment_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_comment_likes
    ADD CONSTRAINT fk_article_comment_likes_article_comments_comment_id FOREIGN KEY (comment_id) REFERENCES content.article_comments(id) ON DELETE CASCADE;


--
-- Name: article_comments fk_article_comments_article_comments_parent_comment_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_comments
    ADD CONSTRAINT fk_article_comments_article_comments_parent_comment_id FOREIGN KEY (parent_comment_id) REFERENCES content.article_comments(id) ON DELETE RESTRICT;


--
-- Name: article_comments fk_article_comments_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_comments
    ADD CONSTRAINT fk_article_comments_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_images fk_article_images_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_images
    ADD CONSTRAINT fk_article_images_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_likes fk_article_likes_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_likes
    ADD CONSTRAINT fk_article_likes_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_shares fk_article_shares_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_shares
    ADD CONSTRAINT fk_article_shares_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_tags fk_article_tags_articles_article_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_tags
    ADD CONSTRAINT fk_article_tags_articles_article_id FOREIGN KEY (article_id) REFERENCES content.articles(id) ON DELETE CASCADE;


--
-- Name: article_tags fk_article_tags_tags_tag_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.article_tags
    ADD CONSTRAINT fk_article_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags(id) ON DELETE CASCADE;


--
-- Name: articles fk_articles_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.articles
    ADD CONSTRAINT fk_articles_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE RESTRICT;


--
-- Name: articles fk_articles_customers_customer_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.articles
    ADD CONSTRAINT fk_articles_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers(id) ON DELETE SET NULL;


--
-- Name: articles fk_articles_promotion_levels_promotion_level_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.articles
    ADD CONSTRAINT fk_articles_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels(id) ON DELETE SET NULL;


--
-- Name: artist_social_links fk_artist_social_links_artists_artist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.artist_social_links
    ADD CONSTRAINT fk_artist_social_links_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists(id) ON DELETE CASCADE;


--
-- Name: categories fk_categories_content_types_content_type_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.categories
    ADD CONSTRAINT fk_categories_content_types_content_type_id FOREIGN KEY (content_type_id) REFERENCES content.content_types(id) ON DELETE RESTRICT;


--
-- Name: category_pricing fk_category_pricing_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.category_pricing
    ADD CONSTRAINT fk_category_pricing_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE CASCADE;


--
-- Name: category_pricing fk_category_pricing_pricing_tiers_pricing_tier_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.category_pricing
    ADD CONSTRAINT fk_category_pricing_pricing_tiers_pricing_tier_id FOREIGN KEY (pricing_tier_id) REFERENCES content.pricing_tiers(id) ON DELETE RESTRICT;


--
-- Name: content_item_tiers fk_content_item_tiers_content_order_items_order_item_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_item_tiers
    ADD CONSTRAINT fk_content_item_tiers_content_order_items_order_item_id FOREIGN KEY (order_item_id) REFERENCES content.content_order_items(id) ON DELETE CASCADE;


--
-- Name: content_item_tiers fk_content_item_tiers_pricing_tiers_pricing_tier_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_item_tiers
    ADD CONSTRAINT fk_content_item_tiers_pricing_tiers_pricing_tier_id FOREIGN KEY (pricing_tier_id) REFERENCES content.pricing_tiers(id) ON DELETE RESTRICT;


--
-- Name: content_order_items fk_content_order_items_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_order_items
    ADD CONSTRAINT fk_content_order_items_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE RESTRICT;


--
-- Name: content_order_items fk_content_order_items_content_orders_order_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_order_items
    ADD CONSTRAINT fk_content_order_items_content_orders_order_id FOREIGN KEY (order_id) REFERENCES content.content_orders(id) ON DELETE CASCADE;


--
-- Name: content_order_items fk_content_order_items_promotion_levels_promotion_level_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_order_items
    ADD CONSTRAINT fk_content_order_items_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels(id) ON DELETE RESTRICT;


--
-- Name: content_orders fk_content_orders_customers_customer_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_orders
    ADD CONSTRAINT fk_content_orders_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers(id) ON DELETE RESTRICT;


--
-- Name: content_orders fk_content_orders_packages_package_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_orders
    ADD CONSTRAINT fk_content_orders_packages_package_id FOREIGN KEY (package_id) REFERENCES content.packages(id) ON DELETE RESTRICT;


--
-- Name: content_payments fk_content_payments_content_orders_order_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.content_payments
    ADD CONSTRAINT fk_content_payments_content_orders_order_id FOREIGN KEY (order_id) REFERENCES content.content_orders(id) ON DELETE CASCADE;


--
-- Name: lyrics fk_lyrics_albums_album_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT fk_lyrics_albums_album_id FOREIGN KEY (album_id) REFERENCES content.albums(id) ON DELETE SET NULL;


--
-- Name: lyrics fk_lyrics_artists_artist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT fk_lyrics_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists(id) ON DELETE SET NULL;


--
-- Name: lyrics fk_lyrics_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT fk_lyrics_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE RESTRICT;


--
-- Name: lyrics fk_lyrics_customers_customer_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT fk_lyrics_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers(id) ON DELETE SET NULL;


--
-- Name: lyrics_likes fk_lyrics_likes_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_likes
    ADD CONSTRAINT fk_lyrics_likes_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: lyrics_revision_votes fk_lyrics_revision_votes_lyrics_revisions_revision_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_revision_votes
    ADD CONSTRAINT fk_lyrics_revision_votes_lyrics_revisions_revision_id FOREIGN KEY (revision_id) REFERENCES content.lyrics_revisions(id) ON DELETE CASCADE;


--
-- Name: lyrics_revisions fk_lyrics_revisions_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_revisions
    ADD CONSTRAINT fk_lyrics_revisions_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: lyrics_shares fk_lyrics_shares_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_shares
    ADD CONSTRAINT fk_lyrics_shares_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: lyrics_tags fk_lyrics_tags_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_tags
    ADD CONSTRAINT fk_lyrics_tags_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: lyrics_tags fk_lyrics_tags_tags_tag_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_tags
    ADD CONSTRAINT fk_lyrics_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags(id) ON DELETE CASCADE;


--
-- Name: lyrics_translation_revisions fk_lyrics_translation_revisions_lyrics_translations_translatio; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translation_revisions
    ADD CONSTRAINT fk_lyrics_translation_revisions_lyrics_translations_translatio FOREIGN KEY (translation_id) REFERENCES content.lyrics_translations(id) ON DELETE CASCADE;


--
-- Name: lyrics_translation_votes fk_lyrics_translation_votes_lyrics_translation_revisions_revis; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translation_votes
    ADD CONSTRAINT fk_lyrics_translation_votes_lyrics_translation_revisions_revis FOREIGN KEY (revision_id) REFERENCES content.lyrics_translation_revisions(id) ON DELETE CASCADE;


--
-- Name: lyrics_translations fk_lyrics_translations_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_translations
    ADD CONSTRAINT fk_lyrics_translations_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: lyrics fk_lyrics_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics
    ADD CONSTRAINT fk_lyrics_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE SET NULL;


--
-- Name: lyrics_view_events fk_lyrics_view_events_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.lyrics_view_events
    ADD CONSTRAINT fk_lyrics_view_events_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: package_slots fk_package_slots_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.package_slots
    ADD CONSTRAINT fk_package_slots_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE SET NULL;


--
-- Name: package_slots fk_package_slots_packages_package_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.package_slots
    ADD CONSTRAINT fk_package_slots_packages_package_id FOREIGN KEY (package_id) REFERENCES content.packages(id) ON DELETE CASCADE;


--
-- Name: playlist_videos fk_playlist_videos_playlists_playlist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.playlist_videos
    ADD CONSTRAINT fk_playlist_videos_playlists_playlist_id FOREIGN KEY (playlist_id) REFERENCES content.playlists(id) ON DELETE CASCADE;


--
-- Name: playlist_videos fk_playlist_videos_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.playlist_videos
    ADD CONSTRAINT fk_playlist_videos_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE CASCADE;


--
-- Name: short_video_bookmarks fk_short_video_bookmarks_short_videos_short_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_bookmarks
    ADD CONSTRAINT fk_short_video_bookmarks_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos(id) ON DELETE CASCADE;


--
-- Name: short_video_likes fk_short_video_likes_short_videos_short_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_likes
    ADD CONSTRAINT fk_short_video_likes_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos(id) ON DELETE CASCADE;


--
-- Name: short_video_shares fk_short_video_shares_short_videos_short_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_shares
    ADD CONSTRAINT fk_short_video_shares_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos(id) ON DELETE CASCADE;


--
-- Name: short_video_view_events fk_short_video_view_events_short_videos_short_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_video_view_events
    ADD CONSTRAINT fk_short_video_view_events_short_videos_short_video_id FOREIGN KEY (short_video_id) REFERENCES content.short_videos(id) ON DELETE CASCADE;


--
-- Name: short_videos fk_short_videos_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.short_videos
    ADD CONSTRAINT fk_short_videos_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE SET NULL;


--
-- Name: streaming_links fk_streaming_links_albums_album_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.streaming_links
    ADD CONSTRAINT fk_streaming_links_albums_album_id FOREIGN KEY (album_id) REFERENCES content.albums(id) ON DELETE CASCADE;


--
-- Name: streaming_links fk_streaming_links_lyrics_lyrics_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.streaming_links
    ADD CONSTRAINT fk_streaming_links_lyrics_lyrics_id FOREIGN KEY (lyrics_id) REFERENCES content.lyrics(id) ON DELETE CASCADE;


--
-- Name: video_ratings fk_video_ratings_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_ratings
    ADD CONSTRAINT fk_video_ratings_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE CASCADE;


--
-- Name: video_shares fk_video_shares_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_shares
    ADD CONSTRAINT fk_video_shares_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE CASCADE;


--
-- Name: video_tags fk_video_tags_tags_tag_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_tags
    ADD CONSTRAINT fk_video_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES content.tags(id) ON DELETE CASCADE;


--
-- Name: video_tags fk_video_tags_videos_video_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.video_tags
    ADD CONSTRAINT fk_video_tags_videos_video_id FOREIGN KEY (video_id) REFERENCES content.videos(id) ON DELETE CASCADE;


--
-- Name: videos fk_videos_artists_artist_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.videos
    ADD CONSTRAINT fk_videos_artists_artist_id FOREIGN KEY (artist_id) REFERENCES content.artists(id) ON DELETE SET NULL;


--
-- Name: videos fk_videos_categories_category_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.videos
    ADD CONSTRAINT fk_videos_categories_category_id FOREIGN KEY (category_id) REFERENCES content.categories(id) ON DELETE RESTRICT;


--
-- Name: videos fk_videos_customers_customer_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.videos
    ADD CONSTRAINT fk_videos_customers_customer_id FOREIGN KEY (customer_id) REFERENCES content.customers(id) ON DELETE SET NULL;


--
-- Name: videos fk_videos_promotion_levels_promotion_level_id; Type: FK CONSTRAINT; Schema: content; Owner: -
--

ALTER TABLE ONLY content.videos
    ADD CONSTRAINT fk_videos_promotion_levels_promotion_level_id FOREIGN KEY (promotion_level_id) REFERENCES content.promotion_levels(id) ON DELETE SET NULL;


--
-- Name: otps fk_otps_users_user_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.otps
    ADD CONSTRAINT fk_otps_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE;


--
-- Name: role_permissions fk_role_permissions_permissions_permission_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.role_permissions
    ADD CONSTRAINT fk_role_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES identity.permissions(id) ON DELETE CASCADE;


--
-- Name: role_permissions fk_role_permissions_roles_role_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.role_permissions
    ADD CONSTRAINT fk_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES identity.roles(id) ON DELETE CASCADE;


--
-- Name: sessions fk_sessions_users_user_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.sessions
    ADD CONSTRAINT fk_sessions_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE;


--
-- Name: user_otp_state fk_user_otp_state_users_user_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_otp_state
    ADD CONSTRAINT fk_user_otp_state_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE;


--
-- Name: user_roles fk_user_roles_roles_role_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_roles
    ADD CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES identity.roles(id) ON DELETE CASCADE;


--
-- Name: user_roles fk_user_roles_users_user_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_roles
    ADD CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE;


--
-- Name: user_token_state fk_user_token_state_users_user_id; Type: FK CONSTRAINT; Schema: identity; Owner: -
--

ALTER TABLE ONLY identity.user_token_state
    ADD CONSTRAINT fk_user_token_state_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE;


--
-- Name: qrtz_blob_triggers qrtz_blob_triggers_sched_name_trigger_name_trigger_group_fkey; Type: FK CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_blob_triggers
    ADD CONSTRAINT qrtz_blob_triggers_sched_name_trigger_name_trigger_group_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES quartz.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_cron_triggers qrtz_cron_triggers_sched_name_trigger_name_trigger_group_fkey; Type: FK CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_cron_triggers
    ADD CONSTRAINT qrtz_cron_triggers_sched_name_trigger_name_trigger_group_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES quartz.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_simple_triggers qrtz_simple_triggers_sched_name_trigger_name_trigger_group_fkey; Type: FK CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_simple_triggers
    ADD CONSTRAINT qrtz_simple_triggers_sched_name_trigger_name_trigger_group_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES quartz.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_simprop_triggers qrtz_simprop_triggers_sched_name_trigger_name_trigger_grou_fkey; Type: FK CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_simprop_triggers
    ADD CONSTRAINT qrtz_simprop_triggers_sched_name_trigger_name_trigger_grou_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES quartz.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_triggers qrtz_triggers_sched_name_job_name_job_group_fkey; Type: FK CONSTRAINT; Schema: quartz; Owner: -
--

ALTER TABLE ONLY quartz.qrtz_triggers
    ADD CONSTRAINT qrtz_triggers_sched_name_job_name_job_group_fkey FOREIGN KEY (sched_name, job_name, job_group) REFERENCES quartz.qrtz_job_details(sched_name, job_name, job_group);


--
-- PostgreSQL database dump complete
--

\unrestrict GY5N81qlbFFYiwZ55USYbNMohA0SGSEpmhLeafzB68u907ycVAmOqDM68KNxfpV

