using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Query for retrieving the full details of a video by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the video to retrieve.</param>
public record AdminGetVideoByIdQuery(string Id) : IQuery<AdminGetVideoByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetVideoByIdQuery" /> containing the full video details.
/// </summary>
/// <param name="Video">The detailed video information.</param>
public record AdminGetVideoByIdResult(VideoDetailDto Video);
