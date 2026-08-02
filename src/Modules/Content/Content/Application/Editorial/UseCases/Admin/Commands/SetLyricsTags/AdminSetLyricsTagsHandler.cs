using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags;

/// <summary>
/// Handles the <see cref="AdminSetLyricsTagsCommand" /> to replace the full set of tags
/// applied to a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminSetLyricsTagsHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminSetLyricsTagsCommand, AdminSetLyricsTagsResult>
{
    /// <inheritdoc />
    public async Task<AdminSetLyricsTagsResult> Handle(
        AdminSetLyricsTagsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        await lyricsRepository.ReplaceTagsAsync(
            lyricsId: lyrics.Id,
            tagIds: command.TagIds,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSetLyricsTagsResult(IsSuccess: true);
    }
}
