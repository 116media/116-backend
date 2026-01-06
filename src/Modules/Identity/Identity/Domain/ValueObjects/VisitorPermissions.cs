using _116.Identity.Domain.Entities;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Defines the permissions available for Visitor role users using PermissionEntity for type safety.
/// </summary>
/// <remarks>
/// These permissions align with the CoreUserRole.Visitor specification and provide
/// type-safe access to permission definitions using the domain entity.
/// </remarks>
public static class VisitorPermissions
{
    /// <summary>
    /// Content-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Content =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "articles",
            "read",
            "Allows visitors to view and read published articles"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "videos",
            "read",
            "Grants access to watch published video content streaming"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "contents",
            "read",
            "Provides broad access to view all published content"
        ),
    ];

    /// <summary>
    /// Profile-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Profile =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_profile",
            "read",
            "Enables visitors to view and read their own profile information"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_profile",
            "update",
            "Allows visitors to modify their own profile information"
        ),
    ];

    /// <summary>
    /// Like-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Likes =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "likes",
            "create",
            "Grants ability to express appreciation by creating likes"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_likes",
            "delete",
            "Allows visitors to remove their previously created likes"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "likes",
            "read",
            "Enables viewing like counts and engagement metrics content"
        ),
    ];

    /// <summary>
    /// Comment-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Comments =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "comments",
            "read",
            "Provides access to view comments and community discussions"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "comments",
            "create",
            "Enables visitors to participate by posting new comments"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_comments",
            "update",
            "Allows visitors to edit their own posted comments"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_comments",
            "delete",
            "Grants ability to remove their own posted comments"
        ),
    ];

    /// <summary>
    /// Bookmark-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Bookmarks =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "bookmarks",
            "create",
            "Enables saving interesting content for later reference access"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_bookmarks",
            "delete",
            "Allows removing items from personal bookmark collection management"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_bookmarks",
            "read",
            "Grants access to view personal saved bookmark collections"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "bookmarks",
            "read",
            "Provides access to view public community bookmark collections"
        ),
    ];

    /// <summary>
    /// Navigation-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Navigation =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "tags",
            "read",
            "Enables browsing content tags for topic based navigation"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "categories",
            "read",
            "Provides access to browse organized content category structures"
        ),
    ];

    /// <summary>
    /// Playlist-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Playlists =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "playlists",
            "create",
            "Grants ability to create custom personalized content playlists"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_playlists",
            "update",
            "Allows modifying personal playlists including adding removing content"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_playlists",
            "delete",
            "Enables removing personal playlists when no longer needed"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_playlists",
            "read",
            "Provides access to view personal created playlist collections"
        ),
    ];

    /// <summary>
    /// Advertisement-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Ads =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "ads_banners",
            "read",
            "Allows viewing banner advertisements throughout the entire platform"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "ads_stories",
            "read",
            "Enables viewing story format advertisements in content feeds"
        ),
    ];

    /// <summary>
    /// Rating-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Rates =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "rates",
            "create",
            "Grants ability to rate content using evaluation mechanisms"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "rates",
            "read",
            "Provides access to view ratings and community assessment"
        ),
    ];

    /// <summary>
    /// Share-related permissions for visitors.
    /// </summary>
    public static readonly PermissionEntity[] Shares =
    [
        PermissionEntity.Create(
            Guid.NewGuid(),
            "shares",
            "create",
            "Enables sharing content through various social media mechanisms"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "shares",
            "read",
            "Provides access to view sharing statistics and metadata"
        ),
        PermissionEntity.Create(
            Guid.NewGuid(),
            "own_shares",
            "read",
            "Grants access to view personal sharing history statistics"
        ),
    ];

    /// <summary>
    /// Gets all visitor permissions as a single flattened array of PermissionEntity.
    /// </summary>
    /// <returns>All permissions for the Visitor role as typed entities.</returns>
    public static PermissionEntity[] GetAllPermissions()
    {
        return Content
            .Concat(second: Profile)
            .Concat(second: Likes)
            .Concat(second: Comments)
            .Concat(second: Bookmarks)
            .Concat(second: Navigation)
            .Concat(second: Playlists)
            .Concat(second: Ads)
            .Concat(second: Rates)
            .Concat(second: Shares)
            .ToArray();
    }
}
