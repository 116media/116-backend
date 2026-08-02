namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// URL path segments for Editorial sub-module routes.
/// Provides centralized string constants for URL segments used in article, video,
/// short video, and lyrics management routes.
/// </summary>
public static class EditorialRouteConstants
{
    /// <summary>
    /// The base endpoint path for article routes.
    /// Combined with admin prefix: /api/v1/admin/articles.
    /// </summary>
    public const string Articles = "articles";

    /// <summary>
    /// The base endpoint path for video routes.
    /// Combined with admin prefix: /api/v1/admin/videos.
    /// </summary>
    public const string Videos = "videos";

    /// <summary>
    /// The base endpoint path for short video routes.
    /// Combined with admin prefix: /api/v1/admin/shorts.
    /// </summary>
    public const string Shorts = "shorts";

    /// <summary>
    /// The base endpoint path for lyrics routes.
    /// Combined with admin prefix: /api/v1/admin/lyrics or public prefix: /api/v1/public/lyrics.
    /// </summary>
    public const string Lyrics = "lyrics";

    /// <summary>
    /// Route segment for submitting an editorial entity for review or payment.
    /// Example: /api/v1/admin/articles/{id}/submit.
    /// </summary>
    public const string Submit = "submit";

    /// <summary>
    /// Route segment for approving an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/approve.
    /// </summary>
    public const string Approve = "approve";

    /// <summary>
    /// Route segment for publishing an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/publish.
    /// </summary>
    public const string Publish = "publish";

    /// <summary>
    /// Route segment for rejecting an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/reject.
    /// </summary>
    public const string Reject = "reject";

    /// <summary>
    /// Route segment for archiving an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/archive.
    /// </summary>
    public const string Archive = "archive";

    /// <summary>
    /// Route segment for article image upload and listing sub-resource.
    /// Example: /api/v1/admin/articles/{id}/images.
    /// </summary>
    public const string Images = "images";

    /// <summary>
    /// Route segment for updating tag associations on an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/tags.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Route segment for SEO metadata operations on an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/seo.
    /// </summary>
    public const string Seo = "seo";

    /// <summary>
    /// Route segment for song-credit metadata operations on a lyrics page.
    /// Example: /api/v1/admin/lyrics/{id}/metadata.
    /// </summary>
    public const string Metadata = "metadata";

    /// <summary>
    /// Route segment for cover/album art image upload on a lyrics page.
    /// Example: /api/v1/admin/lyrics/{id}/cover.
    /// </summary>
    public const string Cover = "cover";

    /// <summary>
    /// Route segment for attaching a YouTube video identifier to a video.
    /// Example: /api/v1/admin/videos/{id}/youtube.
    /// </summary>
    public const string Youtube = "youtube";

    /// <summary>
    /// Route segment for thumbnail image upload on a video or short video.
    /// Example: /api/v1/admin/videos/{id}/thumbnail.
    /// </summary>
    public const string Thumbnail = "thumbnail";

    /// <summary>
    /// Route segment for uploading or replacing the video file of a short video.
    /// Example: /api/v1/admin/shorts/{id}/video.
    /// </summary>
    public const string Video = "video";

    /// <summary>
    /// Route segment for scheduling a video shoot.
    /// Example: /api/v1/admin/videos/{id}/shoot.
    /// </summary>
    public const string Shoot = "shoot";

    /// <summary>
    /// Route segment for activating an editorial entity.
    /// Example: /api/v1/admin/shorts/{id}/activate.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating an editorial entity.
    /// Example: /api/v1/admin/shorts/{id}/deactivate.
    /// </summary>
    public const string Deactivate = "deactivate";

    /// <summary>
    /// Route segment for retrieving promoted editorial entities.
    /// Example: /api/v1/public/articles/promoted.
    /// </summary>
    public const string Promoted = "promoted";

    /// <summary>
    /// Route segment for retrieving active editorial entities.
    /// Example: /api/v1/admin/videos/active.
    /// </summary>
    public const string Active = "active";

    /// <summary>
    /// Route segment for retrieving popularity-ranked editorial entities.
    /// Example: /api/v1/public/articles/popular.
    /// </summary>
    public const string Popular = "popular";

    /// <summary>
    /// Route segment for force-unpromoting a promoted editorial entity (SuperAdmin only).
    /// Example: /api/v1/admin/articles/{slug}/unpromote.
    /// </summary>
    public const string Unpromote = "unpromote";

    /// <summary>
    /// Route segment for the homepage promotion feed endpoint.
    /// Example: /api/v1/public/articles/promotion/feed.
    /// </summary>
    public const string PromotionFeed = "promotion/feed";

    /// <summary>
    /// Route segment for the homepage content feed endpoint.
    /// Example: /api/v1/public/videos/feed.
    /// </summary>
    public const string Feed = "feed";

    /// <summary>
    /// The base endpoint path for artist profile routes.
    /// Combined with admin prefix: /api/v1/admin/artists or public prefix: /api/v1/public/artists.
    /// </summary>
    public const string Artists = "artists";

    /// <summary>
    /// The base endpoint path for album routes.
    /// Combined with admin prefix: /api/v1/admin/albums.
    /// </summary>
    public const string Albums = "albums";

    /// <summary>
    /// Route segment for avatar image upload on an artist profile.
    /// Example: /api/v1/admin/artists/{id}/avatar.
    /// </summary>
    public const string Avatar = "avatar";

    /// <summary>
    /// Route segment for linking or unlinking an artist profile to a lyrics page or video.
    /// Example: /api/v1/admin/lyrics/{id}/artist.
    /// </summary>
    public const string Artist = "artist";

    /// <summary>
    /// Route segment for linking or unlinking an album to a lyrics page.
    /// Example: /api/v1/admin/lyrics/{id}/album.
    /// </summary>
    public const string Album = "album";

    /// <summary>
    /// Route segment for requesting ownership of an artist profile.
    /// Example: /api/v1/artists/{id}/claim.
    /// </summary>
    public const string Claim = "claim";

    /// <summary>
    /// Route segment for verifying and confirming an artist profile's claimed owner.
    /// Example: /api/v1/admin/artists/{id}/verify-owner.
    /// </summary>
    public const string VerifyOwner = "verify-owner";

    /// <summary>
    /// Route segment for streaming platform link operations on an album or a standalone
    /// single lyrics page.
    /// Example: /api/v1/admin/albums/{id}/streaming-links/{platform}.
    /// </summary>
    public const string StreamingLinks = "streaming-links";

    /// <summary>
    /// Route segment for retrieving similar lyrics pages.
    /// Example: /api/v1/public/lyrics/{id}/similar.
    /// </summary>
    public const string Similar = "similar";

    /// <summary>
    /// The base endpoint path for lyrics translation routes, both nested under a lyrics page
    /// (/api/v1/public/lyrics/{id}/translations) and top-level for the review workflow
    /// (/api/v1/translations/{id}/revisions).
    /// </summary>
    public const string Translations = "translations";

    /// <summary>
    /// Route segment for the propose/vote/accept revision workflow on a translation.
    /// Example: /api/v1/translations/{id}/revisions.
    /// </summary>
    public const string Revisions = "revisions";

    /// <summary>
    /// Route segment for casting a community vote on a pending revision.
    /// Example: /api/v1/translations/revisions/{id}/votes.
    /// </summary>
    public const string Votes = "votes";

    /// <summary>
    /// The base endpoint path for community song submission routes, both for submitting a new
    /// song (/api/v1/lyrics/submissions) and for the admin moderation queue
    /// (/api/v1/admin/lyrics/submissions).
    /// </summary>
    public const string Submissions = "submissions";

    /// <summary>
    /// Route segment for a moderator asking a submitter to revise and resubmit their content.
    /// Example: /api/v1/admin/lyrics/submissions/{id}/request-revision.
    /// </summary>
    public const string RequestRevision = "request-revision";
}
