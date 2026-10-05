using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.Contracts;

/// <summary>
/// Resolves and applies a video update: loads the video, validates the category and the slug, and
/// applies the verbs the command changes.
/// </summary>
public interface IAdminUpdateVideoService
{
    /// <summary>
    /// Applies the update, throwing the localized error when the video or category is missing or
    /// the slug is taken by another video. The caller owns the commit.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated video.</returns>
    Task<VideoEntity> UpdateAsync(AdminUpdateVideoCommand command, CancellationToken cancellationToken);
}
