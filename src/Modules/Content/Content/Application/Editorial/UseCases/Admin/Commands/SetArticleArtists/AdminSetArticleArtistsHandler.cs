using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Handles the <see cref="AdminSetArticleArtistsCommand" /> to set-replace the artists an
/// article is tagged with. Every incoming artist id is verified to exist before anything is
/// written, and the error names the first missing id — an admin pasting five ids needs to
/// know which one is wrong.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSetArticleArtistsHandler(
    IArticleRepository articleRepository,
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminSetArticleArtistsCommand, AdminSetArticleArtistsResult>
{
    /// <inheritdoc />
    public async Task<AdminSetArticleArtistsResult> Handle(
        AdminSetArticleArtistsCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);

        foreach (Guid artistId in command.ArtistIds)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(
                id: artistId,
                cancellationToken: cancellationToken
            );

            if (artist is null)
            {
                throw i18n.Artist.NotFound(id: artistId);
            }
        }

        await articleRepository.ReplaceArticleArtistsAsync(
            articleId: command.ArticleId,
            artistIds: command.ArtistIds,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        IReadOnlyList<ArticleArtistEntity> current = await articleRepository.GetArtistsByArticleIdAsync(
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        return new AdminSetArticleArtistsResult(ArtistIds: current.Select(aa => aa.ArtistId).ToList());
    }
}
