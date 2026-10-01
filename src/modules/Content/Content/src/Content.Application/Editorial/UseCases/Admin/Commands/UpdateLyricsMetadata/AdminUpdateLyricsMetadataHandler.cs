using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsMetadataCommand" /> to revise a lyrics page's release metadata.
/// </summary>
/// <param name="lyricsRepository">Repository loading and reloading the lyrics.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="lyricsDtoService">Service assembling the lyrics detail.</param>
public class AdminUpdateLyricsMetadataHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILyricsDtoService lyricsDtoService
) : ICommandHandler<AdminUpdateLyricsMetadataCommand, AdminUpdateLyricsMetadataResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateLyricsMetadataResult> Handle(
        AdminUpdateLyricsMetadataCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        lyrics.UpdateMetadata(
            album: command.Album,
            releaseYear: command.ReleaseYear,
            label: command.Label,
            songwriter: command.Songwriter,
            producer: command.Producer
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );
        var dto = await lyricsDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateLyricsMetadataResult(Lyrics: dto);
    }
}
