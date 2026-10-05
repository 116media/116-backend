using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.Contracts;

/// <summary>
/// Resolves and applies a lyrics update: loads the lyrics, validates the category, the slug and
/// the linked video, and applies the verbs the command changes.
/// </summary>
public interface IAdminUpdateLyricsService
{
    /// <summary>
    /// Applies the update, throwing the localized error when the lyrics, category or video is
    /// missing or the slug is taken by another page. The caller owns the commit.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated lyrics.</returns>
    Task<LyricsEntity> UpdateAsync(AdminUpdateLyricsCommand command, CancellationToken cancellationToken);
}
