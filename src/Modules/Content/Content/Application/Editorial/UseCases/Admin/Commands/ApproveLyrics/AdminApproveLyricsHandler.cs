using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics;

/// <summary>
/// Handles the <see cref="AdminApproveLyricsCommand" /> to approve a lyrics page pending editorial review.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminApproveLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminApproveLyricsCommand, AdminApproveLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminApproveLyricsResult> Handle(
        AdminApproveLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (lyrics.Status == EnumContentStatus.Approved)
        {
            throw i18n.Lyrics.AlreadyApproved();
        }

        if (lyrics.Status != EnumContentStatus.PendingReview)
        {
            throw i18n.Lyrics.InvalidStatusTransition(
                from: lyrics.Status.ToString(),
                to: nameof(EnumContentStatus.Approved)
            );
        }

        lyrics.Approve();
        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminApproveLyricsResult(IsSuccess: true);
    }
}
