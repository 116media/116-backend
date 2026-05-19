using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// PricingTier domain error factory providing simple, readable exception creation.
/// Usage: PricingTierErrors.AlreadyExists(name) or PricingTierErrors.NotFound(id)
/// </summary>
public class PricingTierErrors(PricingTierErrorMessage msg)
{
    /// <summary>
    /// Throws when a pricing tier with the given name already exists.
    /// </summary>
    public ConflictException AlreadyExists(string name)
    {
        return new ConflictException(msg.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a pricing tier is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("PricingTier", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a pricing tier is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(msg.AlreadyActive());
    }

    /// <summary>
    /// Throws when a pricing tier is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(msg.AlreadyInactive());
    }

    /// <summary>
    /// Throws when an inactive pricing tier is used in an operation that requires an active one.
    /// </summary>
    public BadRequestException IsInactive()
    {
        return new BadRequestException(msg.IsInactive());
    }

    /// <summary>
    /// Throws when a pricing tier name is required but not provided.
    /// </summary>
    public BadRequestException NameRequired()
    {
        return new BadRequestException(msg.NameRequired());
    }
}
