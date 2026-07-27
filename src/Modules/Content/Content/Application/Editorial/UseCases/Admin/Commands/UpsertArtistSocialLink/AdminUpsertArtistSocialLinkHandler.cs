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
        await artistRepository.GetByIdOrThrowAsync(id: command.ArtistId, cancellationToken: cancellationToken);

        ArtistSocialLinkEntity? existing = await artistRepository.GetSocialLinkAsync(
            artistId: command.ArtistId,
            platform: command.Platform,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            existing.UpdateUrl(url: command.Url);
            artistRepository.UpdateSocialLink(link: existing);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new AdminUpsertArtistSocialLinkResult(SocialLinkId: existing.Id);
        }

        ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
            id: Guid.NewGuid(),
            artistId: command.ArtistId,
            platform: command.Platform,
            url: command.Url
        );

        await artistRepository.AddSocialLinkAsync(link: link, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpsertArtistSocialLinkResult(SocialLinkId: link.Id);
    }
}
