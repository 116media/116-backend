using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Package domain error factory providing simple, readable exception creation.
/// Usage: PackageErrors.NotFound(id) or PackageErrors.AlreadyActive()
/// </summary>
public class PackageErrors(PackageErrorMessage msg)
{
    /// <summary>
    /// Throws when a package is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Package", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a package is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(msg.AlreadyActive());
    }

    /// <summary>
    /// Throws when a package is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(msg.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a package name is required but not provided.
    /// </summary>
    public BadRequestException NameRequired()
    {
        return new BadRequestException(msg.NameRequired());
    }

    /// <summary>
    /// Throws when a package price is negative.
    /// </summary>
    public BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(msg.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Throws when a slot quantity is not greater than zero.
    /// </summary>
    public BadRequestException SlotQuantityMustBePositive()
    {
        return new BadRequestException(msg.SlotQuantityMustBePositive());
    }

    /// <summary>
    /// Throws when a package slot is not found.
    /// </summary>
    public NotFoundException SlotNotFound(Guid slotId)
    {
        return new NotFoundException("PackageSlot", "id", keyValue: slotId);
    }
}
