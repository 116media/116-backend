using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;

/// <summary>
/// Handles the <see cref="AdminRemoveArtistSocialLinkCommand" /> to remove an artist's
/// social link for a single platform.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemoveArtistSocialLinkHandler(
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRemoveArtistSocialLinkCommand, AdminRemoveArtistSocialLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveArtistSocialLinkResult> Handle(
        AdminRemoveArtistSocialLinkCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.ArtistId,
            cancellationToken: cancellationToken
        );

        if (!artist.RemoveSocialLink(platform: command.Platform))
        {
            throw i18n.Artist.SocialLinkNotFound(platform: command.Platform.ToString());
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveArtistSocialLinkResult(IsSuccess: true);
    }
}
