using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Package domain error factory providing simple, readable exception creation.
/// Usage: PackageErrors.NotFound(id) or PackageErrors.AlreadyActive()
/// </summary>
public class PackageErrors(PackageErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public PackageErrorMessage Msg => i18n;

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
        return new ConflictException(i18n.AlreadyActive());
    }

    /// <summary>
    /// Throws when a package is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(i18n.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a package price is negative.
    /// </summary>
    public BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(i18n.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Throws when a package slot is not found.
    /// </summary>
    public NotFoundException SlotNotFound(Guid slotId)
    {
        return new NotFoundException("PackageSlot", "id", keyValue: slotId);
    }
}
