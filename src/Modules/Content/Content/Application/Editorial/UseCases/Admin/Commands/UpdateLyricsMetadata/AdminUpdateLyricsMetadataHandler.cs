using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsMetadataCommand" /> to update the song-credit
/// metadata of an existing lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class AdminUpdateLyricsMetadataHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileStorageService fileStorage
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

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToLyricsDetailDtoAsync(mapper, userLookup, fileStorage, cancellationToken);
        return new AdminUpdateLyricsMetadataResult(Lyrics: dto);
    }
}
