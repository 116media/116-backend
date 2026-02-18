using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// PricingTier domain error factory providing simple, readable exception creation.
/// Usage: PricingTierErrors.AlreadyExists(name) or PricingTierErrors.NotFound(id)
/// </summary>
public static class PricingTierErrors
{
    /// <summary>
    /// Throws when a pricing tier with the given name already exists.
    /// </summary>
    public static ConflictException AlreadyExists(string name)
    {
        return new ConflictException(PricingTierErrorMessage.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a pricing tier is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("PricingTier", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a pricing tier is already active.
    /// </summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(PricingTierErrorMessage.AlreadyActive());
    }

    /// <summary>
    /// Throws when a pricing tier is already inactive.
    /// </summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(PricingTierErrorMessage.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a pricing tier name is required but not provided.
    /// </summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(PricingTierErrorMessage.NameRequired());
    }
}
