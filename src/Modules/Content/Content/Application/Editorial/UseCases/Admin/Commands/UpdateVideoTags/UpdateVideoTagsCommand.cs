using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Command for replacing the full set of tags assigned to a video.
/// All existing tag associations are removed and replaced with the provided tag set.
/// </summary>
/// <param name="VideoId">The unique identifier of the video whose tags are being updated.</param>
/// <param name="TagIds">The complete set of tag identifiers to assign to this video.</param>
public record UpdateVideoTagsCommand(string VideoId, IReadOnlyList<Guid> TagIds) : ICommand;
