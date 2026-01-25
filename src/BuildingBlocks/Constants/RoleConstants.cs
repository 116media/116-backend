namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Contains constants related to role entity business rules and constraints.
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Maximum allowed length for role names.
    /// </summary>
    public const int MaxRoleNameLength = 20;

    /// <summary>
    /// Maximum allowed length for role descriptions.
    /// </summary>
    public const int MaxRoleDescriptionLength = 300;

    /// <summary>
    /// Default value for role active status.
    /// </summary>
    public const bool DefaultIsActive = true;

    /// <summary>
    /// Default value for role soft-delete status.
    /// </summary>
    public const bool DefaultIsDeleted = false;
}
