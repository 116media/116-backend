using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Query for retrieving the full details of a short video by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the short video to retrieve.</param>
public record AdminGetShortByIdQuery(string Id) : IQuery<AdminGetShortByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetShortByIdQuery" /> containing the short video details.
/// </summary>
/// <param name="ShortVideo">The short video information.</param>
public record AdminGetShortByIdResult(ShortVideoDto ShortVideo);
