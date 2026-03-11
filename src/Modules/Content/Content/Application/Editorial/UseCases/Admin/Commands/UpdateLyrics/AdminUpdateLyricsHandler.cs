using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsCommand" /> to replace the lyrics text of an existing lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateLyricsHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<AdminUpdateLyricsCommand, AdminUpdateLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateLyricsResult> Handle(
        AdminUpdateLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        lyrics.UpdateLyrics(lyricsText: command.LyricsText);

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToLyricsDto(mapper);
        return new AdminUpdateLyricsResult(Lyrics: dto);
    }
}
