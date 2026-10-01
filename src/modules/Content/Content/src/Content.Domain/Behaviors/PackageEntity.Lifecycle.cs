namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="PackageEntity" />. Its state lives in <c>Entities/PackageEntity.cs</c>.
/// </summary>
public partial class PackageEntity
{
    /// <summary>
    /// Activates the package, making it available for new orders.
    /// </summary>
    /// <returns>True if the package was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Deactivates the package, removing it from available bundles for new orders.
    /// </summary>
    /// <returns>True if the package was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }
}
