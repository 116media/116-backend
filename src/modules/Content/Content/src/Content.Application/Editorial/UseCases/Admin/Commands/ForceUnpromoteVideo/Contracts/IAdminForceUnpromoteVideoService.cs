using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.Contracts;

/// <summary>
/// Resolves and applies a forced unpromotion: loads the video by slug and unpromotes it on
/// behalf of the acting admin against the clock.
/// </summary>
public interface IAdminForceUnpromoteVideoService
{
    /// <summary>
    /// Unpromotes the video, throwing the localized error when the slug is unknown. The
    /// caller owns the commit.
    /// </summary>
    /// <param name="slug">The video's slug.</param>
    /// <param name="reason">The admin's reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unpromoted video.</returns>
    Task<VideoEntity> UnpromoteAsync(string slug, string reason, CancellationToken cancellationToken);
}
