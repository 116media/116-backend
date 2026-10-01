using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsCommand" /> to update a lyrics page.
/// </summary>
/// <param name="updateLyricsService">Service resolving and applying the update.</param>
/// <param name="lyricsRepository">Repository reloading the committed lyrics.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="lyricsDtoService">Service assembling the lyrics detail.</param>
public class AdminUpdateLyricsHandler(
    IAdminUpdateLyricsService updateLyricsService,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILyricsDtoService lyricsDtoService
) : ICommandHandler<AdminUpdateLyricsCommand, AdminUpdateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateLyricsResult> Handle(
        AdminUpdateLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await updateLyricsService.UpdateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );
        var dto = await lyricsDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateLyricsResult(Lyrics: dto);
    }
}
