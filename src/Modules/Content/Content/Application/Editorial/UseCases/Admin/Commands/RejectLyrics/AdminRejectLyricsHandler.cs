using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Handles the <see cref="AdminRejectLyricsCommand" /> to reject a lyrics page during editorial review.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRejectLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminRejectLyricsCommand, AdminRejectLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectLyricsResult> Handle(
        AdminRejectLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (lyrics.Status == EnumContentStatus.Rejected)
        {
            throw i18n.Lyrics.AlreadyRejected();
        }

        if (lyrics.Status != EnumContentStatus.PendingReview)
        {
            throw i18n.Lyrics.InvalidStatusTransition(
                from: lyrics.Status.ToString(),
                to: nameof(EnumContentStatus.Rejected)
            );
        }

        lyrics.Reject(reason: command.Reason);
        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyContentRejectedAsync(
            customerId: lyrics.CustomerId,
            contentTitle: lyrics.SongTitle,
            reason: command.Reason,
            cancellationToken: cancellationToken
        );

        return new AdminRejectLyricsResult(IsSuccess: true);
    }
}
