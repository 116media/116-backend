using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;

/// <summary>
/// Handles the <see cref="AdminSubmitLyricsCommand" /> to submit a lyrics page for review or payment.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSubmitLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminSubmitLyricsCommand, AdminSubmitLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminSubmitLyricsResult> Handle(
        AdminSubmitLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (lyrics.CustomerId.HasValue && lyrics.OrderItemId.HasValue)
        {
            ContentOrderEntity? order = await contentOrderRepository.GetOrderByItemIdAsync(
                orderItemId: lyrics.OrderItemId.Value,
                ct: cancellationToken
            );

            bool orderAlreadyPaid = order?.Status == EnumOrderStatus.Paid;

            if (orderAlreadyPaid && !lyrics.MarkPendingReview())
            {
                throw i18n.Lyrics.AlreadyPendingReview();
            }

            if (!orderAlreadyPaid && !lyrics.Submit())
            {
                throw i18n.Lyrics.AlreadySubmitted();
            }
        }
        else if (!lyrics.CustomerId.HasValue && !lyrics.MarkPendingReview())
        {
            throw i18n.Lyrics.AlreadyPendingReview();
        }

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSubmitLyricsResult(IsSuccess: true);
    }
}
