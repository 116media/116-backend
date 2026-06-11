using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// ShortVideo domain error factory providing simple, readable exception creation.
/// Usage: ShortVideoErrors.NotFound(id) or ShortVideoErrors.AlreadyActive()
/// </summary>
public class ShortVideoErrors(ShortVideoErrorMessage msg)
{
    /// <summary>
    /// Throws when a short video is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ShortVideo", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a short video is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(msg.AlreadyActive());
    }

    /// <summary>
    /// Throws when a short video is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(msg.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a short video title is required but not provided.
    /// </summary>
    public BadRequestException TitleRequired()
    {
        return new BadRequestException(msg.TitleRequired());
    }

    /// <summary>
    /// Throws when a short video with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(msg.SlugAlreadyExists(slug: slug));
    }
}
