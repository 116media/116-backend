using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// PromotionLevel domain error factory providing simple, readable exception creation.
/// Usage: PromotionLevelErrors.AlreadyExists(name) or PromotionLevelErrors.NotFound(id)
/// </summary>
public class PromotionLevelErrors(PromotionLevelErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public PromotionLevelErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a promotion level with the given name already exists.
    /// </summary>
    public ConflictException AlreadyExists(string name)
    {
        return new ConflictException(i18n.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a promotion level is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("PromotionLevel", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a promotion level is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(i18n.AlreadyActive());
    }

    /// <summary>
    /// Throws when a promotion level is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(i18n.AlreadyInactive());
    }
}
