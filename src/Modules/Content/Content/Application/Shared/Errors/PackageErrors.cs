using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Package domain error factory providing simple, readable exception creation.
/// Usage: PackageErrors.NotFound(id) or PackageErrors.AlreadyActive()
/// </summary>
public static class PackageErrors
{
    /// <summary>Throws when a package is not found by its identifier.</summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Package", "id", keyValue: id);
    }

    /// <summary>Throws when a package is already active.</summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(PackageErrorMessage.AlreadyActive());
    }

    /// <summary>Throws when a package is already inactive.</summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(PackageErrorMessage.AlreadyInactive());
    }

    /// <summary>Throws when a package name is required but not provided.</summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(PackageErrorMessage.NameRequired());
    }

    /// <summary>Throws when a package price is negative.</summary>
    public static BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(PackageErrorMessage.PriceMustBeNonNegative());
    }

    /// <summary>Throws when a slot quantity is not greater than zero.</summary>
    public static BadRequestException SlotQuantityMustBePositive()
    {
        return new BadRequestException(PackageErrorMessage.SlotQuantityMustBePositive());
    }

    /// <summary>Throws when a package slot is not found.</summary>
    public static NotFoundException SlotNotFound(Guid slotId)
    {
        return new NotFoundException("PackageSlot", "id", keyValue: slotId);
    }
}
