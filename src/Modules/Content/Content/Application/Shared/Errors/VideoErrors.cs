using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Video domain error factory providing simple, readable exception creation.
/// Usage: VideoErrors.NotFound(id) or VideoErrors.SlugAlreadyExists(slug)
/// </summary>
public static class VideoErrors
{
    /// <summary>
    /// Throws when a video is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Video", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a video with the given slug already exists.
    /// </summary>
    public static ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(VideoErrorMessage.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a video title is required but not provided.
    /// </summary>
    public static BadRequestException TitleRequired()
    {
        return new BadRequestException(VideoErrorMessage.TitleRequired());
    }

    /// <summary>
    /// Throws when a video slug is required but not provided.
    /// </summary>
    public static BadRequestException SlugRequired()
    {
        return new BadRequestException(VideoErrorMessage.SlugRequired());
    }

    /// <summary>
    /// Throws when a video cannot be published because no YouTube ID has been attached.
    /// </summary>
    public static BadRequestException CannotPublishWithoutYoutubeId()
    {
        return new BadRequestException(VideoErrorMessage.CannotPublishWithoutYoutubeId());
    }

    /// <summary>
    /// Throws when a hard delete is attempted on a published or approved video.
    /// </summary>
    public static BadRequestException CannotDeletePublishedVideo()
    {
        return new BadRequestException(VideoErrorMessage.CannotDeletePublishedVideo());
    }

    /// <summary>
    /// Throws when an invalid status transition is attempted.
    /// </summary>
    public static BadRequestException InvalidStatusTransition(string from, string to)
    {
        return new BadRequestException(VideoErrorMessage.InvalidStatusTransition(from: from, to: to));
    }
}
