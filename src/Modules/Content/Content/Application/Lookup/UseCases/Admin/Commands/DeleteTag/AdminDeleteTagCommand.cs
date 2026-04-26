using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Command for permanently deleting a content tag.
/// </summary>
/// <param name="Id">The unique identifier of the tag to delete.</param>
public record AdminDeleteTagCommand(string Id) : ICommand<AdminDeleteTagResult>;

/// <summary>
/// Result of the <see cref="AdminDeleteTagCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteTagResult(bool IsSuccess);
