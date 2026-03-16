using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Command for permanently deleting a video draft or rejected video.
/// Deletes the Cloudinary thumbnail asset before removing the database record.
/// Only <c>Draft</c> or <c>Rejected</c> videos can be permanently deleted.
/// </summary>
/// <param name="Id">The unique identifier of the video to delete.</param>
public record AdminDeleteVideoCommand(string Id) : ICommand<AdminDeleteVideoResult>;

/// <summary>
/// Result of the <see cref="AdminDeleteVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteVideoResult(bool IsSuccess);
