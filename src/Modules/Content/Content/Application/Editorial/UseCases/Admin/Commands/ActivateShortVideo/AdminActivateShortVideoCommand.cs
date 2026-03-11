using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Command for making a short video visible on the public feed.
/// </summary>
/// <param name="Id">The unique identifier of the short video to activate.</param>
public record AdminActivateShortVideoCommand(string Id) : ICommand<AdminActivateShortVideoResult>;

/// <summary>
/// Result of the <see cref="AdminActivateShortVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminActivateShortVideoResult(bool IsSuccess);
