using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsSeoCommand" /> to revise a lyrics page's SEO fields.
/// </summary>
/// <param name="lyricsRepository">Repository loading and reloading the lyrics.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="lyricsDtoService">Service assembling the lyrics detail.</param>
public class AdminUpdateLyricsSeoHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILyricsDtoService lyricsDtoService
) : ICommandHandler<AdminUpdateLyricsSeoCommand, AdminUpdateLyricsSeoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateLyricsSeoResult> Handle(
        AdminUpdateLyricsSeoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        lyrics.ReviseSeo(
            metaTitle: command.MetaTitle,
            metaDescription: command.MetaDescription,
            structuredData: command.StructuredData
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );
        var dto = await lyricsDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateLyricsSeoResult(Lyrics: dto);
    }
}
