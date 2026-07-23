using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Handles the <see cref="AdminUpdateLyricsSeoCommand" /> to update the SEO metadata of an existing lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
public class AdminUpdateLyricsSeoHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository
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

        lyrics.UpdateSeo(
            metaTitle: command.MetaTitle,
            metaDescription: command.MetaDescription,
            structuredData: command.StructuredData
        );

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        LyricsEntity updated = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyrics.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, cancellationToken);
        return new AdminUpdateLyricsSeoResult(Lyrics: dto);
    }
}
