namespace _116.Identity.Domain.Enums;

/// <summary>
/// Defines the core user roles within the system hierarchy.
/// </summary>
/// <remarks>
/// This enumeration contains only the foundational administrative roles that form the base
/// of the user management system. These are immutable system roles that cannot be created
/// or modified at runtime.
///
/// <para/>
/// <para>Role Hierarchy and Permissions:</para>
/// <list type="bullet">
/// <item><see cref="SuperAdmin"/> - Has complete system control and can manage all aspects including user roles</item>
/// <item><see cref="Admin"/> - Has administrative privileges but cannot modify core system roles</item>
/// <item><see cref="Visitor"/> - Standard public user with read/write access to
/// content, profile, interactions, bookmarks, playlists, ads, ratings, and shares
/// </item>
/// </list>
///
/// <para>Additional Role Assignment:</para>
/// Users with <see cref="Admin"/> or <see cref="SuperAdmin"/> roles can be assigned additional
/// domain-specific roles (e.g., Moderator, AdsManager, ContentManager) through the permission
/// system. These additional roles are created and managed by <see cref="SuperAdmin"/> users
/// and stored separately from these core system roles.
/// <para/>
/// <para>Usage Example:</para>
/// A user might have <see cref="Admin"/> as their base role, plus additional permissions for
/// "Moderator" and "AdsManager" functions, allowing them to moderate content and manage
/// advertisements while maintaining their core administrative privileges.
/// </remarks>
public enum EnumCoreUserRole
{
    /// <summary>
    /// Administrative role with elevated system privileges.
    /// </summary>
    /// <remarks>
    /// Users with this role have administrative access to most system functions but cannot
    /// modify core system roles or perform super-admin-only operations. They can be assigned
    /// additional domain-specific roles and permissions as needed.
    /// </remarks>
    Admin,

    /// <summary>
    /// Highest privilege role with complete system control.
    /// </summary>
    /// <remarks>
    /// Users with this role have unrestricted access to all system functions, including
    /// the ability to create, modify, and assign additional roles and permissions to other users.
    /// This role should be assigned sparingly and only to trusted system administrators.
    /// </remarks>
    SuperAdmin,

    /// <summary>
    /// Public user role with standard permissions.
    /// </summary>
    /// <remarks>
    /// The Visitor role is intended for public users and has the following permissions:
    /// <list type="bullet">
    /// <item>articles.read</item>
    /// <item>videos.read</item>
    /// <item>contents.read</item>
    /// <item>own_profile.read</item>
    /// <item>own_profile.update</item>
    /// <item>likes.create</item>
    /// <item>own_likes.delete</item>
    /// <item>likes.read</item>
    /// <item>comments.read</item>
    /// <item>comments.create</item>
    /// <item>own_comments.update</item>
    /// <item>own_comments.delete</item>
    /// <item>bookmarks.create</item>
    /// <item>own_bookmarks.delete</item>
    /// <item>own_bookmarks.read</item>
    /// <item>bookmarks.read</item>
    /// <item>tags.read</item>
    /// <item>categories.read</item>
    /// <item>playlists.create</item>
    /// <item>own_playlists.update</item>
    /// <item>own_playlists.delete</item>
    /// <item>own_playlists.read</item>
    /// <item>ads_banners.read</item>
    /// <item>ads_stories.read</item>
    /// <item>rates.create</item>
    /// <item>rates.read</item>
    /// <item>shares.create</item>
    /// <item>shares.read</item>
    /// <item>own_shares.read</item>
    /// </list>
    /// </remarks>
    Visitor
}
