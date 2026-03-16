using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Command for archiving a video, removing it from all public feeds without deleting it.
/// Archiving is reversible — Cloudinary thumbnail assets are not deleted.
/// </summary>
/// <param name="Id">The unique identifier of the video to archive.</param>
public record AdminArchiveVideoCommand(string Id) : ICommand<AdminArchiveVideoResult>;

/// <summary>
/// Result of the <see cref="AdminArchiveVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveVideoResult(bool IsSuccess);
