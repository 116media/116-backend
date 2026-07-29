using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Handles the <see cref="AdminForceUnpromoteLyricsCommand" /> to force-unpromote a promoted lyrics page.
/// Fetches the lyrics page by id, delegates the domain logic to <see cref="LyricsEntity.ForceUnpromote" />,
/// and persists the audit fields for future pro-rata refund calculation.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="currentActor">Provides the identity of the authenticated user from JWT claims.</param>
public class AdminForceUnpromoteLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminForceUnpromoteLyricsCommand, AdminForceUnpromoteLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminForceUnpromoteLyricsResult> Handle(
        AdminForceUnpromoteLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        lyrics.ForceUnpromote(unpromotedBy: currentActor.UserId!, reason: command.Reason);

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyPromotionRemovedAsync(
            customerId: lyrics.CustomerId,
            contentTitle: lyrics.SongTitle,
            reason: command.Reason,
            cancellationToken: cancellationToken
        );

        return new AdminForceUnpromoteLyricsResult(LyricsId: lyrics.Id, UnpromotedAt: lyrics.UnpromotedAt!.Value);
    }
}
