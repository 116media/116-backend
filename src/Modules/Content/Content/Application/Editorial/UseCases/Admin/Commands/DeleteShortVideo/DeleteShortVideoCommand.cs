using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Command for permanently deleting a short video and its associated media assets from cloud storage.
/// </summary>
/// <param name="Id">The unique identifier of the short video to delete.</param>
public record DeleteShortVideoCommand(string Id) : ICommand;
