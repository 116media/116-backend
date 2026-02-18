using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// PromotionLevel domain error factory providing simple, readable exception creation.
/// Usage: PromotionLevelErrors.AlreadyExists(name) or PromotionLevelErrors.NotFound(id)
/// </summary>
public static class PromotionLevelErrors
{
    /// <summary>
    /// Throws when a promotion level with the given name already exists.
    /// </summary>
    public static ConflictException AlreadyExists(string name)
    {
        return new ConflictException(PromotionLevelErrorMessage.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a promotion level is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("PromotionLevel", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a promotion level is already active.
    /// </summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(PromotionLevelErrorMessage.AlreadyActive());
    }

    /// <summary>
    /// Throws when a promotion level is already inactive.
    /// </summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(PromotionLevelErrorMessage.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a promotion level name is required but not provided.
    /// </summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(PromotionLevelErrorMessage.NameRequired());
    }

    /// <summary>
    /// Throws when the promotion level duration is not a positive number.
    /// </summary>
    public static BadRequestException DurationMustBePositive()
    {
        return new BadRequestException(PromotionLevelErrorMessage.DurationMustBePositive());
    }

    /// <summary>
    /// Throws when the promotion level price is negative.
    /// </summary>
    public static BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(PromotionLevelErrorMessage.PriceMustBeNonNegative());
    }
}
