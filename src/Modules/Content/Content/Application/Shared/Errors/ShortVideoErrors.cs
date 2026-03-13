using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// ShortVideo domain error factory providing simple, readable exception creation.
/// Usage: ShortVideoErrors.NotFound(id) or ShortVideoErrors.AlreadyActive()
/// </summary>
public static class ShortVideoErrors
{
    /// <summary>
    /// Throws when a short video is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ShortVideo", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a short video is already active.
    /// </summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(ShortVideoErrorMessage.AlreadyActive());
    }

    /// <summary>
    /// Throws when a short video is already inactive.
    /// </summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(ShortVideoErrorMessage.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a short video title is required but not provided.
    /// </summary>
    public static BadRequestException TitleRequired()
    {
        return new BadRequestException(ShortVideoErrorMessage.TitleRequired());
    }
}
