using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Handles the <see cref="AdminCreateLyricsCommand" /> to create a lyrics page.
/// </summary>
/// <param name="createLyricsService">Service resolving and staging the lyrics.</param>
/// <param name="lyricsRepository">Repository reloading the committed lyrics.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="lyricsDtoService">Service assembling the lyrics detail.</param>
public class AdminCreateLyricsHandler(
    IAdminCreateLyricsService createLyricsService,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILyricsDtoService lyricsDtoService
) : ICommandHandler<AdminCreateLyricsCommand, AdminCreateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateLyricsResult> Handle(
        AdminCreateLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await createLyricsService.CreateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity created = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );
        var dto = await lyricsDtoService.CreateDetailAsync(created, cancellationToken);
        return new AdminCreateLyricsResult(Lyrics: dto);
    }
}
