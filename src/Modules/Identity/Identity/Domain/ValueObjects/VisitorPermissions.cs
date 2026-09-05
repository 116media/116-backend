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
    /// Returns content-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetContent() =>
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
    /// Returns profile-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetProfile() =>
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
    /// Returns like-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetLikes() =>
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
    /// Returns comment-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetComments() =>
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
    /// Returns bookmark-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetBookmarks() =>
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
    /// Returns navigation-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetNavigation() =>
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
    /// Returns playlist-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetPlaylists() =>
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
    /// Returns advertisement-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetAds() =>
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
    /// Returns rating-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetRates() =>
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
    /// Returns share-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetShares() =>
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
        return GetContent()
            .Concat(GetProfile())
            .Concat(GetLikes())
            .Concat(GetComments())
            .Concat(GetBookmarks())
            .Concat(GetNavigation())
            .Concat(GetPlaylists())
            .Concat(GetAds())
            .Concat(GetRates())
            .Concat(GetShares())
            .ToArray();
    }
}
