using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Command for archiving a video, removing it from all public feeds without deleting it.
/// Archiving is reversible — Cloudinary thumbnail assets are not deleted.
/// </summary>
/// <param name="Id">The unique identifier of the video to archive.</param>
public record ArchiveVideoCommand(string Id) : ICommand;
