using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;

/// <summary>
/// Handles the <see cref="AdminRemoveArtistSocialLinkCommand" /> to remove an artist's
/// social link for a single platform.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRemoveArtistSocialLinkHandler(IArtistRepository artistRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminRemoveArtistSocialLinkCommand, AdminRemoveArtistSocialLinkResult>
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
            throw ArtistErrors.SocialLinkNotFound(platform: command.Platform.ToString());
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRemoveArtistSocialLinkResult(IsSuccess: true);
    }
}
