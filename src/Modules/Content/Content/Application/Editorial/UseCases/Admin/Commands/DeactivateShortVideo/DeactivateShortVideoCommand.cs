using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Command for hiding a short video from the public feed.
/// </summary>
/// <param name="Id">The unique identifier of the short video to deactivate.</param>
public record DeactivateShortVideoCommand(string Id) : ICommand;
