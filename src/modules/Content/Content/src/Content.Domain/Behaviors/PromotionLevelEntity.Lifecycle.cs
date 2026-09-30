using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="PromotionLevelEntity" />. Its state lives in <c>Entities/PromotionLevelEntity.cs</c>.
/// </summary>
public partial class PromotionLevelEntity
{
    /// <summary>
    /// Activates the promotion level, making it available for selection on new orders.
    /// </summary>
    /// <returns>True if the promotion level was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new PromotionLevelChangedEvent(PromotionLevelId: Id));

        return true;
    }

    /// <summary>
    /// Deactivates the promotion level, hiding it from the order form for new orders.
    /// </summary>
    /// <returns>True if the promotion level was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new PromotionLevelChangedEvent(PromotionLevelId: Id));

        return true;
    }
}
