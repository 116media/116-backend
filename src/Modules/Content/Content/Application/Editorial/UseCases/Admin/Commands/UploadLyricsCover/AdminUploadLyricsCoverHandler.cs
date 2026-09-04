using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Handles the <see cref="AdminUploadLyricsCoverCommand" /> to upload or replace a lyrics
/// page's cover/album art image. The cover file is tracked via <see cref="FileEntity" /> in
/// the Core module.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadLyricsCoverHandler(
    ILyricsRepository lyricsRepository,
    IFileUploadService fileUploadService,
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

        FileEntity uploaded = await fileUploadService.UploadImageAsync(
            file: file,
            publicId: command.LyricsId.ToString(),
            folder: "content/lyrics-covers",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileUploadService.RecordAsync(
                    file: uploaded,
                    supersededFileId: lyrics.CoverImageFileId,
                    cancellationToken: ct
                );

                lyrics.SetCoverImageFileId(coverImageFileId: uploaded.Id);

                lyricsRepository.Update(lyrics: lyrics);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadLyricsCoverResult(
            CoverImageUrl: uploaded.StorageUrl,
            CoverImageStorageKey: uploaded.StorageKey!
        );
    }
}
