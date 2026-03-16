using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Command for permanently deleting a short video and its associated media assets from cloud storage.
/// </summary>
/// <param name="Id">The unique identifier of the short video to delete.</param>
public record AdminDeleteShortVideoCommand(string Id) : ICommand<AdminDeleteShortVideoResult>;

/// <summary>
/// Result of the <see cref="AdminDeleteShortVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteShortVideoResult(bool IsSuccess);
