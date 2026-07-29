using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics;

/// <summary>
/// Handles the <see cref="AdminPublishLyricsCommand" /> to publish an approved lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminPublishLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminPublishLyricsCommand, AdminPublishLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminPublishLyricsResult> Handle(
        AdminPublishLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (lyrics.Status == EnumContentStatus.Published)
        {
            throw i18n.Lyrics.AlreadyPublished();
        }

        if (lyrics.Status != EnumContentStatus.Approved)
        {
            throw i18n.Lyrics.InvalidStatusTransition(
                from: lyrics.Status.ToString(),
                to: nameof(EnumContentStatus.Published)
            );
        }

        lyrics.Publish();
        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyContentPublishedAsync(
            customerId: lyrics.CustomerId,
            contentTitle: lyrics.SongTitle,
            publicUrl: ContentPublicLinks.Lyrics(lyrics.Slug),
            cancellationToken: cancellationToken
        );

        return new AdminPublishLyricsResult(IsSuccess: true);
    }
}
