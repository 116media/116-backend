using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
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
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadLyricsCoverHandler(
    ILyricsRepository lyricsRepository,
    IFileRepository fileRepository,
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

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: lyrics.CoverImageFileId,
            file: file,
            publicId: command.LyricsId.ToString(),
            folder: "content/lyrics-covers",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        lyrics.SetCoverImageFileId(coverImageFileId: fileEntity.Id);

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadLyricsCoverResult(
            CoverImageUrl: fileEntity.StorageUrl,
            CoverImageStorageKey: fileEntity.StorageKey!
        );
    }
}
