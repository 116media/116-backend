namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Contains constants related to permission entity business rules and constraints.
/// </summary>
public static class PermissionConstants
{
    /// <summary>
    /// Maximum allowed length for permission resource names.
    /// </summary>
    public const int MaxPermissionResourceLength = 15;

    /// <summary>
    /// Maximum allowed length for permission action names.
    /// </summary>
    public const int MaxPermissionActionLength = 15;

    /// <summary>
    /// Maximum allowed length for permission descriptions.
    /// </summary>
    public const int MaxPermissionDescriptionLength = 300;

    /// <summary>
    /// Default value for permission active status.
    /// </summary>
    public const bool DefaultIsActive = true;

    /// <summary>
    /// Default value for permission soft-delete status.
    /// </summary>
    public const bool DefaultIsDeleted = false;
}
