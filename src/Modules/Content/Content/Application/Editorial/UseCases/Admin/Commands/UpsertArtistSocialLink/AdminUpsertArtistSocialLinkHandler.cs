using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Handles the <see cref="AdminUpsertArtistSocialLinkCommand" /> to set or replace an
/// artist's social link for a single platform.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpsertArtistSocialLinkHandler(IArtistRepository artistRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminUpsertArtistSocialLinkCommand, AdminUpsertArtistSocialLinkResult>
{
    /// <inheritdoc />
    public async Task<AdminUpsertArtistSocialLinkResult> Handle(
        AdminUpsertArtistSocialLinkCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.ArtistId,
            cancellationToken: cancellationToken
        );

        artist.SetSocialLink(platform: command.Platform, url: command.Url);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArtistSocialLinkEntity link = artist.FindSocialLink(platform: command.Platform)!;

        return new AdminUpsertArtistSocialLinkResult(SocialLinkId: link.Id);
    }
}
