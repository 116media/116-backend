using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Handles the <see cref="AdminUploadLyricsCoverCommand" /> to upload or replace a lyrics
/// page's cover/album art image. The cover file is tracked via <see cref="FileReferenceDto" /> in
/// the Core module.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadLyricsCoverHandler(
    ILyricsRepository lyricsRepository,
    IFileStorageService fileStorage,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadLyricsCoverCommand, AdminUploadLyricsCoverResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadLyricsCoverResult> Handle(
        AdminUploadLyricsCoverCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.File!;

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: command.LyricsId.ToString(),
            folder: "content/lyrics-covers",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: lyrics.CoverImageFileId,
                    cancellationToken: ct
                );

                lyrics.SetCoverImageFileId(coverImageFileId: uploaded.Reference.Id);

                lyricsRepository.Update(lyrics: lyrics);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadLyricsCoverResult(
            CoverImageUrl: uploaded.Reference.StorageUrl,
            CoverImageStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
