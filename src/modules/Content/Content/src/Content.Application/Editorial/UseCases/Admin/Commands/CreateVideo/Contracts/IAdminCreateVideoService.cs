using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.Contracts;

/// <summary>
/// Resolves and applies a video creation: validates the category and the slug, builds the
/// aggregate and stages it.
/// </summary>
public interface IAdminCreateVideoService
{
    /// <summary>
    /// Stages the new video, throwing the localized error when the category is missing or the
    /// slug is taken. The caller owns the commit.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged video.</returns>
    Task<VideoEntity> CreateAsync(AdminCreateVideoCommand command, CancellationToken cancellationToken);
}
